using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using SharedScans.Interfaces;
using Reloaded.Mod.Interfaces;
using RyoTune.Reloaded;
using Reloaded.Hooks.Definitions;

namespace gbfr.utility.modtools.Hooks.Reflection;

public unsafe class ReflectionHooks : IHookBase
{
    private readonly ILogger _logger;

    private readonly static Dictionary<string, RelinkObjectType> _knownObjects = new();
    public int ObjectCount => _knownObjects.Count;
    public bool HasLoadedObjects { get; private set; } = false;

    private nint _charaParameterBaseList;

    private readonly Lock _lock = new();

    public static IHook<RegisterReflectionObjectDelegate> HOOK_ReflectionAddObject { get; private set; }

    public static IHook<CharaParameterBaseList__StaticCtor> FUNC_CharaParameterBaseList__StaticCtor { get; private set; }
    public static IHook<CharaParameterBaseList__GetByName> FUNC_CharaParameterBaseList__GetByName { get; private set; }

    public static IHook<RegisterBehaviorTreeComponentFactories> HOOK_RegisterBehaviorTreeComponentFactories { get; private set; }
    public static BehaviorTreeComponentObjectExists FUNC_BehaviorTreeComponentObjectExists { get; private set; }
    public static GetNewBehaviorTreeComponentByName FUNC_GetNewBehaviorTreeComponentByName { get; private set; }
    // TODO: Find the ISceneObject object creator by name so we can hook that too for getting default values.

    public delegate ulong RegisterReflectionObjectDelegate(ObjectDef* objectDef);

    public delegate ulong CharaParameterBaseList__StaticCtor(void* list, uint* nameHash, delegate* unmanaged<ulong*> createCallback);
    public delegate ulong CharaParameterBaseList__GetByName(nint list, nint outputPtr, string namePtr);

    public delegate nint RegisterBehaviorTreeComponentFactories(nint a1);
    public delegate bool BehaviorTreeComponentObjectExists(string name);
    public delegate nint GetNewBehaviorTreeComponentByName(string name);

    public ReflectionHooks(ILogger logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        // We hook this because character object params are created separately.
        
        Project.Scans.AddScanHook(nameof(CharaParameterBaseList__StaticCtor), (result, hooks)
            => FUNC_CharaParameterBaseList__StaticCtor = hooks.CreateHook<CharaParameterBaseList__StaticCtor>(CharaParameterBaseList__StaticCtorImpl, result).Activate());
        Project.Scans.AddScanHook(nameof(CharaParameterBaseList__GetByName), (result, hooks)
            => FUNC_CharaParameterBaseList__GetByName = hooks.CreateHook<CharaParameterBaseList__GetByName>(CharaParameterBaseList__GetByNameImpl, result).Activate());

        Project.Scans.AddScanHook(nameof(RegisterBehaviorTreeComponentFactories), (result, hooks)
            => HOOK_RegisterBehaviorTreeComponentFactories = hooks.CreateHook<RegisterBehaviorTreeComponentFactories>(ReflectionRegisterObjectFactoriesImpl, result).Activate());
        Project.Scans.AddScanHook(nameof(RegisterReflectionObjectDelegate), (result, hooks)
            => HOOK_ReflectionAddObject = hooks.CreateHook<RegisterReflectionObjectDelegate>(ss__reflection__AddObjectImpl, result).Activate());

        Project.Scans.AddScanHook(nameof(BehaviorTreeComponentObjectExists), (result, hooks)
            => FUNC_BehaviorTreeComponentObjectExists = hooks.CreateWrapper<BehaviorTreeComponentObjectExists>(result, out _));
        Project.Scans.AddScanHook(nameof(GetNewBehaviorTreeComponentByName), (result, hooks)
            => FUNC_GetNewBehaviorTreeComponentByName = hooks.CreateWrapper<GetNewBehaviorTreeComponentByName>(result, out _));
    }

    // This hooks the object registerer for all Param files i.e Em0001Param.
    // We need to hook it and grab an object from it, because otherwise we will never get the reflection info.
    // Reflection info is only created when the game needs an object, unlike the rest of all other reflected objects in the game i.e FSM.
    // nameHash is XXHash32
    private unsafe ulong CharaParameterBaseList__StaticCtorImpl(void* list, uint* nameHash, delegate* unmanaged<ulong*> createCallback)
    {
        var obj = createCallback(); // This will cause AddObject to be called, which we hook
        return FUNC_CharaParameterBaseList__StaticCtor.OriginalFunction(list, nameHash, createCallback);
    }

    private unsafe ulong CharaParameterBaseList__GetByNameImpl(nint listPtr, nint outputPtr, string name) // output ptr points to 0x00 object ptr, 0x08 = refcounter?
    {
        _charaParameterBaseList = listPtr;
        return FUNC_CharaParameterBaseList__GetByName.OriginalFunction(listPtr, outputPtr, name);
    }

    // Hook to know when all stuff has been registered. Otherwise ReflectionHasObjectByName will segfault.
    private nint ReflectionRegisterObjectFactoriesImpl(nint a1)
    {
        var res = HOOK_RegisterBehaviorTreeComponentFactories.OriginalFunction(a1);
        HasLoadedObjects = true;
        return res;
    }


    // This hooks the function that registers a new object to be reflected.
    private unsafe ulong ss__reflection__AddObjectImpl(ObjectDef* objectDef)
    {
        var res = HOOK_ReflectionAddObject.OriginalFunction(objectDef);

        lock (_knownObjects)
        {
            string objectName = Marshal.PtrToStringAnsi((nint)objectDef->pObjectType->pName);
            if (!_knownObjects.ContainsKey(objectName))
            {
                RegisterType(objectDef->pObjectType);
                //_logger.WriteLine($"Registered new reflected object: {objectName}");
            }
        }

        return res;
    }

    public void DumpAll()
    {
        // iterate in steps, as dumping may append to _knownObjects.
        int i = 0;
        while (true)
        {
            int n = _knownObjects.Count;
            for (; i < n; i++)
            {
                var objInfo = _knownObjects.ElementAt(i);
                DumpObjectReflectionInfo(objInfo.Value);
            }

            if (i == n)
                break;
        }
    }

    private void RegisterType(IObjectType* objectType)
    {
        string objectName = Marshal.PtrToStringAnsi((nint)objectType->pName);
        string inheritName = Marshal.PtrToStringAnsi((nint)objectType->pTypeName);

        var relinkObjectType = new RelinkObjectType();
        relinkObjectType.Name = objectName;
        relinkObjectType.InheritName = inheritName;
        relinkObjectType.ReflectionTypeInfoPtr = objectType;

        RelinkObjectType baseObj = null;
        if (!string.IsNullOrEmpty(relinkObjectType.InheritName))
        {
            _knownObjects.TryGetValue(relinkObjectType.InheritName, out baseObj);
        }

        IAttributeList* attrList = objectType->pAttrList;
        int count = (int)(attrList->pEnd - attrList->pBegin);
        for (int i = 0; i < count; i++)
        {
            IAttribute* attr = attrList->pBegin[i];

            if (attr->field_0x08 == 1) // Swapped? weird
            {
                var attrName = Marshal.PtrToStringAnsi((nint)attr->pTypeName);
                var attrTypeName = Marshal.PtrToStringAnsi((nint)attr->pAttrName);

                if (HasAttribute(baseObj, attrName))
                    continue;

                relinkObjectType.Attributes.TryAdd(attrName, new RelinkObjectAttribute(attrName, attrTypeName, attr));
            }
            else
            {
                var attrName = Marshal.PtrToStringAnsi((nint)attr->pAttrName);
                var attrTypeName = Marshal.PtrToStringAnsi((nint)attr->pTypeName);

                if (HasAttribute(baseObj, attrName))
                    continue;

                relinkObjectType.Attributes.TryAdd(attrName, new RelinkObjectAttribute(attrName, attrTypeName, attr));
            }
        }

        _knownObjects.Add(objectName, relinkObjectType);
    }

    private bool HasAttribute(RelinkObjectType baseObj, string name)
    {
        if (baseObj is not null)
        {
            if (baseObj.Attributes.ContainsKey(name))
                return true;

            if (!string.IsNullOrEmpty(baseObj.InheritName) && _knownObjects.TryGetValue(baseObj.InheritName, out baseObj))
            {
                return HasAttribute(baseObj, name);
            }
        }

        return false;
    }

    private void DumpObjectReflectionInfo(RelinkObjectType objectType)
    {
        nint defaultObjectPtr = 0;
        if (HasLoadedObjects && FUNC_BehaviorTreeComponentObjectExists(objectType.Name))
        {
            // This may create nested objects.
            defaultObjectPtr = FUNC_GetNewBehaviorTreeComponentByName(objectType.Name);
        }
        else if (_charaParameterBaseList != 0 && objectType.Name.EndsWith("Param"))
        {
            nint charaPtr = Marshal.AllocHGlobal(0x10);
            FUNC_CharaParameterBaseList__GetByName.OriginalFunction(_charaParameterBaseList, charaPtr, objectType.Name);
            if (*(nint*)charaPtr != 0)
                defaultObjectPtr = *(nint*)charaPtr;

            Marshal.FreeHGlobal(charaPtr);
        }

        if (!string.IsNullOrEmpty(objectType.InheritName))
            _logger.WriteLine($"public class {objectType.Name} : {objectType.InheritName}");
        else
            _logger.WriteLine($"public class {objectType.Name}");

        _logger.WriteLine("{");

        RelinkObjectType inheritType = null;

        _logger.WriteLine("    [JsonIgnore]");
        _logger.WriteLine($"    public override string ComponentName => nameof({objectType.Name});");
        _logger.WriteLine("");

        foreach (KeyValuePair<string, RelinkObjectAttribute> attr in objectType.Attributes)
        {
            if (inheritType is null || !inheritType.Attributes.ContainsKey(attr.Key))
            {
                AddProperty(attr.Value, defaultObjectPtr);
            }
        }

        // Generate constructor with attributes from inheriting class
        _logger.WriteLine($"    public {objectType.Name}()");
        _logger.WriteLine("    {");

        foreach (KeyValuePair<string, RelinkObjectAttribute> attr in objectType.Attributes)
        {
            if (inheritType is not null && inheritType.Attributes.ContainsKey(attr.Key))
            {
                string valStr = GetValueStr(attr.Value, defaultObjectPtr);
                if (!string.IsNullOrEmpty(valStr))
                {
                    string humanizedAttrName = attr.Key;
                    if (attr.Key.EndsWith('_'))
                    {
                        humanizedAttrName = attr.Key.Substring(0, attr.Key.Length - 1);
                        humanizedAttrName = FirstCharToUpper(humanizedAttrName);
                    }

                    _logger.WriteLine($"        {humanizedAttrName}{GetValueStr(attr.Value, defaultObjectPtr)}");
                }
            }
        }
        _logger.WriteLine("    }");

        _logger.WriteLine("}");
        _logger.WriteLine("\n");
    }

    private void AddProperty(RelinkObjectAttribute attribute, nint defaultObjectPtr)
    {
        string humanizedAttrName = attribute.Name;
        if (attribute.Name.EndsWith('_'))
            humanizedAttrName = attribute.Name.Substring(0, attribute.Name.Length - 1);

        humanizedAttrName = FirstCharToUpper(humanizedAttrName);
        string valueStr = GetValueStr(attribute, defaultObjectPtr);

        _logger.WriteLine($"    [JsonPropertyName(\"{attribute.Name}\")]");
        _logger.WriteLine($"    public {TypeToCSharpType(attribute.Type)} {humanizedAttrName} {{ get; set; }}{valueStr} " +
            $"// Offset 0x{(attribute.AttributePtr->field_0x08 == 1 ? attribute.AttributePtr->field_0x38 : attribute.AttributePtr->dwOffset):X}");

        _logger.WriteLine("");
        //_logWindow.Log(nameof(ReflectionDumper), $"    {attrTypeName} {humanizedAttrName}; // 0x{XXHash32Custom.Hash(humanizedAttrName):X8}");
        //_logWindow.Log(nameof(ReflectionDumper), ($"    {attrTypeName} {attrName}; // Offset 0x{attr->dwOffset:X}, 0x08:{attr->field_0x08}, 0x38:{attr->field_0x38}"));

    }

    private static string GetValueStr(RelinkObjectAttribute attribute, nint defaultObjectPtr)
    {
        string csharpTypeName = TypeToCSharpType(attribute.Type);
        string valueStr = "";
        if (defaultObjectPtr != 0)
        {
            nint fieldOffset = defaultObjectPtr + attribute.AttributePtr->dwOffset;
            switch (csharpTypeName)
            {
                case "bool":
                    {
                        bool val = *(bool*)fieldOffset;
                        valueStr = $" = {val.ToString().ToLower()};";
                    }
                    break;
                case "short":
                    {
                        short val = *(short*)fieldOffset;
                        valueStr = $" = {val};";
                    }
                    break;
                case "sbyte":
                    {
                        sbyte val = *(sbyte*)fieldOffset;
                        valueStr = $" = {val};";
                    }
                    break;
                case "byte":
                    {
                        byte val = *(byte*)fieldOffset;
                        valueStr = $" = {val};";
                    }
                    break;
                case "float":
                    {
                        float val = *(float*)fieldOffset;
                        valueStr = $" = {val}f;";
                    }
                    break;
                case "int":
                    {
                        int val = *(int*)fieldOffset;
                        valueStr = $" = {val};";
                    }
                    break;
                case "uint":
                    {
                        uint val = *(uint*)fieldOffset;
                        if (val == 0xFFFFFFFF)
                            valueStr = $" = 0x{val:X};";
                        else
                            valueStr = $" = {val};";
                    }
                    break;
                case "Vector4":
                    {
                        Vector4 val = *(Vector4*)fieldOffset;
                        valueStr = $" = new Vector4({val.X}f, {val.Y}f, {val.Z}f, {val.W}f);";
                    }
                    break;
                case "Vector3":
                    {
                        Vector3 val = *(Vector3*)fieldOffset;
                        valueStr = $" = new Vector3({val.X}f, {val.Y}f, {val.Z}f);";
                    }
                    break;
                case "Vector2":
                    {
                        Vector2 val = *(Vector2*)fieldOffset;
                        valueStr = $" = new Vector2({val.X}f, {val.Y}f);";
                    }
                    break;
            }
        }

        return valueStr;
    }

    public static string TypeToCSharpType(string type)
    {
        return type switch
        {
            "bool" => "bool",
            "cVec4" => "Vector4",
            "cVec3" => "Vector3",
            "cVec2" => "Vector2",
            "s16" => "short",
            "s8" => "sbyte",
            "u8" => "byte",
            "f32" => "float",
            "s32" => "int",
            "u32" => "uint",
            _ => type,
        };
    }
    public static string FirstCharToUpper(string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
}

public class RelinkObjectType
{
    public string Name { get; set; }
    public string InheritName { get; set; }

    public unsafe IObjectType* ReflectionTypeInfoPtr { get; set; }
    public Dictionary<string, RelinkObjectAttribute> Attributes { get; set; } = [];
}

public class RelinkObjectAttribute
{
    public string Name { get; set; }
    public string Type { get; set; }
    public unsafe IAttribute* AttributePtr { get; set; }

    public unsafe RelinkObjectAttribute(string name, string type, IAttribute* attributePtr)
    {
        Name = name;
        Type = type;
        AttributePtr = attributePtr;
    }

}
