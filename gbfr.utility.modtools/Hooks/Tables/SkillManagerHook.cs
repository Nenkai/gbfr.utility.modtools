using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks.Tables;

public unsafe class SkillManagerHook : TableManagerBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;

    private delegate void SkillManagerLoad(SkillManager* this_);
    private IHook<SkillManagerLoad> _skillManagerLoadHook;

    public SkillManagerHook(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public override void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(SkillManagerLoad), groupSource, result
            => _skillManagerLoadHook = _hooks.CreateHook<SkillManagerLoad>(SkillManagerLoadImpl, result).Activate());
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
