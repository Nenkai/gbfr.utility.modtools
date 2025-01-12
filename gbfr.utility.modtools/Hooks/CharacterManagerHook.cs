using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using GBFRDataTools.Database;
using GBFRDataTools.Database.Entities;
using gbfr.utility.modtools.ImGuiSupport.Windows.Tables;
using SharedScans.Interfaces;

namespace gbfr.utility.modtools.Hooks;

public unsafe class CharacterManagerHook
{
    private ISharedScans _scans;

    public delegate void CharacterManagerLoad(CharacterManager* this_);
    public HookContainer<CharacterManagerLoad> HOOK_CharacterManagerLoad { get; private set; }

    private CharacterManagerWindow _characterManagerWindow;

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(CharacterManagerLoad)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 8D ?? ?? ?? ?? C5 78 29 45 ?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CB",
    };

    public CharacterManagerHook(ISharedScans scans, CharacterManagerWindow characterManagerWindow)
    {
        _scans = scans;
        _characterManagerWindow = characterManagerWindow;
    }

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_CharacterManagerLoad = _scans.CreateHook<CharacterManagerLoad>(CharacterManagerLoadImpl, "a");
    }

    public void CharacterManagerLoadImpl(CharacterManager* this_)
    {
        HOOK_CharacterManagerLoad.Hook.OriginalFunction(this_);

        _characterManagerWindow.AddTableMap("chara", &this_->Chara); // unordered_map<cyan::string_hash32, table::CharaData>
        _characterManagerWindow.AddTableMap("chara_costume", &this_->CharaCostume, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaCostumeData>>
        _characterManagerWindow.AddTableMap("chara_exp", &this_->CharaExp, isVectorMap: true); // unordered_map<uint, vector<table::CharaExpData>>
        _characterManagerWindow.AddTableMap("chara_exp_type", &this_->CharaExpType); // unordered_map<uint, CharaExpTypeData>
        _characterManagerWindow.AddTableMap("chara_status", &this_->CharaStatus, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaStatusData>>
        _characterManagerWindow.AddTableMap("chara_gem", &this_->CharaGem, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaGemData>>
        _characterManagerWindow.AddTableMap("chara_color", &this_->CharaColor); // unordered_map<uint, CharaColorData>
        _characterManagerWindow.AddTableMap("chara_drain", &this_->CharaDrain, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaDrainData>>
        _characterManagerWindow.AddTableVector("chara_diff", &this_->CharaDiff);
        _characterManagerWindow.AddTableMap("chara_power_adjust", &this_->CharaPowerAdjust); // unordered_map<uint, table::CharaPowerAdjustdata>
        _characterManagerWindow.AddTableMap("chara_power_attenuate", &this_->CharaPowerAttenuate, isVectorMap: true); // unordered_map<uint, vector<table::CharaPowerAttenuateData>>
        _characterManagerWindow.AddTableVector("chara_level_sync", &this_->CharaLevelSync);
        _characterManagerWindow.AddTableMap("chara_status_fate", &this_->CharaStatusFate, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaStatusFateData>>
        _characterManagerWindow.AddTableVector("chara_invite", &this_->CharaInvite);
        _characterManagerWindow.AddTableMap("chara_guest_npc_parameter", &this_->CharaGuestNpcParameter); // unordered_map<uint, table::CharaGuestNpcParameterData>

        _characterManagerWindow.AddTableMap("formation_slot", &this_->FormationSlot); // unordered_map<cyan::string_hash32, table::FormationSlotData>
    }
}
