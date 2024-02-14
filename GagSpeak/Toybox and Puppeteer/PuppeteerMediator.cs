using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using Emote = Lumina.Excel.GeneratedSheets.Emote;

namespace GagSpeak.ToyboxandPuppeteer;
// a mediator for holding timer information and tracks when a pattern has been saved
public class PuppeteerMediator
{
    private readonly CharacterHandler _characterHandler;
    private readonly IDataManager _dataManager;

    public PuppeteerMediator(CharacterHandler characterHandler, IDataManager dataManager) {
        _characterHandler = characterHandler;
        _dataManager = dataManager;
    }

    /// <summary> Checks if the message contains the global trigger word </summary>
    /// <returns> True if the message contains the trigger word, false if not </returns>
    public bool ContainsGlobalTriggerWord(string messageRecieved, out string puppeteerMessageToSend) {
        string triggerWord = _characterHandler.playerChar._globalTriggerPhrase;
        if(string.IsNullOrEmpty(triggerWord) || string.IsNullOrWhiteSpace(triggerWord) || triggerWord == "" || triggerWord == " ") {
            puppeteerMessageToSend = string.Empty;
            return false;
        }
        // now that we have our trigger word, see if the trigger word exists within our message
        if (messageRecieved.Contains(triggerWord)) {
            string remainingMessage = messageRecieved.Substring(messageRecieved.IndexOf(triggerWord) + triggerWord.Length).Trim();
            remainingMessage = GetGlobalSubstringWithinParentheses(remainingMessage);
            if (remainingMessage != null) {
                remainingMessage = ConvertSquareToAngleBrackets(remainingMessage);
                puppeteerMessageToSend = remainingMessage;
                GagSpeak.Log.Debug($"[PuppeteerMediator]: New Message: {puppeteerMessageToSend}");
                return true;
            }
        }
        // we must set it to something, so set it to an empty string
        puppeteerMessageToSend = string.Empty;
        return false;
    }


    /// <summary> Checks if whitelisted players message contains your trigger word </summary>
    /// <returns> True if the message contains the trigger word, false if not </returns>
    public bool ContainsTriggerWord(string SenderName, string messageRecieved, out string puppeteerMessageToSend) {
        int indexOfWhitelistedChar = _characterHandler.GetWhitelistIndex(SenderName); // our temp name in our whitelist
        // if the index is -1, then the sender is not whitelisted
        if (indexOfWhitelistedChar == -1) {
            // if user is not in whitelist, exit early.
            puppeteerMessageToSend = string.Empty;
            return false;
        }
        GagSpeak.Log.Debug($"[PuppeteerMediator]: Index of Whitelisted Char: {indexOfWhitelistedChar}");
        string triggerWords = _characterHandler.playerChar._triggerPhraseForPuppeteer[indexOfWhitelistedChar];
        string[] triggerWordArray = triggerWords.Split('|');

        foreach (string triggerWord in triggerWordArray) {
            if(string.IsNullOrEmpty(triggerWord) || string.IsNullOrWhiteSpace(triggerWord) || triggerWord == "" || triggerWord == " ") {
                continue;
            }
            GagSpeak.Log.Debug($"[PuppeteerMediator]: Trigger Word: {triggerWord}");

            // now that we have our trigger word, see if the trigger word exists within our message
            if (messageRecieved.Contains(triggerWord)) {
                string remainingMessage = messageRecieved.Substring(messageRecieved.IndexOf(triggerWord) + triggerWord.Length).Trim();
                remainingMessage = GetSubstringWithinParentheses(remainingMessage, indexOfWhitelistedChar);
                if (remainingMessage != null) {
                    remainingMessage = ConvertSquareToAngleBrackets(remainingMessage);
                    puppeteerMessageToSend = remainingMessage;
                    GagSpeak.Log.Debug($"[PuppeteerMediator]: New Message: {puppeteerMessageToSend}");
                    return true;
                }
            }
        }
        // we must set it to something, so set it to an empty string
        puppeteerMessageToSend = string.Empty;
        return false;
    }

    public bool MeetsSettingCriteria(string SenderName, SeString messageRecieved) {
        int indexOfWhitelistedChar = _characterHandler.GetWhitelistIndex(SenderName); // our temp name in our whitelist
        // if the index is -1, then the sender is not whitelisted
        if (indexOfWhitelistedChar == -1) { return false; }
        // At this point, our main concern is if the message to play is within the parameters of the settings we set
        // for the player. If the player has the setting enabled, then we can proceed.
        if(_characterHandler.playerChar._allowSitRequests[indexOfWhitelistedChar]) {
            if(messageRecieved.TextValue == "sit" || messageRecieved.TextValue == "groundsit") {
                GagSpeak.Log.Debug($"[PuppeteerMediator]: valid sit command");
                return true;
            } else {
                GagSpeak.Log.Debug($"[PuppeteerMediator]: not a sit command");
            }
        }
        if(_characterHandler.playerChar._allowMotionRequests[indexOfWhitelistedChar]) {
            // we can check to see if it is a valid emote
            var emotes = _dataManager.GetExcelSheet<Emote>();
            if(emotes != null){
                // check if the message matches any emotes from that sheet
                foreach (var emote in emotes) {
                    if (messageRecieved.TextValue == emote.Name.RawString.ToLower()) {
                        GagSpeak.Log.Debug($"[PuppeteerMediator]: valid emote command");
                        // then it is an emote, and we have enabled that option, so return true
                        return true;
                    }
                }
                GagSpeak.Log.Debug($"[PuppeteerMediator]: not a valid emote!");
            }
        }
        if(_characterHandler.playerChar._allowAllCommands[indexOfWhitelistedChar]) {
            GagSpeak.Log.Debug($"[PuppeteerMediator]: valid all type command order");
            return true;
        }
        // if we reach here, it means we dont meet the criteria
        GagSpeak.Log.Debug($"[PuppeteerMediator]: not a valid command, or all commands is not active");
        return false;   
    }

    public bool MeetsGlobalSettingCriteria(SeString messageRecieved) {
        // At this point, our main concern is if the message to play is within the parameters of the settings we set
        // for the player. If the player has the setting enabled, then we can proceed.
        if(_characterHandler.playerChar._globalAllowSitRequests) {
            if(messageRecieved.TextValue == "sit" || messageRecieved.TextValue == "groundsit") {
                GagSpeak.Log.Debug($"[PuppeteerMediator]: valid sit command");
                return true;
            } else {
                GagSpeak.Log.Debug($"[PuppeteerMediator]: not a sit command");
            }
        }
        if(_characterHandler.playerChar._globalAllowMotionRequests) {
            // we can check to see if it is a valid emote
            var emotes = _dataManager.GetExcelSheet<Emote>();
            if(emotes != null){
                // check if the message matches any emotes from that sheet
                foreach (var emote in emotes) {
                    if (messageRecieved.TextValue == emote.Name.RawString.ToLower()) {
                        GagSpeak.Log.Debug($"[PuppeteerMediator]: valid emote command");
                        // then it is an emote, and we have enabled that option, so return true
                        return true;
                    }
                }
                GagSpeak.Log.Debug($"[PuppeteerMediator]: not a valid emote!");
            }
        }
        if(_characterHandler.playerChar._globalAllowAllCommands) {
            GagSpeak.Log.Debug($"[PuppeteerMediator]: valid all type command order");
            return true;
        }
        // if we reach here, it means we dont meet the criteria
        GagSpeak.Log.Debug($"[PuppeteerMediator]: not a valid command, or all commands is not active");
        return false;   
    }

    public SeString ConvertAliasCommandsIfAny(string SenderName, string puppeteerMessageToSend) {
        // as a final step, let's check your alias list for the player, and translate any aliases you have set for them
        int indexOfWhitelistedChar = _characterHandler.GetWhitelistIndex(SenderName); // our temp name in our whitelist
        // we dont really need to do this check, but im being safe
        if (indexOfWhitelistedChar == -1) { return puppeteerMessageToSend; }
        // now we can use this index to scan our aliasLists
        AliasList aliasListToScan = _characterHandler.playerChar._triggerAliases[indexOfWhitelistedChar];
        // see if our message contains any of the alias strings. For it to match, it must match the full alias string.
        foreach (AliasTrigger alias in aliasListToScan._aliasTriggers) {
            // if the alias is enabled
            if (alias._enabled 
            && !string.IsNullOrWhiteSpace(alias._inputCommand) 
            && !string.IsNullOrWhiteSpace(alias._outputCommand) 
            && puppeteerMessageToSend.Contains(alias._inputCommand))
            {
                // replace the alias command with the output command
                puppeteerMessageToSend = puppeteerMessageToSend.Replace(alias._inputCommand, alias._outputCommand);
            }
        }
        GagSpeak.Log.Debug($"[PuppeteerMediator]: New Message: {puppeteerMessageToSend}");
        return puppeteerMessageToSend;
    }

    /// <summary> encapsulates the puppeteer command within '(' and ')' </summary>
    private string GetSubstringWithinParentheses(string str, int indexOfWhitelistedChar) {
        int startIndex = str.IndexOf(_characterHandler.playerChar._StartCharForPuppeteerTrigger[indexOfWhitelistedChar]);
        int endIndex = str.IndexOf(_characterHandler.playerChar._EndCharForPuppeteerTrigger[indexOfWhitelistedChar]);

        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex) {
            return str.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        }
        return str;
    }

    /// <summary> encapsulates the puppeteer command within '(' and ')' </summary>
    private string GetGlobalSubstringWithinParentheses(string str) {
        int startIndex = str.IndexOf('(');
        int endIndex = str.IndexOf(')');

        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex) {
            return str.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        }
        return str;
    }

    /// <summary> Converts square brackets to angle brackets </summary>
    private string ConvertSquareToAngleBrackets(string str) => str.Replace("[", "<").Replace("]", ">");
}
