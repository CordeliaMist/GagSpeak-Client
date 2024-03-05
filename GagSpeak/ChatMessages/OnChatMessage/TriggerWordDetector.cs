using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.ChatMessages;
/// <summary>
/// Used for checking messages send to the games chatbox, not meant for detouring or injection
/// Messages passed through here are scanned to see if they are encoded, for puppeteer, or include any hardcore features.
public class TriggerWordDetector
{
    private readonly    GagSpeakConfig         _config;                            // config from GagSpeak
    private readonly    PuppeteerMediator      _puppeteerMediator;                 // puppeteer mediator

    /// <summary> This is the constructor for the OnChatMsgManager class. </summary>
    public TriggerWordDetector(GagSpeakConfig config, PuppeteerMediator puppeteerMediator) {
        _config = config;
        _puppeteerMediator = puppeteerMediator;
    }

    public bool IsValidGlobalTriggerWord(SeString chatmessage, XivChatType type, out SeString messageToSend) {
        // create the string that will be sent out
        messageToSend = new SeString();
        // see if it contains your trigger word for them
        if(_puppeteerMediator.ContainsGlobalTriggerWord(chatmessage.TextValue, out string globalPuppeteerMessageToSend)) {
            // contained the trigger word, so process it.
            if(globalPuppeteerMessageToSend != string.Empty) {
                // set the message to send
                messageToSend = globalPuppeteerMessageToSend;
                // now get the incoming chattype converted to our chat channel,
                ChatChannel.ChatChannels? incomingChannel = ChatChannel.GetChatChannelFromXivChatType(type);
                // if it isnt any of our active channels then we just dont wanna even process it
                if(incomingChannel != null) {
                    // it isnt null meaning it is eithing the channels so now we can check if it meets the criteria
                    if(_config.ChannelsPuppeteer.Contains(incomingChannel.Value)
                    && _puppeteerMediator.MeetsGlobalSettingCriteria(messageToSend))
                    {
                        return true;
                    } else {
                        GSLogger.LogType.Debug($"[TriggerWordDetector] Not an Enabled Chat Channel, or command didnt abide by your settings aborting");
                        return false;
                    }
                } else {
                    GSLogger.LogType.Debug($"[TriggerWordDetector] Not an Enabled Chat Channel, aborting");
                    return false;
                }
            } else {
                GSLogger.LogType.Debug($"[TriggerWordDetector] Puppeteer message to send was empty, aborting");
                return false;
            }
        } else {
            return false;
        }
    }
        
    public bool IsValidPuppeteerTriggerWord(string senderName, SeString chatmessage, XivChatType type, ref bool isHandled, out SeString messageToSend) {
        // create the string that will be sent out
        messageToSend = new SeString();
        // see if it contains your trigger word for them
        if(_puppeteerMediator.ContainsTriggerWord(senderName, chatmessage.TextValue, out string puppeteerMessageToSend)){
            if(puppeteerMessageToSend != string.Empty) {
                // apply any alias translations, if any
                messageToSend = _puppeteerMediator.ConvertAliasCommandsIfAny(senderName, puppeteerMessageToSend);
                // now get the incoming chattype converted to our chat channel,
                ChatChannel.ChatChannels? incomingChannel = ChatChannel.GetChatChannelFromXivChatType(type);
                // if it isnt any of our active channels then we just dont wanna even process it
                if(incomingChannel != null) {
                    // it isnt null meaning it is eithing the channels so now we can check if it meets the criteria
                    if(_config.ChannelsPuppeteer.Contains(incomingChannel.Value)) {
                        if(_puppeteerMediator.MeetsSettingCriteria(senderName, messageToSend)) {
                            return true;
                        } else {
                            GSLogger.LogType.Debug($"[TriggerWordDetector] Command didnt abide by your settings aborting");
                            return false;
                        }
                    } else {
                        GSLogger.LogType.Debug($"[TriggerWordDetector] Not an Enabled Chat Channel, aborting");
                        return false;
                    } 
                } else {
                    GSLogger.LogType.Debug($"[TriggerWordDetector] Not an Enabled Chat Channel, aborting");
                    return false;
                }
            } else {
                GSLogger.LogType.Debug($"[TriggerWordDetector] Puppeteer message to send was empty, aborting");
                return false;
            }
        } else {
            return false;
        }
    }
}