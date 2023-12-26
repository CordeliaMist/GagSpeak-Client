using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Services;

namespace GagSpeak.Data;
public class GagManager : IDisposable
{
    private readonly    GagSpeakConfig  _config;
    private readonly    GagService      _gagService;    
    public              List<Gag>       _activeGags;

    public GagManager(GagSpeakConfig config, GagService gagService) {
        _config = config;
        _gagService = gagService;
        // Filter the GagTypes list to only include gags with names in _config.selectedGagTypes
        _activeGags = _gagService.GagTypes.Where(gag => _config.selectedGagTypes.Contains(gag.Name)).ToList();
        // subscribe to our events
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
        //_config.phoneticSymbolList.ItemChanged += OnPhoneticSymbolListChanged;
    }

    public void Dispose() {
        _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
        //_config.phoneticSymbolList.ItemChanged -= OnPhoneticSymbolListChanged;
    }


    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        // Update _activeGags when _config.selectedGagTypes changes
        _activeGags = _config.selectedGagTypes
            .Where(gagType => _gagService.GagTypes.Any(gag => gag.Name == gagType))
            .Select(gagType => _gagService.GagTypes.First(gag => gag.Name == gagType))
            .ToList();
    }

    public string ProcessMessage(string IPAspacedMessage) {
        string outputStr = "";
        try {
            // Get the first Gag object from _activeGags
            var firstGag = _activeGags.FirstOrDefault();
            if (firstGag != null) {
                outputStr = firstGag.ConvertIPAtoGagSpeak(IPAspacedMessage);
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"Error processing message with gag {_activeGags.FirstOrDefault()}: {e.Message}");
        }
        return outputStr;
    }
}