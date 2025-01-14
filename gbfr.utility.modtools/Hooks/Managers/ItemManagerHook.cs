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

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class ItemManagerHook : TableManagerBase
{
    private ISharedScans _scans;

    public delegate void ItemManagerLoad(ItemManager* this_);
    public HookContainer<ItemManagerLoad> HOOK_ItemManagerLoad { get; private set; }

    public ItemManagerHook(ISharedScans scans)
    {
        _scans = scans;
    }

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(ItemManagerLoad)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 4D ?? C5 " +
            "78 29 45 ?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CB C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? " +
            "?? 48 C7 05 ?? ?? ?? ?? ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 48 8D 05 ?? ?? ?? ?? 48 89 45 ?? 48 C7 45 ?? ?? ?? ?? ?? " +
            "48 8D 4D ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B 4D ?? 48 89 0D ?? ?? ?? ?? C5 F8 10 45 ?? C5 F8 11 05 ?? ?? " +
            "?? ?? 48 85 C0 74 ?? 48 8B 10 48 89 C1 FF 52 ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 48 8B 35",
    };

    public void Init()
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_ItemManagerLoad = _scans.CreateHook<ItemManagerLoad>(ItemManagerLoadImpl, "a");
    }

    public void ItemManagerLoadImpl(ItemManager* this_)
    {
        HOOK_ItemManagerLoad.Hook.OriginalFunction(this_);

        AddTableMap("item", &this_->Item); // unordered_map<cyan::string_hash32, table::ItemData>
        AddTableMap("item_category", &this_->ItemCategory); // unordered_map<int, table::ItemCategoryData>
        AddTableMap("item_consume", &this_->ItemConsume); // unordered_map<cyan::string_hash32, table::ItemConsumeData>
        AddTableMap("item_material_list", &this_->ItemMaterialList); // unordered_map<int, table::ItemMaterialListData>
        AddTableMap("item_material_common_anima", &this_->ItemMaterialCommonAnima); // unordered_map<cyan::string_hash32, table::ItemMaterialCommonData>
        AddTableMap("item_material_common_special", &this_->ItemMaterialCommonSpecial); // unordered_map<cyan::string_hash32, table::ItemMaterialCommonData>
        AddTableMap("item_material_common_boss", &this_->ItemMaterialCommonBoss); // unordered_map<cyan::string_hash32, table::ItemMaterialCommonData>
        AddTableMap("item_material_common_stage", &this_->ItemMaterialCommonStage); // unordered_map<cyan::string_hash32, table::ItemMaterialCommonData>
        AddTableMap("item_tier_map", &this_->ItemTierMap); // unordered_map<cyan::string_hash32, table::ItemTierMapData>
        AddTableMap("item_pendulum", &this_->ItemPendulum); // unordered_map<cyan::string_hash32, table::ItemPendulumData>
        AddTableMap("item_junk", &this_->ItemJunk); // unordered_map<cyan::string_hash32, table::ItemJunkData>
        AddTableMap("item_junk_appear_rate", &this_->ItemJunkAppearRate, isVectorMap: true); // unordered_map<cyan::string_hash32, vector<table::ItemJunkAppearRate>>
        AddTableMap("item_junk_rate_group", &this_->ItemJunkRate, isVectorMap: true); // unordered_map<int, vector<table::ItemJunkAppearRate>>
        AddTableMap("item_important", &this_->ItemImportant); // unordered_map<cyan::string_hash32, table::ItemImportantData>
        AddTableMap("item_quest_detail_disp", &this_->ItemQuestDetailDisp); // unordered_map<int, table::ItemQuestDetailDispData>
        AddTableMap("item_pendulum_sell", &this_->ItemPendulumSell); // unordered_map<int, table::ItemPendulumSell>
        AddTableMap("dropcoin_param", &this_->DropCoinParam); // unordered_map<int, table::DropCoinParam>
    }
}
