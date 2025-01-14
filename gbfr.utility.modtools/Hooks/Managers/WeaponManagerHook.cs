using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class WeaponManagerHook : TableManagerBase
{
    private IReloadedHooks _hooks;

    private delegate void WeaponManagerLoad(WeaponManager* this_);
    private IHook<WeaponManagerLoad> _weaponManagerLoadHook;

    public WeaponManagerHook(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 29 C4 48 8D AC 24 ?? ?? ?? ?? " +
            "C5 78 29 85 ?? ?? ?? ?? C5 F8 29 BD ?? ?? ?? ?? C5 F8 29 B5 ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 48 89 CB", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _weaponManagerLoadHook = _hooks.CreateHook<WeaponManagerLoad>(WeaponManagerLoadImpl, addr).Activate();
        });
    }

    public void WeaponManagerLoadImpl(WeaponManager* this_)
    {
        _weaponManagerLoadHook.OriginalFunction(this_);

        AddTableMap("weapon", &this_->Weapon); // unordered_map<cyan::string_hash32, table::WeaponData>
        AddTableVector("weapon_exp", &this_->WeaponExp); // vector<table::WeaponExpData>
        AddTableMap("weapon_status", &this_->WeaponStatus, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::WeaponStatusData>>
        AddTableMap("weapon_status_level_sync", &this_->WeaponStatusLevelSync, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::WeaponStatusData>>
        AddTableMap("weapon_status_awake", &this_->WeaponStatusAwake, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::WeaponStatusData>>
        AddTableMap("weapon_status_plus", &this_->WeaponStatusPlus, isVectorMap: true); // unordered_map<cyan::string_hash32, table::WeaponStatusData>
        AddTableVector("weapon_limit", &this_->WeaponLimit); // vector<table::WeaponLimitData>
        AddTableMap("weapon_skill_level", &this_->WeaponSkillLevel); // unordered_map<cyan::string_hash32, table::WeaponSkillLevelData>
    }
}
