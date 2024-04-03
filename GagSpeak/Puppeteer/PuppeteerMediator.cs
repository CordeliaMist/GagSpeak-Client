using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using System.Linq;
using System.Text.RegularExpressions;
using Emote = Lumina.Excel.GeneratedSheets.Emote;
using GagSpeak.Utility;

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
        var match = MatchTriggerWord(messageRecieved, triggerWord);
        if (match.Success) {
            string remainingMessage = messageRecieved.Substring(match.Index + match.Length).Trim();
            remainingMessage = GetGlobalSubstringWithinParentheses(remainingMessage);
            if (remainingMessage != null) {
                remainingMessage = ConvertSquareToAngleBrackets(remainingMessage);
                puppeteerMessageToSend = remainingMessage;
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
        // if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(SenderName, out int whitelistCharIdx))
        {
            string triggerWords = _characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._triggerPhraseForPuppeteer;
            string[] triggerWordArray = triggerWords.Split('|');

            foreach (string triggerWord in triggerWordArray)
            {
                if (string.IsNullOrEmpty(triggerWord) || string.IsNullOrWhiteSpace(triggerWord) || triggerWord == "" || triggerWord == " ")
                {
                    continue;
                }
                // now that we have our trigger word, see if the trigger word exists within our message
                var match = MatchTriggerWord(messageRecieved, triggerWord);
                if (match.Success)
                {
                    string remainingMessage = messageRecieved.Substring(match.Index + match.Length).Trim();
                    remainingMessage = GetSubstringWithinParentheses(remainingMessage, whitelistCharIdx);
                    if (remainingMessage != null)
                    {
                        remainingMessage = ConvertSquareToAngleBrackets(remainingMessage);
                        puppeteerMessageToSend = remainingMessage;
                        GSLogger.LogType.Debug($"[PuppeteerMediator]: Index of Whitelisted Char in found Match: {whitelistCharIdx}");
                        return true;
                    }
                }
            }
        }
        // we must set it to something, so set it to an empty string
        puppeteerMessageToSend = string.Empty;
        return false;
    }

    public bool MeetsSettingCriteria(string SenderName, SeString messageRecieved) {
        // if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(SenderName, out int whitelistCharIdx))
        {
            // At this point, our main concern is if the message to play is within the parameters of the settings we set
            // for the player. If the player has the setting enabled, then we can proceed.
            if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowSitRequests) {
                if(messageRecieved.TextValue == "sit" || messageRecieved.TextValue == "groundsit")
                {
                    GSLogger.LogType.Debug($"[PuppeteerMediator]: valid sit command");
                    return true;
                } else {
                    GSLogger.LogType.Debug($"[PuppeteerMediator]: not a sit command");
                }
            }
            if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowMotionRequests) {
                // we can check to see if it is a valid emote
                var emotes = _dataManager.GetExcelSheet<Emote>();
                if(emotes != null){
                    // check if the message matches any emotes from that sheet
                    foreach (var emote in emotes) {
                        if (messageRecieved.TextValue == emote.Name.RawString.Replace(" ", "").ToLower()) {
                            GSLogger.LogType.Debug($"[PuppeteerMediator]: valid emote command");
                            // then it is an emote, and we have enabled that option, so return true
                            return true;
                        }
                    }
                    GSLogger.LogType.Debug($"[PuppeteerMediator]: not a valid emote!");
                }
            }
            if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowAllCommands) {
                GSLogger.LogType.Debug($"[PuppeteerMediator]: valid all type command order");
                return true;
            }
        }
        // if we reach here, it means we dont meet the criteria
        GSLogger.LogType.Debug($"[PuppeteerMediator]: not a valid command, or all commands is not active");
        return false;   
    }

    public bool MeetsGlobalSettingCriteria(SeString messageRecieved) {
        // At this point, our main concern is if the message to play is within the parameters of the settings we set
        // for the player. If the player has the setting enabled, then we can proceed.
        if(_characterHandler.playerChar._globalAllowSitRequests) {
            if(messageRecieved.TextValue == "sit" || messageRecieved.TextValue == "groundsit") {
                GSLogger.LogType.Debug($"[PuppeteerMediator]: valid sit command");
                return true;
            } else {
                GSLogger.LogType.Debug($"[PuppeteerMediator]: not a sit command");
            }
        }
        if(_characterHandler.playerChar._globalAllowMotionRequests) {
            // we can check to see if it is a valid emote
            var emotes = _dataManager.GetExcelSheet<Emote>();
            if(emotes != null){
                // check if the message matches any emotes from that sheet
                foreach (var emote in emotes) {
                    if (messageRecieved.TextValue == emote.Name.RawString.Replace(" ", "").ToLower()) {
                        GSLogger.LogType.Debug($"[PuppeteerMediator]: valid emote command");
                        // then it is an emote, and we have enabled that option, so return true
                        return true;
                    }
                }
                GSLogger.LogType.Debug($"[PuppeteerMediator]: not a valid emote!");
            }
        }
        if(_characterHandler.playerChar._globalAllowAllCommands) {
            GSLogger.LogType.Debug($"[PuppeteerMediator]: valid all type command order");
            return true;
        }
        // if we reach here, it means we dont meet the criteria
        GSLogger.LogType.Debug($"[PuppeteerMediator]: not a valid command, or all commands is not active");
        return false;   
    }

    public SeString ConvertAliasCommandsIfAny(string SenderName, string puppeteerMessageToSend) {
        // if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(SenderName, out int whitelistCharIdx))
        {
            // now we can use this index to scan our aliasLists
            AliasList aliasListToScan = _characterHandler.playerChar._triggerAliases[whitelistCharIdx];
            // Sort the aliases by length in descending order, to ensure longer equivalent input variants are taken before shorter ones.
            var sortedAliases = aliasListToScan._aliasTriggers.OrderByDescending(alias => alias._inputCommand.Length);
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
            GSLogger.LogType.Debug($"[PuppeteerMediator]: New Message: {puppeteerMessageToSend}");
        }
        return puppeteerMessageToSend;
    }

    /// <summary> encapsulates the puppeteer command within '(' and ')' </summary>
    private string GetSubstringWithinParentheses(string str, int indexOfWhitelistedChar) {
        int startIndex = str.IndexOf(_characterHandler.playerChar._uniquePlayerPerms[indexOfWhitelistedChar]._StartCharForPuppeteerTrigger);
        int endIndex = str.IndexOf(_characterHandler.playerChar._uniquePlayerPerms[indexOfWhitelistedChar]._EndCharForPuppeteerTrigger);

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

    private Match MatchTriggerWord(string message, string triggerWord)
    {
        var triggerRegex = $@"(?<=^|\s){triggerWord}(?=[^a-z])";
        return Regex.Match(message, triggerRegex);
    }
}
