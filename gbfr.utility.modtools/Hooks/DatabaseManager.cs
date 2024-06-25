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
using gbfr.utility.modtools.Windows;

namespace gbfr.utility.modtools.Hooks;

public unsafe class DatabaseManager
{
    private IReloadedHooks _hooks;

    private delegate void CharacterManagerLoad(CharacterManager* this_);
    private IHook<CharacterManagerLoad> _characterManagerHook;

    public DatabaseManager(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IStartupScanner startupScanner)
    {
        startupScanner.AddMainModuleScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 8D ?? ?? ?? ?? C5 78 29 45 ?? C5 F8 29 7D ?? C5 F8 29 75 ?? 48 C7 45 ?? ?? ?? ?? ?? 48 89 CB", e =>
        {
            if (!e.Found)
                return;

            var addr = Process.GetCurrentProcess().MainModule.BaseAddress + e.Offset;
            _characterManagerHook = _hooks.CreateHook<CharacterManagerLoad>(CharacterManagerLoadImpl, addr).Activate();
        });
    }

    public void CharacterManagerLoadImpl(CharacterManager* this_)
    {
        _characterManagerHook.OriginalFunction(this_);

        List<TableColumn> charaColumns = TableMappingReader.ReadColumnMappings("chara", new Version(1, 3, 1), out int readSize);

        var charaTable = new DatabaseTable("chara", charaColumns, readSize,&this_->Chara);
        CharacterManagerWindow.Tables.Add(charaTable);
        CharacterManagerWindow.SelectedTable = charaTable;
    }
}
