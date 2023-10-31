using System;
using System.IO;
using System.Linq;
using GagSpeak.UI;
using Newtonsoft.Json.Linq;

namespace GagSpeak.Services;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class ConfigMigrationService
{
    private readonly SaveService         _saveService;
    private readonly BackupService       _backupService;

    private GagSpeakConfig _config = null!;
    private JObject       _data   = null!;

    public ConfigMigrationService(SaveService saveService, BackupService backupService)
    {
        _saveService         = saveService;
        _backupService       = backupService;
    }

    public void Migrate(GagSpeakConfig config)
    {
        _config = config; 
        if (config.Version >= GagSpeakConfig.Constants.CurrentVersion || !File.Exists(_saveService.FileNames.ConfigFile))
        {
            //AddColors(config, false);
            return;
        }

        _data = JObject.Parse(File.ReadAllText(_saveService.FileNames.ConfigFile));
        MigrateV1To2();
        MigrateV2To4();
        //AddColors(config, true);
    }

    private void MigrateV1To2()
    {
        if (_config.Version > 1)
            return;
        // There was information about colors here before, if you ever need them, put them back here.
        _backupService.CreateMigrationBackup("pre_v1_to_v2_migration");
        _config.Version = 2;
        //var customizationColor = _data["CustomizationColor"]?.ToObject<uint>() ?? ColorId.CustomizationDesign.Data().DefaultColor;
        //_config.Colors[ColorId.CustomizationDesign] = customizationColor;
        //var stateColor = _data["StateColor"]?.ToObject<uint>() ?? ColorId.StateDesign.Data().DefaultColor;
        //_config.Colors[ColorId.StateDesign] = stateColor;
        //var equipmentColor = _data["EquipmentColor"]?.ToObject<uint>() ?? ColorId.EquipmentDesign.Data().DefaultColor;
        //_config.Colors[ColorId.EquipmentDesign] = equipmentColor;
    }

    private void MigrateV2To4()
    {
        if (_config.Version > 4)
            return;
    
        _config.Version = 4;
    }

    // private static void AddColors(GagSpeakConfig config, bool forceSave)
    // {
    //     var save = false;
    //     foreach (var color in Enum.GetValues<ColorId>())
    //         save |= config.Colors.TryAdd(color, color.Data().DefaultColor);

    //     if (save || forceSave)
    //         config.Save();
    //     Colors.SetColors(config);
    // }
}
#pragma warning restore IDE1006
