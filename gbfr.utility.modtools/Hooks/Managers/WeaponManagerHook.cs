using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class WeaponManagerHook : TableManagerBase
{
    private delegate void WeaponManagerLoad(WeaponManager* this_);
    private IHook<WeaponManagerLoad> _weaponManagerLoadHook;

    public WeaponManagerHook()
    {

    }

    public override void Init()
    {
        Project.Scans.AddScanHook(nameof(WeaponManagerLoad), (result, hooks)
            => _weaponManagerLoadHook = hooks.CreateHook<WeaponManagerLoad>(WeaponManagerLoadImpl, result).Activate());
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
