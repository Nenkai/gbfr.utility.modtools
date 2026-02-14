using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RyoTune.Reloaded;

using Reloaded.Hooks.Definitions;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class LimitApManagerHook : TableManagerBase
{
    private delegate void LimitManagerLoad(LimitManager* this_);
    private IHook<LimitManagerLoad> _limitManagerLoadHook;

    public LimitApManagerHook()
    {

    }

    public override void Init()
    {
        Project.Scans.AddScanHook(nameof(LimitManagerLoad), (result, hooks)
            => _limitManagerLoadHook = hooks.CreateHook<LimitManagerLoad>(LimitManagerLoadImpl, result).Activate());
    }

    public void LimitManagerLoadImpl(LimitManager* this_)
    {
        _limitManagerLoadHook.OriginalFunction(this_);

        AddTableMap("limit_bonus", &this_->LimitBonus); // unordered_map<cyan::string_hash32, table::LimitBonusData>
        AddTableMap("limit_bonus_type", &this_->LimitBonusType); // unordered_map<int, table::LimitBonusTypeData>
        AddTableMap("limit_bonus_param", &this_->LimitBonusParam); // unordered_map<cyan::string_hash32, table::LimitBonusParamData>
        AddTableMap("limit_bonus_param_type", &this_->LimitBonusParamType); // unordered_map<int, table::LimitBonusParamTypeData>
        AddTableMap("limit_bonus_meditation", &this_->LimitBonusMeditation); // unordered_map<int, table::MeditationData>
        AddTableMap("limit_bonus_meditation_category", &this_->LimitBonusMeditationCategory, isVectorMap: true); // unordered_map<int, vector<table::MeditationCategoryData>>
        AddTableVector("limit_bonus_meditation_weight", &this_->LimitBonusMeditationWeight);
        AddTableMap("ap_tree_atk", &this_->ApTreeAtk); // unordered_map<cyan::string_hash32, table::ApTreeData>
        AddTableMap("ap_tree_def", &this_->ApTreeDef); // unordered_map<cyan::string_hash32, table::ApTreeData>
        AddTableMap("ap_tree_wep", &this_->ApTreeWep); // unordered_map<cyan::string_hash32, table::ItemJunkData>
        AddTableMap("ap_open_rank", &this_->ApOpenRank); // unordered_map<int, table::ApOpenRankData>
    }
}
