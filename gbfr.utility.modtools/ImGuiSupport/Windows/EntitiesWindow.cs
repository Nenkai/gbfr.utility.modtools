using DearImguiSharp;

using gbfr.utility.modtools.Hooks;

using Reloaded.Mod.Interfaces;

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

public unsafe class EntitiesWindow : IImguiWindow, IImguiMenuComponent
{
    public bool IsOverlay => false;
    public bool IsOpen = false;

    private bool _showHateParams = false;
    private bool _showTargetHateParams = false;

    private EntityHooks _entityHooks;

    public EntitiesWindow(EntityHooks entityHooks)
    {
        _entityHooks = entityHooks;
    }

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

    public void BeginMenuComponent()
    {
        if (ImGui.MenuItemEx("Enemies & Hostility / Hate", "", "", false, true))
        {
            IsOpen = true;
        }
    }

    private ConcurrentDictionary<uint, uint> _currentTargets = [];

    public void Render(ImguiSupport imguiSupport)
    {
        if (!IsOpen)
            return;

        if (ImGui.Begin("Hostility / 'Hate' Data", ref IsOpen, 0))
        {
            if (_entityHooks.LoadedEntitiesPtr is null)
                return;

            ImGui.Checkbox("Show Enemy Hate Params", ref _showHateParams);
            ImGui.Checkbox("Show Targets Hate Params", ref _showTargetHateParams);

            Span<EntityRef> entries = _entityHooks.LoadedEntitiesPtr->AsSpan();
            for (int i = *(int*)_entityHooks.EnemyStartIndexPtr; i < entries.Length; i++)
            {
                ref EntityRef enemyEntity = ref entries[i];
                EntityWrapper* entWrapper = enemyEntity.EntityRefPtr;
                if (entWrapper is null)
                    continue;

                cObj* enemyObj = entWrapper->EntityObjPtr;

                string name = Marshal.PtrToStringAnsi((nint)(&entWrapper->Name[0]));
                ExEmAttackTarget* enemyAttackTarget = _entityHooks.WRAPPER_EntityRef_GetEmAttackTargetExtension((EntityRef*)Unsafe.AsPointer(ref enemyEntity));
                if (enemyAttackTarget is null)
                    continue;

                if (enemyAttackTarget->Target.EntityRefPtr is not null)
                {
                    ImGui.Text($"{name} (actor id {enemyAttackTarget->Target.ActorId}) ({enemyObj->GetName()}) - num updates: {enemyAttackTarget->NumTargetUpdates}");
                    ImGui.Text($"-> Targetting: {enemyAttackTarget->Target.EntityRefPtr->EntityObjPtr->GetName()} (actor id {enemyAttackTarget->Target.ActorId})");

                    if (_currentTargets.TryGetValue(enemyEntity.ActorId, out uint targetId))
                    {
                        if (targetId != enemyAttackTarget->Target.ActorId)
                        {
                            OverlayLogger.Instance.AddMessage($"{name} (actor id {enemyEntity.ActorId}) ({enemyObj->GetName()}) now targets {enemyAttackTarget->Target.EntityRefPtr->EntityObjPtr->GetName()} (actor id {enemyAttackTarget->Target.ActorId})");
                            _currentTargets[enemyEntity.ActorId] = enemyAttackTarget->Target.ActorId;
                        }
                    }
                    else
                        _currentTargets.TryAdd(enemyEntity.ActorId, enemyAttackTarget->Target.ActorId);

                }
                else
                    ImGui.Text($"{name} ({enemyObj->GetName()}) - no target");

                if (_showHateParams)
                {
                    ImGui.Text("Params:");
                    var node = enemyAttackTarget->HashToAttackHateParamMap.List.Node->Next;
                    for (int j = 0; j < enemyAttackTarget->HashToAttackHateParamMap.Size(); j++)
                    {
                        var data = node->Data;
                        float value = *(float*)data;

                        if (HateParamNames.TryGetValue(node->Key, out string paramName))
                            ImGui.BulletText($"{paramName}: {value:F2}");
                        else
                            ImGui.BulletText($"{node->Key:X8}: {value:F2}");
                        node = node->Next;
                    }
                }

                ImGui.Text("Enemy Targets:");
                var span = enemyAttackTarget->AttackTargetPlayerList.AsSpan();
                for (int j = 0; j < span.Length; j++)
                {
                    ref AttackTargetPlayerEntry attackTargetEntry = ref span[j];
                    AttackTargetPlayer* attackTarget = attackTargetEntry.AttackTarget;
                    if (attackTarget is not null && enemyAttackTarget->Target.EntityRefPtr is not null)
                    {
                        if (enemyAttackTarget->Target.EntityRefPtr->EntityObjPtr == attackTarget->TargettingPlayer.EntityRefPtr->EntityObjPtr)
                            ImGui.BulletText($"=> {j} ({attackTarget->TargettingPlayer.EntityRefPtr->EntityObjPtr->GetName()}) - last targetted index: {attackTarget->LastTargettedIndex} - hate: {attackTarget->WeightMultiplier}");
                        else
                            ImGui.BulletText($"{j} ({attackTarget->TargettingPlayer.EntityRefPtr->EntityObjPtr->GetName()}) - last targetted index: {attackTarget->LastTargettedIndex} - hate: {attackTarget->WeightMultiplier}");

                        if (_showTargetHateParams)
                        {
                            ImGui.Indent(12);

                            var node = attackTarget->HateParams.List.Node->Next;
                            for (int k = 0; k < attackTarget->HateParams.Size(); k++)
                            {
                                UnkHateParamWrapper* data = (UnkHateParamWrapper*)&(node->Data);
                                AttackHateBase* attackHate = data->AttackHate;
                                if (HateParamNames.TryGetValue(node->Key, out string paramName))
                                    ImGui.BulletText($"{paramName} = {attackHate->Param.Value}");
                                else
                                    ImGui.BulletText($"0x{node->Key:X8} = {attackHate->Param.Value}");
                                node = node->Next;
                            }
                            ImGui.Unindent(12);
                        }
                    }
                }
            }

            ImGui.Separator();
            for (uint i = 0; i < 4; i++)
                ImGui.BulletText($"Player #{i + 1} hostility: {_entityHooks.WRAPPER_GetHostilityForPlayer(i):F2}");

        }
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