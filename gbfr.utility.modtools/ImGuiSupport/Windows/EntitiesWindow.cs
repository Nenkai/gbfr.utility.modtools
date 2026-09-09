using gbfr.utility.modtools.Hooks.Behavior;
using gbfr.utility.modtools.Structs;

using NenTools.ImGui.Interfaces;
using NenTools.ImGui.Interfaces.Shell;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.ImGuiSupport.Windows;

public unsafe class EntitiesWindow : IImGuiComponent
{
    private readonly IImGui _imGui;
    private readonly EntityHooks _entityHooks;

    public bool IsOverlay => false;
    public bool IsOpen = false;

    private bool _showHateParams = false;
    private bool _showTargetHateParams = false;

    private ConcurrentDictionary<uint, uint> _currentTargets = [];

    public Dictionary<ulong, string> HateParamNames = new()
    {
        [0x24A0FD4D33968F07] = "AttackHateClose", // - hateRateClosePlayer_",
        [0xA90504BAF52F01A5u] = "AttackHateFar", //  - hateRateFarPlayer_",
        [0xC22A8882B0DBAA68u] = "AttackHateClosePerSec", //  - hateRateClosePlayerPerSec_",
        [0x216458528ADF1013] = "AttackHateFarPerSec", //  - hateRateFarPlayerPerSec_",
        [0x7EFAB85DFA46128F] = "AttackHateFront", //  - hateRateFrontAngle_",
        [0x2EE90B3899F1F1C] = "AttackHateBack", //  - hateRateBackAngle_",
        [0xA18BFDA170EAD1A7u] = "AttackHateDamage", //  - hateRateDamage_",
        [0xD9CCA257AEDA9D2] = "AttackHateLowHp", //  - hateRateLowHpPlayer_",
        [0xE11A8EEB4277BEE9u] = "AttackHateHighHp", //  - hateRateHighHpPlayer_",
        [0x1A89C1E9C30E9625] = "AttackHateManualPlayer", //  - hateRateManualPlayer_",
        [0xEE6BF56811C47336u] = "AttackHateHelpPlayer", //  - hateRateHelpPlayer_",
        [0x31DD2B4D5DF84A4] = "AttackHateSlowAilment", //  - buff or debuff unk id 10?",
        [0x376DD6D8EECFFBC1] = "AttackHateFrozenAilment", //  - buff or debuff unk id 11?",
        [0x7E7BA7A132DDB652] = "AttackHateProvocationAilment", //  - ?",
        [0xBBFE81B7949AC823u] = "AttackHateFirstTarget", //  - hateRateFirstTargetPlayer_",
        [0x29F6DF02F321174F] = "AttackHateLastTarget", //  - hateRateLastTargetPlayer_",
        [0x8C1098397095FCCBu] = "AttackHateManyTarget", //  - hateRateTargetCountManyPlayer_",
        [0x1F39A2DCD44D0B2E] = "AttackHateFewTarget", //  - hateRateTargetCountFewPlayer_",
    };

    public EntitiesWindow(IImGui imGui, EntityHooks entityHooks)
    {
        _imGui = imGui;
        _entityHooks = entityHooks;
    }

    public void RenderMenu(IImGuiShell imGuiShell)
    {
        if (_imGui.MenuItemEx("Enemies & Hostility / Hate"u8, ""u8, false, true))
        {
            IsOpen = true;
        }
    }

    public void Render(IImGuiShell imGuiShell)
    {
        if (!IsOpen)
            return;

        if (_imGui.Begin("Hostility / 'Hate' Data"u8, ref IsOpen, 0))
        {
            if (_entityHooks.LoadedEntitiesPtr is null)
                return;

            _imGui.Checkbox("Show Enemy Hate Params"u8, ref _showHateParams);
            _imGui.Checkbox("Show Targets Hate Params"u8, ref _showTargetHateParams);

            Span<EntityRef> entries = _entityHooks.LoadedEntitiesPtr->AsSpan();
            for (int i = *(int*)_entityHooks.EnemyStartIndexPtr; i < entries.Length; i++)
            {
                ref EntityRef enemyEntity = ref entries[i];
                EntityWrapper* entWrapper = enemyEntity.EntityRefPtr;
                if (entWrapper is null)
                    continue;

                cObj* enemyObj = entWrapper->EntityObjPtr;

                string name = Marshal.PtrToStringAnsi((nint)(&entWrapper->Name[0]));
                IExEmAttackTarget enemyAttackTarget = new ExEmAttackTargetView<ExEmAttackTarget_ER>((ExEmAttackTarget_ER*)_entityHooks.WRAPPER_EntityRef_GetEmAttackTargetExtension((EntityRef*)Unsafe.AsPointer(ref enemyEntity)));
                if (enemyAttackTarget is null)
                    continue;

                if (_imGui.CollapsingHeader($"{name} (AID {enemyAttackTarget.Target.ActorId}) ({enemyObj->GetName()}) - tgt updates: {enemyAttackTarget.NumTargetUpdates}###enemy{enemyEntity.ActorId}##"))
                {
                    if (enemyAttackTarget.Target.EntityRefPtr is not null)
                    {
                        _imGui.Text($"-> Targetting: {enemyAttackTarget.Target.EntityRefPtr->EntityObjPtr->GetName()} (actor id {enemyAttackTarget.Target.ActorId})");

                        if (_currentTargets.TryGetValue(enemyEntity.ActorId, out uint targetId))
                        {
                            if (targetId != enemyAttackTarget.Target.ActorId)
                            {
                                imGuiShell.LogWriteLine($"{nameof(EntitiesWindow)}", $"{name} (actor id {enemyEntity.ActorId}) ({enemyObj->GetName()}) now targets {enemyAttackTarget.Target.EntityRefPtr->EntityObjPtr->GetName()} (actor id {enemyAttackTarget.Target.ActorId})");
                                _currentTargets[enemyEntity.ActorId] = enemyAttackTarget.Target.ActorId;
                            }
                        }
                        else
                            _currentTargets.TryAdd(enemyEntity.ActorId, enemyAttackTarget.Target.ActorId);

                    }
                    else
                        _imGui.Text($"{name} ({enemyObj->GetName()}) - no target");


                    if (_showHateParams)
                    {
                        _imGui.Text("Params:"u8);
                        var node = enemyAttackTarget.HashToAttackHateParamMap.List.Node->Next;
                        for (int j = 0; j < enemyAttackTarget.HashToAttackHateParamMap.Size(); j++)
                        {
                            var data = node->Data;
                            float value = *(float*)data;

                            if (HateParamNames.TryGetValue(node->Key, out string paramName))
                                _imGui.BulletText($"{paramName}: {value:F2}");
                            else
                                _imGui.BulletText($"{node->Key:X8}: {value:F2}");
                            node = node->Next;
                        }
                    }

                    _imGui.Text("Enemy Targets:"u8);
                    var span = enemyAttackTarget.AttackTargetPlayerList.AsSpan();
                    for (int j = 0; j < span.Length; j++)
                    {
                        ref AttackTargetPlayerEntry attackTargetEntry = ref span[j];
                        AttackTargetPlayer* attackTarget = attackTargetEntry.AttackTarget;
                        if (attackTarget is not null && enemyAttackTarget.Target.EntityRefPtr is not null)
                        {
                            _imGui.Indent();
                            bool openTarget;
                            string targetTypeName = attackTarget->TargettingPlayer.EntityRefPtr->EntityObjPtr->GetName();
                            if (enemyAttackTarget.Target.EntityRefPtr->EntityObjPtr == attackTarget->TargettingPlayer.EntityRefPtr->EntityObjPtr)
                                openTarget = _imGui.CollapsingHeader($"=> {j} ({targetTypeName}) - last targetted index: {attackTarget->LastTargettedIndex} - hate: {attackTarget->WeightMultiplier}###target{attackTarget->TargettingPlayer.ActorId}");
                            else
                                openTarget = _imGui.CollapsingHeader($"{j} ({targetTypeName}) - last targetted index: {attackTarget->LastTargettedIndex} - hate: {attackTarget->WeightMultiplier}###target{attackTarget->TargettingPlayer.ActorId}");

                            
                            if (openTarget)
                            {
                                if (_showTargetHateParams)
                                {
                                    _imGui.IndentEx(12);

                                    var node = attackTarget->HateParams.List.Node->Next;
                                    for (int k = 0; k < attackTarget->HateParams.Size(); k++)
                                    {
                                        UnkHateParamWrapper* data = (UnkHateParamWrapper*)&(node->Data);
                                        AttackHateBase* attackHate = data->AttackHate;
                                        if (HateParamNames.TryGetValue(node->Key, out string paramName))
                                            _imGui.BulletText($"{paramName} = {attackHate->Param.Value}");
                                        else
                                            _imGui.BulletText($"0x{node->Key:X8} = {attackHate->Param.Value}");
                                        node = node->Next;
                                    }
                                    _imGui.UnindentEx(12);
                                }
                            }
                            _imGui.Unindent();
                        }
                    }
                }
            }

            _imGui.Separator();
            for (uint i = 0; i < 4; i++)
                _imGui.BulletText($"Player #{i + 1} hostility: {_entityHooks.WRAPPER_GetHostilityForPlayer(i):F2}");
        }

        _imGui.End();
    }
}

public unsafe struct UnkHateParamWrapper
{
    public nint __vftable;
    public float Field_0x08;
    public float Field_0x0C;
    public nint Field_0x10;

    public AttackHateBase* AttackHate;
}

public struct AttackHateBase
{
    public nint __vftable;
    public EntityRef EnemyTarget;
    public EntityRef PlayerEntity;
    public nint UnkVTable;
    public AttackHateParam Param;
}

public struct AttackHateParam
{
    public nint __vftable; // Deleter vtable?
    public float Value;
}