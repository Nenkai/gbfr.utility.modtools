using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gbfr.utility.modtools.ImGuiSupport.Windows.Tables;

using Reloaded.Hooks.Definitions;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class CharacterManagerHook : TableManagerBase
{
    public delegate void CharacterManagerLoad(CharacterManager* this_);
    public IHook<CharacterManagerLoad> HOOK_CharacterManagerLoad { get; private set; }

    private CharacterManagerWindow _characterManagerWindow;

    public CharacterManagerHook()
    {

    }

    public override void Init()
    {
        Project.Scans.AddScanHook(nameof(CharacterManagerLoad), (result, hooks)
            => HOOK_CharacterManagerLoad = hooks.CreateHook<CharacterManagerLoad>(CharacterManagerLoadImpl, result).Activate());
    }

    public void CharacterManagerLoadImpl(CharacterManager* this_)
    {
        HOOK_CharacterManagerLoad.OriginalFunction(this_);

        AddTableMap("chara", &this_->Chara); // unordered_map<cyan::string_hash32, table::CharaData>
        AddTableMap("chara_costume", &this_->CharaCostume, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaCostumeData>>
        AddTableMap("chara_exp", &this_->CharaExp, isVectorMap: true); // unordered_map<uint, vector<table::CharaExpData>>
        AddTableMap("chara_exp_type", &this_->CharaExpType); // unordered_map<uint, CharaExpTypeData>
        AddTableMap("chara_status", &this_->CharaStatus, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaStatusData>>
        AddTableMap("chara_gem", &this_->CharaGem, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaGemData>>
        AddTableMap("chara_color", &this_->CharaColor); // unordered_map<uint, CharaColorData>
        AddTableMap("chara_drain", &this_->CharaDrain, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaDrainData>>
        AddTableVector("chara_diff", &this_->CharaDiff);
        AddTableMap("chara_power_adjust", &this_->CharaPowerAdjust); // unordered_map<uint, table::CharaPowerAdjustdata>
        AddTableMap("chara_power_attenuate", &this_->CharaPowerAttenuate, isVectorMap: true); // unordered_map<uint, vector<table::CharaPowerAttenuateData>>
        AddTableVector("chara_level_sync", &this_->CharaLevelSync);
        AddTableMap("chara_status_fate", &this_->CharaStatusFate, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaStatusFateData>>
        AddTableVector("chara_invite", &this_->CharaInvite);
        AddTableMap("chara_guest_npc_parameter", &this_->CharaGuestNpcParameter); // unordered_map<uint, table::CharaGuestNpcParameterData>

        AddTableMap("formation_slot", &this_->FormationSlot); // unordered_map<cyan::string_hash32, table::FormationSlotData>
    }
}
