﻿using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using GBFRDataTools.Database;
using GBFRDataTools.Database.Entities;
using gbfr.utility.modtools.Windows.Tables;

namespace gbfr.utility.modtools.Hooks;

public unsafe class GemManagerHook
{
    private IReloadedHooks _hooks;

    private delegate void GemManagerLoad(GemManager* this_);
    private IHook<GemManagerLoad> _gemManagerLoadHook;

    private GemManagerWindow _gemManagerWindow;
    public GemManagerHook(IReloadedHooks hooks, GemManagerWindow gemManagerWindow)
    {
        _hooks = hooks;
        _gemManagerWindow = gemManagerWindow;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 4D ?? C5 78 29 45 ?? C5 F8 29 7D ?? C5 F8 29 75 ?? " +
            "48 C7 45 ?? ?? ?? ?? ?? 48 89 CE C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 C7 05 ?? ?? ?? ?? ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 48 8D 05 ?? ?? " +
            "?? ?? 48 89 45 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 8D 4D ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8B 4D ?? 48 89 0D ?? ?? ?? ?? C5 F8 10 45 ?? C5 F8 11 05 ?? ?? ?? " +
            "?? 48 85 C0 74 ?? 48 8B 10 48 89 C1 FF 52 ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _gemManagerLoadHook = _hooks.CreateHook<GemManagerLoad>(GemManagerLoadImpl, addr).Activate();
        });
    }

    public void GemManagerLoadImpl(GemManager* this_)
    {
        _gemManagerLoadHook.OriginalFunction(this_);

        _gemManagerWindow.AddTableMap("gem", &this_->Gem); // unordered_map<cyan::string_hash32, table::GemData>
        _gemManagerWindow.AddTableMap("gem_rare", &this_->GemRare); // unordered_map<cyan::string_hash32, table::GemRare>
        //_gemManagerWindow.AddTableMap("gem_type", &this_->GemType); // unordered_map<uint, table::GemTypeData>>
        _gemManagerWindow.AddTableMap("gem_ticket", &this_->GemTicket, isVectorMap: true); // unordered_map<int, vector<table::GemTicket>
        _gemManagerWindow.AddTableMap("gem_sell", &this_->GemSell, isVectorMap: true); // unordered_map<int, vector<table::GemSell>>
        _gemManagerWindow.AddTableMap("gem_mix_rupi", &this_->GemMixRupi); // unordered_map<int, table::GemMixRupiData>
        _gemManagerWindow.AddTableMap("gem_mix_success", &this_->GemMixSuccess); // unordered_map<int, table::GemMixTicketData>
        _gemManagerWindow.AddTableMap("gem_mix_ticket", &this_->GemMixTicket); // unordered_map<cyan::string_hash32, table::GemMixTicketData>
    }
}
