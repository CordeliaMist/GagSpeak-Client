using GagSpeak.CharacterData;

namespace GagSpeak.Utility;

/// <summary> A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements </summary>
public static class AltCharHelpers
{
    private static CharacterHandler charHandle = GagSpeak._services.GetService<CharacterHandler>();

    /// <summary> Retrieve the added character's name (tuple index 0) </summary>
    public static string FetchOriginalName(int idx) => charHandle.whitelistChars[idx]._charNAW[0]._name;

    /// <summary> Retrieve the added character's homeworld (tuple index 0) at a spesified index </summary>
    public static string FetchOriginalWorld(int idx) => charHandle.whitelistChars[idx]._charNAW[0]._homeworld;

    /// <summary> Fetch original name of current whitelist active Idx </summary>
    public static string FetchActiveIdxOriginalName() => FetchOriginalName(charHandle.activeListIdx);

    /// <summary> Fetch original world of current whitelist active Idx </summary>
    public static string FetchActiveIdxOriginalWorld() => FetchOriginalWorld(charHandle.activeListIdx);

    /// <summary> Fetch the name of the character at the specified index and tuple index </summary>
    public static string FetchName(int idx, int tupleIdx) => charHandle.whitelistChars[idx]._charNAW[tupleIdx]._name;

    /// <summary> Fetch the homeworld of the character at the specified index and tuple index </summary>
    public static string FetchWorld(int idx, int tupleIdx) => charHandle.whitelistChars[idx]._charNAW[tupleIdx]._homeworld;

    /// <summary> Fetch the name of the character based on the activeIdx and the charNAWIdxToProcess </summary>
    public static string FetchCurrentName() => FetchName(charHandle.activeListIdx, charHandle.whitelistChars[charHandle.activeListIdx]._charNAWIdxToProcess);

    /// <summary> Fetch the homeworld of the character based on the activeIdx and the charNAWIdxToProcess </summary>
    public static string FetchCurrentWorld() => FetchWorld(charHandle.activeListIdx, charHandle.whitelistChars[charHandle.activeListIdx]._charNAWIdxToProcess);


    /// <summary> See if a name is located anywhere in the tuple list of the spesified index, and if so return that index </summary>
    public static bool IsNameInTupleAtIndex(int whitelistIdx, string name, out int tupleIdx) {
        for (int i = 0; i < charHandle.whitelistChars[whitelistIdx]._charNAW.Count; i++) {
            if (charHandle.whitelistChars[whitelistIdx]._charNAW[i]._name == name) {
                tupleIdx = i;
                return true;
            }
        }
        tupleIdx = -1;
        return false;
    }

    /// <summary> See if a name is located anywhere in the whitelist characters </summary>
    public static bool IsPlayerInWhitelist(string playerName) {
        // for each whitelisted character, check if the player name is in the list of names
        for (int i = 0; i < charHandle.whitelistChars.Count; i++)
        {
            // if the player name is found in the list of names, set the indexes and return true
            for (int j = 0; j < charHandle.whitelistChars[i]._charNAW.Count; j++)
            {
                // if the player name is found in the list of names, set the indexes and return true
                if (charHandle.whitelistChars[i]._charNAW[j]._name == playerName) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary> See if a name is located anywhere in the whitelist characters </summary>
    public static bool IsPlayerInWhitelist(string playerName, out int whitelistCharIdx) {
        // initial vars for the whitelist index and name index in the list at the whitelisted character it is found in.
        whitelistCharIdx = -1;
        // for each whitelisted character, check if the player name is in the list of names
        for (int i = 0; i < charHandle.whitelistChars.Count; i++)
        {
            // if the player name is found in the list of names, set the indexes and return true
            for (int j = 0; j < charHandle.whitelistChars[i]._charNAW.Count; j++)
            {
                // if the player name is found in the list of names, set the indexes and return true
                if (charHandle.whitelistChars[i]._charNAW[j]._name == playerName) {
                    whitelistCharIdx = i;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary> See if a name is located anywhere in the whitelist characters </summary>
    public static bool IsPlayerInWhitelist(string playerName, out int whitelistCharIdx, out int CharNameIdx) {
        // initial vars for the whitelist index and name index in the list at the whitelisted character it is found in.
        whitelistCharIdx = -1;
        CharNameIdx = -1;
        // for each whitelisted character, check if the player name is in the list of names
        for (int i = 0; i < charHandle.whitelistChars.Count; i++)
        {
            // if the player name is found in the list of names, set the indexes and return true
            for (int j = 0; j < charHandle.whitelistChars[i]._charNAW.Count; j++)
            {
                // if the player name is found in the list of names, set the indexes and return true
                if (charHandle.whitelistChars[i]._charNAW[j]._name == playerName) {
                    whitelistCharIdx = i;
                    CharNameIdx = j;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary> Retrieve the targetplayerNameFormat of a character at specified whitelist idx and tuple idx </summary>
    public static string FetchNameWorldFormatByTupleIdx(int whitelistIdx, int tupleIdx)
    {
        string ret = "";
        ret += charHandle.whitelistChars[whitelistIdx]._charNAW[tupleIdx]._name;
        ret += "@";
        ret += charHandle.whitelistChars[whitelistIdx]._charNAW[tupleIdx]._homeworld;
        return ret;
    }

    /// <summary> Retrieve the targetplayerNameFormat of a character at specified whitelist idx <summary>
    public static string FetchNameWorldFormatByWhitelistIdxForNAWIdxToProcess(int whitelistIdx)
    {
        string ret = "";
        ret += charHandle.whitelistChars[whitelistIdx]._charNAW[charHandle.whitelistChars[whitelistIdx]._charNAWIdxToProcess]._name;
        ret += "@";
        ret += charHandle.whitelistChars[whitelistIdx]._charNAW[charHandle.whitelistChars[whitelistIdx]._charNAWIdxToProcess]._homeworld;
        return ret;
    }

}

