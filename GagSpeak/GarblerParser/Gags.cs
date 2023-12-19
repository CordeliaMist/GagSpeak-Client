using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Data;
using GagSpeak.Services;
using GagSpeak.Garbler.PhonemeData;

namespace GagSpeak.Data;
public class Gag
{
    private readonly    GagSpeakConfig                  _config;                            // The GagSpeak configuration
    public              string                          Name { get; set; } = "";            // Name of the gag
    public              GagEnums.LipPos                 LipPos { get; set; }                // What Position has the gag locked the lips in?
    public              GagEnums.RestrictionLvl         LipRestriction { get; set; }        // Restriction of the lips
    public              GagEnums.TonguePos              TonguePos { get; set; }             // Restricted Position of the tongue caused by the gag
    public              GagEnums.RestrictionLvl         TongueRestriction { get; set; }     // How restricted the tongue is by the gag. How much can it move?
    public              GagEnums.RestrictionLvl         JawRestriction { get; set; }        // Restriction of the jaw, how much can it open?
    public              GagEnums.RestrictionLvl         PackedMouthSeverity { get; set; }   // Severity of the packed mouth, how stuffed full is it? (useful for pump gags)
    private             List<GagEnums.RestrictionLvl>   PhoneticRestrictions;               // List of the restriction severity for each phoneme in the current phoneme master list

    public Gag(GagSpeakConfig config) {
        _config = config;
        // set up the restrictions
        PhoneticRestrictions = new List<GagEnums.RestrictionLvl>();
    }

    // Adding this stupid info here because it's being a butt and i dont want to put up with this things sorry ass right now.
    public void AddInfo(string name, GagEnums.LipPos lipPos, GagEnums.RestrictionLvl lipRestriction, GagEnums.TonguePos tonguePos,
    GagEnums.RestrictionLvl tongueRestriction, GagEnums.RestrictionLvl jawRestriction, GagEnums.RestrictionLvl packedMouthSeverity) {
        Name = name;
        LipPos = lipPos;
        LipRestriction = lipRestriction;
        TonguePos = tonguePos;
        TongueRestriction = tongueRestriction;
        JawRestriction = jawRestriction;
        PackedMouthSeverity = packedMouthSeverity;
        InitializePhoneticRestrictions(); // Optionally reinitialize restrictions if needed
    }

    /// <summary>
    /// Initializes the PhoneticRestrictions list based on the currently selected phoneme master list
    /// </summary>
    public void InitializePhoneticRestrictions() {
        foreach (var phoneticSymbol in _config.phoneticSymbolList) {
            PhoneticRestrictions.Add(CalculateRestriction(phoneticSymbol));
        }
    }

    /// <summary>
    /// Refreshes the restricted list identifiers based on the currently selected phoneme master list after it is updated. Should be called by a server/event
    /// </summary>
    public void RefreshPhoneticRestrictions() {
        PhoneticRestrictions.Clear();
        InitializePhoneticRestrictions();
    }

    public GagEnums.RestrictionLvl CalculateRestriction(PhoneticSymbol phoneticSymbol) {
        // Logic to calculate the GagEnums.RestrictionLvl based on the phonetic symbol and the gag attributes
        // This will likely involve a series of if/else or switch statements to check the phonetic symbol's properties and the gag's properties
        // and return the appropriate GagEnums.RestrictionLvl
        return GagEnums.RestrictionLvl.None;
    }

    /// <summary>
    /// Converts the given IPA spaced message into a gagged message based on the current gag's attributes
    /// </summary>
    /// <param name="IPAspacedMessage"></param> Message will be structured like "h-ɛ-m-ɪ-ŋ-ɝ". The hyphens are simply to seperate the phonems, and are removed after they are iterated through
    /// <returns></returns>
    public string ConvertIPAtoGagSpeak(string IPAspacedMessage) {
        string output = "";
        string[] words = IPAspacedMessage.Split(' ');

        foreach (string word in words) {
            string[] phonemes = word.Split('-');

            foreach (string phoneme in phonemes) {
                int index = _config.phoneticSymbolList.FindIndex(symbol => symbol.Phoneme == phoneme);
                GagEnums.RestrictionLvl restriction = PhoneticRestrictions[index];
                // switch case based on restriction level to determine mufflesound
                switch(restriction) {
                    case GagEnums.RestrictionLvl.None:
                        output += phoneme;
                        break;
                    case GagEnums.RestrictionLvl.Partial:
                        output += _config.phoneticSymbolList[index].PartialMuffledSound;
                        break;
                    case GagEnums.RestrictionLvl.Complete:
                        output += _config.phoneticSymbolList[index].FullMuffledSound;
                        break;
                    case GagEnums.RestrictionLvl.Silenced:
                        output += _config.phoneticSymbolList[index].TotalMuffledSound;
                        break;
                }
            }
            output += " ";
        }

        return output.TrimEnd();
    }
}
