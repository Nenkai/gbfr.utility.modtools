using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;

using RyoTune.Reloaded;

namespace gbfr.utility.modtools.Hooks.Managers;

public unsafe class ItemManagerHook : TableManagerBase
{
    public delegate void ItemManagerLoad(ItemManager* this_);
    public IHook<ItemManagerLoad> HOOK_ItemManagerLoad { get; private set; }

    public ItemManagerHook()
    {

    }

    public override void Init()
    {
        Project.Scans.AddScanHook(nameof(ItemManagerLoad), (result, hooks)
            => HOOK_ItemManagerLoad = hooks.CreateHook<ItemManagerLoad>(ItemManagerLoadImpl, result).Activate());
    }

    public void ItemManagerLoadImpl(ItemManager* this_)
    {
        HOOK_ItemManagerLoad.OriginalFunction(this_);

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
