using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class SkillManagerHook : TableManagerBase
{
    private delegate void SkillManagerLoad(SkillManager* this_);
    private IHook<SkillManagerLoad> _skillManagerLoadHook;

    public SkillManagerHook()
    {

    }

    public override void Init()
    {
        Project.Scans.AddScanHook(nameof(SkillManagerLoad), (result, hooks)
            => _skillManagerLoadHook = hooks.CreateHook<SkillManagerLoad>(SkillManagerLoadImpl, result).Activate());
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
