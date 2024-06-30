using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public unsafe struct FileOpenResult
{
    public void* pFileStorage; // flatark::impl::Flatark::ChunkFileStorage
    public byte* pFileData;
    public ulong FileSize;
}

public unsafe struct StringWrap
{
    public char* pStr;
    public char* StringSize;
}

// Good resource: https://bbs.kanxue.com/thread-270547.htm

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/list#L286
public unsafe struct StdListNode // _List_node
{
    public StdListNode* Next;
    public StdListNode* Previous;
    public uint Key;
    public uint Padding;
    public void* Data; // Starting from here is data. Type is templated, it could be anything else inline to this struct i.e a std::vector
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/list#L755
public unsafe struct StdList // std::list
{
    public StdListNode* Node;
    public uint Size;
}

// https://github.com/microsoft/STL/blob/881bcadeca4ae9240a132588d9ac983e7b24dbe0/stl/inc/xhash#L1960
public unsafe struct StdUnorderedMap
{
    public ulong LoadFactor;
    public StdList List;  
    public StdVector Vec;
    public ulong Mask;
    public ulong MaskIdx;
};

public unsafe struct StdVector
{
    public void* Myfirst;
    public void* Mylast;
    public void* Myend;
}

public unsafe struct CharacterManager
{
    public uint _UnkHash;
    public uint _Unk2;
    public StdVector _AllKeys; // All main keys
    public fixed byte _pad[0x18];

    public StdUnorderedMap Chara;
    public StdUnorderedMap CharaCostume;
    public StdUnorderedMap CharaExp;
    public StdUnorderedMap CharaExpType;
    public StdUnorderedMap CharaStatus;
    public StdUnorderedMap CharaGem;
    public StdUnorderedMap CharaColor;
    public StdUnorderedMap CharaDrain;
    public StdVector CharaDiff;
    public StdUnorderedMap CharaPowerAdjust;
    public StdUnorderedMap CharaPowerAttenuate;
    public StdVector CharaLevelSync;
    public StdUnorderedMap CharaStatusFate;
    public StdVector CharaInvite;
    public StdUnorderedMap CharaGuestNpcParameter;
    public StdUnorderedMap FormationSlot;

    // ...lots more
}

public unsafe struct GemManager
{
    public StdUnorderedMap Gem;
    public StdUnorderedMap GemRare;
    public StdUnorderedMap GemType;
    public StdUnorderedMap GemTicket;
    public StdUnorderedMap GemSell;
    public StdUnorderedMap GemMixRupi;
    public StdUnorderedMap GemMixSuccess;
    public StdUnorderedMap GemMixTicket;
}

public unsafe struct ItemManager
{
    public fixed byte char0[0x60];
    public StdUnorderedMap Item;
    public StdUnorderedMap ItemCategory;
    public StdUnorderedMap ItemConsume;
    public StdUnorderedMap ItemMaterialList;
    public StdUnorderedMap ItemMaterialCommonAnima;
    public StdUnorderedMap ItemMaterialCommonSpecial;
    public StdUnorderedMap ItemMaterialCommonBoss;
    public StdUnorderedMap ItemMaterialCommonStage;
    public StdUnorderedMap ItemTierMap;
    public StdUnorderedMap ItemPendulum;
    public StdUnorderedMap ItemJunk;
    public StdUnorderedMap ItemJunkAppearRate;
    public StdUnorderedMap ItemJunkRate;
    public StdUnorderedMap ItemImportant;
    public StdUnorderedMap ItemQuestDetailDisp;
    public StdUnorderedMap ItemPendulumTicket;
    public StdUnorderedMap ItemPendulumSell;
    public StdUnorderedMap DropCoinParam;

    // Not tables
    public StdVector _ItemKeys; // List of all item keys
    public StdUnorderedMap _ItemQuestDetailDisp; // Duplicate of ItemQuestDetailDisp?
}

public unsafe struct LimitManager
{
    public StdUnorderedMap LimitBonus;
    public StdUnorderedMap LimitBonusType;
    public StdUnorderedMap LimitBonusParam;
    public StdUnorderedMap LimitBonusParamType;
    public StdUnorderedMap LimitBonusMeditation;
    public StdUnorderedMap LimitBonusMeditationCategory;
    public StdVector LimitBonusMeditationWeight;
    public StdUnorderedMap ApTreeAtk;
    public StdUnorderedMap ApTreeDef;
    public StdUnorderedMap ApTreeWep;
    public StdUnorderedMap ApOpenRank;
    // ...
}

public unsafe struct WeaponManager
{
    public StdUnorderedMap Weapon;
    public StdVector WeaponExp;
    public StdUnorderedMap WeaponStatus;
    public StdUnorderedMap WeaponStatusLevelSync;
    public StdUnorderedMap WeaponStatusAwake;
    public StdUnorderedMap WeaponStatusPlus;
    public StdVector WeaponLimit;
    public StdUnorderedMap WeaponSkillLevel;
    // ...
}

public unsafe struct SkillManager
{
    public StdVector _AllKeys;

    public StdUnorderedMap Skill;
    public StdUnorderedMap SkillStatus;
    public StdUnorderedMap SkillLot;
    public StdUnorderedMap SkillTypeLot;
    public StdUnorderedMap SkillLevelLot;
    public StdUnorderedMap LimitBonusMeditationCategory;
    public StdVector LimitBonusMeditationWeight;
    public StdUnorderedMap ApTreeAtk;
    public StdUnorderedMap ApTreeDef;
    public StdUnorderedMap ApTreeWep;
    public StdUnorderedMap ApOpenRank;
    // ...
}