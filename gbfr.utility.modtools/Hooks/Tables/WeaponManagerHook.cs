using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks.Tables;

public unsafe class WeaponManagerHook : TableManagerBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;

    private delegate void WeaponManagerLoad(WeaponManager* this_);
    private IHook<WeaponManagerLoad> _weaponManagerLoadHook;

    public WeaponManagerHook()
    {

    }

    public override void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(WeaponManagerLoad), groupSource, result
            => _weaponManagerLoadHook = _hooks.CreateHook<WeaponManagerLoad>(WeaponManagerLoadImpl, result).Activate());
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
