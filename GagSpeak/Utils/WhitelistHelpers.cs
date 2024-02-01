using System.Linq;
using GagSpeak.CharacterData;

namespace GagSpeak.Utility;

/// <summary> A collective helper class for going through the configurations Whitelist of whitlistcharacter objects </summary>
public static class WhitelistHelpers {
    /// <summary> Checks if the player is in the whitelist </summary>
    public static bool IsPlayerInWhitelist(string playerName, GagSpeakConfig config) {
        return config.whitelist.Any(x => x._name == playerName);
    }

    // get the location in the whitelist where a player is at
    public static int GetWhitelistIndex(string playerName, GagSpeakConfig config) {
        return config.whitelist.FindIndex(x => x._name == playerName);
    }

    // helper for if the index is within the bounds of the whitelist
    public static bool IsIndexWithinBounds(int index, GagSpeakConfig config) {
        return index >= 0 && index < config.whitelist.Count;
    }


    // helper for adding a new item to the whitelist
    public static void AddNewWhitelistItem(string playerName, string playerWorld, string relationshipStatus, GagSpeakConfig config) {
        // add the whitelist entry
        config.whitelist.Add(new WhitelistedCharacterInfo(playerName, playerWorld, relationshipStatus));
        // then update the values in the playerInfo which are stored as lists to match these permissions
        config.playerInfo._grantExtendedLockTimes.Add(false);
        config.playerInfo._triggerPhraseForPuppeteer.Add(""); // blank trigger words are not processed
        // save the information
        config.Save();
    }

    // replace the whitelist item at index with new whitelist item
    public static void ReplaceWhitelistItem(int index, string playerName, string playerWorld, GagSpeakConfig config) {
        // replace the whitelist entry
        config.whitelist[index] = new WhitelistedCharacterInfo(playerName, playerWorld, "None");
        // update the player info at that index too
        config.playerInfo._grantExtendedLockTimes[index] = false;
        config.playerInfo._triggerPhraseForPuppeteer[index] = "";
        // save the information
        config.Save();
    }

    // helper for removing an item from the whitelist
    public static void RemoveWhitelistItem(int index, GagSpeakConfig config) {
        // remove the whitelist entry
        config.whitelist.RemoveAt(index);
        // then update the values in the playerInfo which are stored as lists to match these permissions
        config.playerInfo._grantExtendedLockTimes.RemoveAt(index);
        config.playerInfo._triggerPhraseForPuppeteer.RemoveAt(index);
        // save the information
        config.Save();
    }

    public static void RemoveWhitelistItemAtIndex(int index, GagSpeakConfig config) {
        // remove the whitelist entry
        config.whitelist.RemoveAt(index);
        // then update the values in the playerInfo which are stored as lists to match these permissions
        config.playerInfo._grantExtendedLockTimes.RemoveAt(index);
        config.playerInfo._triggerPhraseForPuppeteer.RemoveAt(index);
        // save the information
        config.Save();
    }
}