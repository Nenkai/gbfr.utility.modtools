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
using gbfr.utility.modtools.Windows;

namespace gbfr.utility.modtools.Hooks;

public unsafe class DatabaseManager
{
    private IReloadedHooks _hooks;

    private delegate void CharacterManagerLoad(CharacterManager* this_);
    private IHook<CharacterManagerLoad> _characterManagerHook;

    public DatabaseManager(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 8D ?? ?? ?? ?? C5 78 29 45 ?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CB", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _characterManagerHook = _hooks.CreateHook<CharacterManagerLoad>(CharacterManagerLoadImpl, addr).Activate();
        });
    }

    public void CharacterManagerLoadImpl(CharacterManager* this_)
    {
        _characterManagerHook.OriginalFunction(this_);

        CharacterManagerWindow.AddTable("chara", &this_->Chara); // unordered_map<cyan::string_hash32, table::CharaData>
        CharacterManagerWindow.AddTable("chara_costume", &this_->CharaCostume, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::CharaCostumeData>>
        CharacterManagerWindow.AddTable("chara_exp", &this_->CharaExp); // unordered_map<cyan::string_hash32, table::CharaExpData>
        CharacterManagerWindow.AddTable("chara_exp_type", &this_->CharaExpType);
        CharacterManagerWindow.AddTable("chara_status", &this_->CharaStatus);
        CharacterManagerWindow.AddTable("chara_gem", &this_->CharaGem);
        CharacterManagerWindow.AddTable("chara_color", &this_->CharaColor);
        CharacterManagerWindow.AddTable("chara_drain", &this_->CharaDrain);

        CharacterManagerWindow.AddTable("chara_power_adjust", &this_->CharaPowerAdjust);
        CharacterManagerWindow.AddTable("chara_power_attenuate", &this_->CharaPowerAttenuate);

        CharacterManagerWindow.AddTable("chara_status_fate", &this_->CharaStatusFate);

        CharacterManagerWindow.AddTable("chara_guest_npc_parameter", &this_->CharaGuestNpcParameter);

        CharacterManagerWindow.AddTable("formation_slot", &this_->FormationSlot);
    }
}
