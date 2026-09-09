using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools.Hooks.Tables;

public unsafe class GemManagerHook : TableManagerBase
{
    private readonly ILogger _logger;
    private readonly IReloadedHooks _hooks;
    private readonly IScanManager _scanManager;

    public delegate void GemManagerLoad(GemManager* this_);
    public IHook<GemManagerLoad> HOOK_GemManagerLoad { get; private set; }

    public GemManagerHook(ILogger logger, IScanManager scanManager, IReloadedHooks hooks)
    {
        _logger = logger;
        _scanManager = scanManager;
        _hooks = hooks;
    }

    public override void Init(string groupSource)
    {
        _scanManager.AddScan(nameof(GemManagerLoad), groupSource, result
            => HOOK_GemManagerLoad = _hooks.CreateHook<GemManagerLoad>(GemManagerLoadImpl, result).Activate());
    }

    public void GemManagerLoadImpl(GemManager* this_)
    {
        HOOK_GemManagerLoad.OriginalFunction(this_);

        AddTableMap("gem", &this_->Gem); // unordered_map<cyan::string_hash32, table::GemData>
        AddTableMap("gem_rare", &this_->GemRare); // unordered_map<cyan::string_hash32, table::GemRare>
        //_gemManagerWindow.AddTableMap("gem_type", &this_->GemType); // unordered_map<uint, table::GemTypeData>>
        AddTableMap("gem_ticket", &this_->GemTicket, isVectorMap: true); // unordered_map<int, vector<table::GemTicket>
        AddTableMap("gem_sell", &this_->GemSell, isVectorMap: true); // unordered_map<int, vector<table::GemSell>>
        AddTableMap("gem_mix_rupi", &this_->GemMixRupi); // unordered_map<int, table::GemMixRupiData>
        AddTableMap("gem_mix_success", &this_->GemMixSuccess); // unordered_map<int, table::GemMixTicketData>
        AddTableMap("gem_mix_ticket", &this_->GemMixTicket); // unordered_map<cyan::string_hash32, table::GemMixTicketData>
    }
}
