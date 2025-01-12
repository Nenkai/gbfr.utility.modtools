﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

using gbfr.utility.modtools.ImGuiSupport.Windows;
using SharedScans.Interfaces;

namespace gbfr.utility.modtools.Hooks;

public unsafe class ReflectionHooks
{
    private readonly static Dictionary<string, nint> _knownObjects = new();
    public int ObjectCount => _knownObjects.Count;
    public bool HasLoadedObjects { get; private set; } = false;

    private nint _charaParameterBaseList;

    private readonly Lock _lock = new();

    private readonly ISharedScans _scans;
    private readonly LogWindow _logWindow;

    public static HookContainer<CharaParameterBaseList__StaticCtor> FUNC_CharaParameterBaseList__StaticCtor { get; private set; }
    public static HookContainer<CharaParameterBaseList__GetByName> FUNC_CharaParameterBaseList__GetByName { get; private set; }

    public static HookContainer<ReflectionRegisterObjectFactories> HOOK_ReflectionRegisterObjectFactories { get; private set; }
    public static HookContainer<RegisterReflectionObjectDelegate> HOOK_ReflectionAddObject { get; private set; }
    public static WrapperContainer<ReflectionHasObjectByName> FUNC_ReflectionHasObjectByName { get; private set; }
    public static WrapperContainer<ReflectionCreateObjectByName> FUNC_ReflectionCreateObjectByName { get; private set; }

    public unsafe delegate ulong CharaParameterBaseList__StaticCtor(void* list, uint* nameHash, delegate* unmanaged<ulong*> createCallback);
    public unsafe delegate ulong CharaParameterBaseList__GetByName(nint list, nint outputPtr, string namePtr);

    public unsafe delegate nint ReflectionRegisterObjectFactories(nint a1);
    public unsafe delegate ulong RegisterReflectionObjectDelegate(ObjectDef* objectDef);
    public unsafe delegate bool ReflectionHasObjectByName(string name);
    public unsafe delegate nint ReflectionCreateObjectByName(string name);

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(CharaParameterBaseList__StaticCtor)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 83 EC ?? 48 8D 6C 24 ?? 48 C7 45 ?? ?? ?? ?? ?? 4C 89 C7 48 89 D3 48 89 CE 48 8D 0D ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 05 ?? ?? ?? ?? 65 48 8B 0C 25 ?? ?? ?? ?? 48 8B 04 C1 48 8B 88 ?? ?? ?? ?? 48 8B 41 ?? 4C 8B 40 ?? 4D 85 C0 0F 84 ?? ?? ?? ?? FF 40 ?? 49 8B 08 48 89 48 ?? 8B 0B 41 89 48 ?? 49 89 78 ?? 0F B6 C1 49 BF ?? ?? ?? ?? ?? ?? ?? ?? 4C 31 F8 49 BD ?? ?? ?? ?? ?? ?? ?? ?? 49 0F AF C5 0F B6 D5 48 31 C2 49 0F AF D5 89 C8 C1 E8 ?? 0F B6 C0 48 31 D0 49 0F AF C5 49 89 CC 49 C1 EC ?? 49 31 C4 4D 0F AF E5 48 8B 15 ?? ?? ?? ?? 4C 21 E2 48 8B 1D ?? ?? ?? ?? 48 89 D0 48 C1 E0 ?? 48 8B 44 03 ?? 4C 8B 35 ?? ?? ?? ?? 4C 39 F0 0F 84 ?? ?? ?? ?? 3B 48 ?? 75 ?? 48 89 06 C6 46 ?? ?? EB ?? 48 01 D2 48 8B 14 D3 90 48 39 D0 0F 84 ?? ?? ?? ?? 48 8B 40 ?? 3B 48 ?? 75 ?? 48 89 06 C6 46 ?? ?? 4D 85 C0 0F 84 ?? ?? ?? ?? 4C 89 C0 48 25 ?? ?? ?? ?? 0F 84 ?? ?? ?? ?? 65 48 8B 1C 25 ?? ?? ?? ?? 44 89 C2 81 E2 ?? ?? ?? ?? 0F B6 48 ?? 48 D3 EA 48 3B 58 ?? 0F 85 ?? ?? ?? ?? 48 C1 E2 ?? 80 7C 10 ?? ?? 0F 85 ?? ?? ?? ?? 48 8B 8C 10 ?? ?? ?? ?? 49 89 08 4C 89 84 10 ?? ?? ?? ?? FF 8C 10 ?? ?? ?? ?? 0F 85 ?? ?? ?? ?? 48 8D 0C 10 48 83 C1 ?? 48 83 C4 ?? 5B 5F 5E 41 5C 41 5D 41 5E 41 5F 5D E9 ?? ?? ?? ?? 48 89 D0 48 8B 0D ?? ?? ?? ?? 48 BA ?? ?? ?? ?? ?? ?? ?? ?? 48 39 D1 0F 84 ?? ?? ?? ?? 48 FF C1 78 ?? C4 E1 FA 2A C1 EB ?? 48 89 CA 48 D1 EA 83 E1 ?? 48 09 D1 C4 E1 FA 2A C1 C5 FA 58 C0 C5 FA 10 0D ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 85 D2 78 ?? C4 E1 EA 2A D2 EB ?? 48 89 D3 48 D1 EB 89 D1 83 E1 ?? 48 09 D9 C4 E1 EA 2A D1 C5 EA 58 D2 C5 FA 5E D2 C5 F8 2E D1 0F 86 ?? ?? ?? ?? C5 FA 5E C1 C4 E3 79 0A C0 ?? C4 E1 FA 2C C0 48 89 C1 C5 FA 5C 05 ?? ?? ?? ?? C4 E1 FA 2C D8 48 C1 F9 ?? 48 21 CB 48 09 C3 48 83 FB ?? B8 ?? ?? ?? ?? 48 0F 43 C3 48 39 C2 73 ?? 48 8D 0C D5 ?? ?? ?? ?? 48 39 C1 48 0F 42 C8 48 81 FA ?? ?? ?? ?? 48 0F 43 C8 48 89 CA 48 89 75 ?? 4C 89 45 ?? 48 B8 ?? ?? ?? ?? ?? ?? ?? ?? 48 39 C2 0F 83 ?? ?? ?? ?? 48 FF CA 48 83 CA ?? 48 0F BD CA 83 F1 ?? F6 D9 BE ?? ?? ?? ?? 48 D3 E6 B8 ?? ?? ?? ?? 48 D3 E0 48 89 C1 4C 89 F2 E8 ?? ?? ?? ?? 48 8D 56 ?? 48 89 15 ?? ?? ?? ?? 48 89 35 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 4C 8B 08 4D 39 F1 75 ?? E9 ?? ?? ?? ?? 66 66 66 66 66 2E 0F 1F 84 00 ?? ?? ?? ?? 4A 89 04 C2 48 89 04 FA 4D 39 F1 0F 84 ?? ?? ?? ?? 4C 89 C8 41 0F B6 49 ?? 4C 31 F9 49 0F AF CD 41 0F B6 51 ?? 48 31 CA 49 0F AF D5 41 0F B6 49 ?? 48 31 D1 49 0F AF CD 41 0F B6 59 ?? 48 31 CB 49 0F AF DD 48 23 1D ?? ?? ?? ?? 4D 8B 09 48 8B 15 ?? ?? ?? ?? 4C 8D 04 1B 48 8D 3C 1B 48 FF C7 48 C1 E3 ?? 48 8B 34 1A 4C 39 F6 74 ?? 48 8B 1C FA 8B 48 ?? 3B 4B ?? 75 ?? 48 8B 0B 48 39 C1 74 ?? 4C 8B 40 ?? 4D 89 08 49 8B 71 ?? 48 89 0E 48 8B 59 ?? 48 89 03 48 89 71 ?? 4D 89 41 ?? 48 89 58 ?? E9 ?? ?? ?? ?? 66 0F 1F 84 00 ?? ?? ?? ?? 48 39 DE 74 ?? 48 8B 5B ?? 3B 4B ?? 75 ?? 48 8B 0B 48 8B 50 ?? 4C 89 0A 49 8B 59 ?? 48 89 0B 48 8B 79 ?? 48 89 07 48 89 59 ?? 49 89 51 ?? 48 89 78 ?? E9 ?? ?? ?? ?? 48 8B 48 ?? 4C 89 09 49 8B 59 ?? 48 89 33 48 8B 7E ?? 48 89 07 48 89 5E ?? 49 89 49 ?? 48 89 78 ?? 4A 89 04 C2 E9 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 4C 21 E2 48 8B 3D ?? ?? ?? ?? 48 89 D1 48 C1 E1 ?? 48 8B 4C 0F ?? 48 39 C1 4C 8B 45 ?? 48 8B 75 ?? 74 ?? 41 8B 58 ?? 3B 59 ?? 74 ?? 48 01 D2 48 8B 04 D7 66 66 66 66 66 2E 0F 1F 84 00 ?? ?? ?? ?? 48 39 C1 74 ?? 48 8B 49 ?? 3B 59 ?? 75 ?? 48 8B 01 48 8B 48 ?? 48 FF 05 ?? ?? ?? ?? 49 89 00 49 89 48 ?? 4C 89 01 4C 89 40 ?? 48 8B 3D ?? ?? ?? ?? 4C 23 25 ?? ?? ?? ?? 49 C1 E4 ?? 4A 8D 14 27 4A 8D 1C 27 48 83 C3 ?? 4A 8B 3C 27 48 3B 3D ?? ?? ?? ?? 74 ?? 48 39 C7 74 ?? 48 89 DA 48 39 0B 75 ?? EB ?? 4C 89 02 48 89 DA 4C 89 02 4C 89 06 C6 46 ?? ?? 48 83 C4 ?? 5B 5F 5E 41 5C 41 5D 41 5E 41 5F 5D C3 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 49 89 C0 E9 ?? ?? ?? ?? 48 3B 58 ?? 0F 94 C2 48 89 C1 48 83 C4 ?? 5B 5F 5E 41 5C 41 5D 41 5E 41 5F 5D E9 ?? ?? ?? ?? 4C 89 45 ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? CC 66 66 66 66 2E 0F 1F 84 00 ?? ?? ?? ?? 48 89 54 24 ?? 55 41 57 41 56 41 55 41 54 56 57 53 48 83 EC ?? 48 8D 6A ?? 48 8B 4D ?? E8 ?? ?? ?? ?? 90 48 83 C4 ?? 5B 5F 5E 41 5C 41 5D 41 5E 41 5F 5D C3 CC CC CC CC CC CC CC CC CC CC CC CC 56 57 53 48 83 EC ?? 48 89 D6 48 89 CB 48 8B 05 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 49 89 C2 49 29 CA 4C 89 D2 48 C1 FA ?? 48 39 DA 73 ?? 48 8D 0D ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0C DD ?? ?? ?? ?? 48 85 C9 0F 84 ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 C7 E9 ?? ?? ?? ?? 48 39 C1 0F 84 ?? ?? ?? ?? 49 83 C2 ?? 49 83 FA ?? 0F 82 ?? ?? ?? ?? 49 C1 EA ?? 49 FF C2 4D 89 D0 49 83 E0 ?? C4 E1 F9 6E C6 C5 F9 70 C0 ?? C4 E3 7D 18 C0 ?? 49 8D 50 ?? 49 89 D1 49 C1 E9 ?? 49 FF C1 48 85 D2 0F 84 ?? ?? ?? ?? 4C 89 CB 48 83 E3 ?? 31 FF 0F 1F 84 00 ?? ?? ?? ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 04 39 C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? C4 E3 7D 19 84 39 ?? ?? ?? ?? ?? C5 F8 11 84 39 ?? ?? ?? ?? C4 E3 7D 19 84 39 ?? ?? ?? ?? ?? C5 F8 11 84 39 ?? ?? ?? ?? C4 E3 7D 19 84 39 ?? ?? ?? ?? ?? C5 F8 11 84 39 ?? ?? ?? ?? C4 E3 7D 19 84 39 ?? ?? ?? ?? ?? C5 F8 11 84 39 ?? ?? ?? ?? 48 81 C7 ?? ?? ?? ?? 48 83 C3 ?? 0F 85 ?? ?? ?? ?? 41 F6 C1 ?? 74 ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 04 39 C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? C4 E3 7D 19 44 39 ?? ?? C5 F8 11 44 39 ?? 4D 39 C2 0F 84 ?? ?? ?? ?? 4A 8D 0C C1 66 66 66 66 66 2E 0F 1F 84 00 ?? ?? ?? ?? 48 89 31 48 83 C1 ?? 48 39 C1 75 ?? E9 ?? ?? ?? ?? 31 FF 4C 8B 05 ?? ?? ?? ?? 4C 39 05 ?? ?? ?? ?? 74 ?? 4D 85 C0 74 ?? 4C 89 C0 48 25 ?? ?? ?? ?? 74 ?? 65 4C 8B 0C 25 ?? ?? ?? ?? 44 89 C2 81 E2 ?? ?? ?? ?? 0F B6 48 ?? 48 D3 EA 4C 3B 48 ?? 0F 85 ?? ?? ?? ?? 48 C1 E2 ?? 80 7C 10 ?? ?? 0F 85 ?? ?? ?? ?? 48 8B 8C 10 ?? ?? ?? ?? 49 89 08 4C 89 84 10 ?? ?? ?? ?? FF 8C 10 ?? ?? ?? ?? 0F 84 ?? ?? ?? ?? 48 89 3D ?? ?? ?? ?? 48 8D 04 DF 48 89 05 ?? ?? ?? ?? 48 89 05 ?? ?? ?? ?? 48 FF CB 48 B9 ?? ?? ?? ?? ?? ?? ?? ?? 48 21 D9 48 83 F9 ?? 0F 82 ?? ?? ?? ?? 48 FF C1 49 89 C8 49 83 E0 ?? C4 E1 F9 6E C6 C5 F9 70 C0 ?? C4 E3 7D 18 C0 ?? 49 8D 50 ?? 49 89 D1 49 C1 E9 ?? 49 FF C1 48 85 D2 0F 84 ?? ?? ?? ?? 4C 89 CA 48 83 E2 ?? 31 DB 66 66 66 66 66 2E 0F 1F 84 00 ?? ?? ?? ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 04 1F C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? C4 E3 7D 19 84 1F ?? ?? ?? ?? ?? C5 F8 11 84 1F ?? ?? ?? ?? C4 E3 7D 19 84 1F ?? ?? ?? ?? ?? C5 F8 11 84 1F ?? ?? ?? ?? C4 E3 7D 19 84 1F ?? ?? ?? ?? ?? C5 F8 11 84 1F ?? ?? ?? ?? C4 E3 7D 19 84 1F ?? ?? ?? ?? ?? C5 F8 11 84 1F ?? ?? ?? ?? 48 81 C3 ?? ?? ?? ?? 48 83 C2 ?? 0F 85 ?? ?? ?? ?? 41 F6 C1 ?? 74 ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 04 1F C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? C4 E3 7D 19 44 1F ?? ?? C5 F8 11 44 1F ?? 4C 39 C1 74 ?? 4A 8D 3C C7 66 90 48 89 37 48 83 C7 ?? 48 39 C7 75 ?? 48 83 C4 ?? 5B 5F 5E C5 F8 77 C3 31 DB 41 F6 C1 ?? 75 ?? EB ?? 31 FF 41 F6 C1 ?? 0F 85 ?? ?? ?? ?? E9 ?? ?? ?? ?? 4C 3B 48 ?? 0F 94 C2 48 89 C1 E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0C 10 48 83 C1 ?? E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC 55",
        [nameof(CharaParameterBaseList__GetByName)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 F8 29 B5 ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 4C 89 C6 49 89 D6",

        [nameof(ReflectionRegisterObjectFactories)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CE 8B 05 ?? ?? ?? ?? 65 48 8B 0C 25 ?? ?? ?? ?? 48 8B 04 C1 48 8B 88 ?? ?? ?? ?? 48 8B 81",
        [nameof(RegisterReflectionObjectDelegate)] = "55 41 57 41 56 41 54 56 57 53 48 83 EC ?? 48 8D 6C 24 ?? 48 C7 45 ?? ?? ?? ?? ?? 49 89 CF 48 8D 0D",
        [nameof(ReflectionHasObjectByName)] = "56 48 83 EC ?? E8 ?? ?? ?? ?? 4C 8B 0D",
        [nameof(ReflectionCreateObjectByName)] = "56 48 83 EC ?? E8 ?? ?? ?? ?? 4C 8B 05",
    };

    public ReflectionHooks(ISharedScans scans, LogWindow window)
    {
        _scans = scans;
        _logWindow = window;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        // We hook this because character object params are created separately.
        FUNC_CharaParameterBaseList__StaticCtor = _scans.CreateHook<CharaParameterBaseList__StaticCtor>(CharaParameterBaseList__StaticCtorImpl, "a");
        FUNC_CharaParameterBaseList__GetByName = _scans.CreateHook<CharaParameterBaseList__GetByName>(CharaParameterBaseList__GetByNameImpl, "a");

        HOOK_ReflectionRegisterObjectFactories = _scans.CreateHook<ReflectionRegisterObjectFactories>(ReflectionRegisterObjectFactoriesImpl, "a");
        HOOK_ReflectionAddObject = _scans.CreateHook<RegisterReflectionObjectDelegate>(ss__reflection__AddObjectImpl, "a");
        FUNC_ReflectionHasObjectByName = _scans.CreateWrapper<ReflectionHasObjectByName>("a");
        FUNC_ReflectionCreateObjectByName = _scans.CreateWrapper<ReflectionCreateObjectByName>("a");
    }

    // This hooks the object registerer for all Param files i.e Em0001Param.
    // We need to hook it and grab an object from it, because otherwise we will never get the reflection info.
    // Reflection info is only created when the game needs an object, unlike the rest of all other reflected objects in the game i.e FSM.
    // nameHash is XXHash32
    private unsafe ulong CharaParameterBaseList__StaticCtorImpl(void* list, uint* nameHash, delegate* unmanaged<ulong*> createCallback)
    {
        var obj = createCallback(); // This will cause AddObject to be called, which we hook
        return FUNC_CharaParameterBaseList__StaticCtor.Hook.OriginalFunction(list, nameHash, createCallback);
    }

    private unsafe ulong CharaParameterBaseList__GetByNameImpl(nint listPtr, nint outputPtr, string name) // output ptr points to 0x00 object ptr, 0x08 = refcounter?
    {
        _charaParameterBaseList = listPtr;
        return FUNC_CharaParameterBaseList__GetByName.Hook.OriginalFunction(listPtr, outputPtr, name);
    }

    // Hook to know when all stuff has been registered. Otherwise ReflectionHasObjectByName will segfault.
    private nint ReflectionRegisterObjectFactoriesImpl(nint a1)
    {
        var res = HOOK_ReflectionRegisterObjectFactories.Hook.OriginalFunction(a1);
        HasLoadedObjects = true;
        return res;
    }


    // This hooks the function that registers a new object to be reflected.
    private unsafe ulong ss__reflection__AddObjectImpl(ObjectDef* objectDef)
    {
        var res = HOOK_ReflectionAddObject.Hook.OriginalFunction(objectDef);

        lock (_knownObjects)
        {
            string objectName = Marshal.PtrToStringAnsi((nint)objectDef->pObjectType->pName);
            if (!_knownObjects.ContainsKey(objectName))
            {
                _knownObjects.Add(objectName, (nint)objectDef->pObjectType);
                _logWindow.Log(nameof(ReflectionHooks), $"Registered new reflected object: {objectName}");
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
                DumpObjectReflectionInfo((IObjectType*)objInfo.Value);
            }

            if (i == n)
                break;
        }
    }

    private void DumpObjectReflectionInfo(IObjectType* objectDef)
    {
        string objectName = Marshal.PtrToStringAnsi((nint)objectDef->pName);
        string typeName = Marshal.PtrToStringAnsi((nint)objectDef->pTypeName);

        nint defaultObjectPtr = 0;
        if (HasLoadedObjects && FUNC_ReflectionHasObjectByName.Wrapper(objectName))
        {
            // This may create nested objects.
            defaultObjectPtr = FUNC_ReflectionCreateObjectByName.Wrapper(objectName);
        }
        else if (_charaParameterBaseList != 0 && objectName.EndsWith("Param"))
        {
            nint charaPtr = Marshal.AllocHGlobal(0x10);
            FUNC_CharaParameterBaseList__GetByName.Hook.OriginalFunction(_charaParameterBaseList, charaPtr, objectName);
            if (*(nint*)charaPtr != 0)
                defaultObjectPtr = *(nint*)charaPtr;

            Marshal.FreeHGlobal(charaPtr);
        }

        if (!string.IsNullOrEmpty(typeName))
            _logWindow.Log(nameof(ReflectionHooks), $"public class {objectName} : {typeName}");
        else
            _logWindow.Log(nameof(ReflectionHooks), $"public class {objectName}");

        _logWindow.Log(nameof(ReflectionHooks), "{");

        IAttributeList* attrList = objectDef->pAttrList;
        int count = (int)(attrList->pEnd - attrList->pBegin);
        for (int i = 0; i < count; i++)
        {
            IAttribute* attr = attrList->pBegin[i];

            if (attr->field_0x08 == 1) // Swapped? weird
            {
                var attrName = Marshal.PtrToStringAnsi((nint)attr->pTypeName);
                var attrTypeName = Marshal.PtrToStringAnsi((nint)attr->pAttrName);

                AddLog(attrTypeName, attrName, defaultObjectPtr, attr);
            }
            else
            {
                var attrName = Marshal.PtrToStringAnsi((nint)attr->pAttrName);
                var attrTypeName = Marshal.PtrToStringAnsi((nint)attr->pTypeName);

                AddLog(attrTypeName, attrName, defaultObjectPtr, attr);
            }
        }
        _logWindow.Log(nameof(ReflectionHooks), "}");
        _logWindow.Log(nameof(ReflectionHooks), "\n");
    }

    private void AddLog(string typeName, string attrName, nint defaultObjectPtr, IAttribute* attr)
    {
        string humanizedAttrName = attrName;
        if (attrName.EndsWith('_'))
        {
            humanizedAttrName = attrName.Substring(0, attrName.Length - 1);
            humanizedAttrName = FirstCharToUpper(humanizedAttrName);
        }

        string csharpTypeName = TypeToCSharpType(typeName);
        string valueStr = "";
        if (defaultObjectPtr != 0)
        {
            nint fieldOffset = defaultObjectPtr + attr->dwOffset;
            switch (csharpTypeName)
            {
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
            }
        }

        _logWindow.Log(nameof(ReflectionHooks), $"    [JsonPropertyName(\"{attrName}\")]");
        _logWindow.Log(nameof(ReflectionHooks), $"    public {TypeToCSharpType(typeName)} {humanizedAttrName} {{ get; set; }}{valueStr}");
        _logWindow.Log(nameof(ReflectionHooks), "");
        //_logWindow.Log(nameof(ReflectionDumper), $"    {attrTypeName} {humanizedAttrName}; // 0x{XXHash32Custom.Hash(humanizedAttrName):X8}");
        //_logWindow.Log(nameof(ReflectionDumper), ($"    {attrTypeName} {attrName}; // Offset 0x{attr->dwOffset:X}, 0x08:{attr->field_0x08}, 0x38:{attr->field_0x38}"));

    }
    public static string TypeToCSharpType(string type)
    {
        return type switch
        {
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

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
public unsafe struct ObjectDef
{
    [FieldOffset(0x00)]
    public byte* pName;

    [FieldOffset(0x08)]
    public uint dwHash;

    [FieldOffset(0x10)]
    public IObjectType* pObjectType;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IObjectType
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public byte* pName;

    [FieldOffset(0x28)]
    public IAttributeList* pAttrList;

    [FieldOffset(0x50)]
    public byte* pTypeName;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttributeList
{
    [FieldOffset(0x00)]
    public IAttribute** pBegin;

    [FieldOffset(0x08)]
    public IAttribute** pEnd;

    [FieldOffset(0x10)]
    public IAttribute** pCapacity;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct IAttribute
{
    [FieldOffset(0)]
    public void* pVtable;

    [FieldOffset(0x08)]
    public int field_0x08;

    [FieldOffset(0x10)]
    public byte* pAttrName;

    [FieldOffset(0x18)]
    public byte* pTypeName;

    [FieldOffset(0x20)]
    public int dwOffset;

    [FieldOffset(0x24)]
    public int field_0x24;

    [FieldOffset(0x28)]
    public int field_0x28;

    [FieldOffset(0x2C)]
    public int field_0x2C;

    [FieldOffset(0x30)]
    public int field_0x30;

    [FieldOffset(0x34)]
    public int field_0x34;

    [FieldOffset(0x38)]
    public int field_0x38;
}
