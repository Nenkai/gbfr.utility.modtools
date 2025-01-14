using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class LimitApManagerHook : TableManagerBase
{
    private IReloadedHooks _hooks;

    private delegate void LimitManagerLoad(LimitManager* this_);
    private IHook<LimitManagerLoad> _limitManagerLoadHook;

    public LimitApManagerHook(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 4D ?? C5 78 29 45 " +
            "?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CB C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 C7 05 ?? ?? " +
            "?? ?? ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 48 8D 05 ?? ?? ?? ?? 48 89 45 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 8D 4D ?? 48 8D 55 ?? E8 ?? " +
            "?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B 4D ?? 48 89 0D ?? ?? ?? ?? C5 F8 10 45 ?? C5 F8 11 05 ?? ?? ?? ?? 48 85 C0 74 ?? 48 8B 10 48 89 C1 FF " +
            "52 ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 4C 8B 25", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _limitManagerLoadHook = _hooks.CreateHook<LimitManagerLoad>(LimitManagerLoadImpl, addr).Activate();
        });
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
