using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class SkillManagerHook : TableManagerBase
{
    private IReloadedHooks _hooks;

    private delegate void SkillManagerLoad(SkillManager* this_);
    private IHook<SkillManagerLoad> _skillManagerLoadHook;

    public SkillManagerHook(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IStartupScanner startupScanner)
    {
        // Geez.
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 4D ?? C5 78 29 45 ?? C5 F8 29 " +
            "7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CE C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 C7 05 ?? ?? ?? ?? ?? ?? ?? ?? 48 85 " +
            "C9 74 ?? 48 8B 01 FF 50 ?? 48 8D 05 ?? ?? ?? ?? 48 89 45 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 8D 4D ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B " +
            "4D ?? 48 89 0D ?? ?? ?? ?? C5 F8 10 45 ?? C5 F8 11 05 ?? ?? ?? ?? 48 85 C0 74 ?? 48 8B 10 48 89 C1 FF 52 ?? 48 8B 0D ?? ?? ?? ?? 48 89 75 ?? 48 85 C9 " +
            "74 ?? 48 8B 01 FF 50 ?? 4C 8B 3D ?? ?? ?? ?? 49 8B 37 49 83 C7 ?? 48 85 F6 4C 0F 44 FE C7 45 ?? ?? ?? ?? ?? C5 F8 57 C0 C5 F8 11 45 ?? 48 8D 0D ?? ?? " +
            "?? ?? 48 8D 15 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 05 ?? ?? ?? ?? 65 48 8B 0C 25 ?? ?? ?? ?? 48 8B 04 C1 48 8B 88 ?? ?? ?? ?? 48 8B 41 ?? 4C 8B 40 ?? 4D 85 C0", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _skillManagerLoadHook = _hooks.CreateHook<SkillManagerLoad>(SkillManagerLoadImpl, addr).Activate();
        });
    }

    public void SkillManagerLoadImpl(SkillManager* this_)
    {
        _skillManagerLoadHook.OriginalFunction(this_);

        AddTableMap("skill", &this_->Skill); // unordered_map<cyan::string_hash32, table::SkillData>
        AddTableMap("skill_status", &this_->SkillStatus, isVectorMap: true); // unordered_map<int, vector<table::SkillStatusData>>
        AddTableMap("skill_lot", &this_->SkillLot, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::SkillLotData>>
        AddTableMap("skill_type_lot", &this_->SkillTypeLot); // unordered_map<int, table::SkillTypeLotData>
        AddTableMap("skill_level_lot", &this_->SkillLevelLot); // unordered_map<int, table::SkillLevelLotData>

    }
}
