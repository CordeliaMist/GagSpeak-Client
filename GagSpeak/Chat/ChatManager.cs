using System;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using XivCommon.Functions;
using System.Diagnostics;
using OtterGui.Classes;
namespace GagSpeak.Chat;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class ChatManager
{
    private readonly IChatGui _clientChat;
    private readonly GagSpeakConfig _config;
    private readonly IClientState _clientState;
    private readonly IObjectTable _objectTable;
    private readonly RealChatInteraction _realChatInteraction;
    private readonly IFramework _framework; // framework from XIVClientStructs
    private Queue<string> messageQueue = new Queue<string>();
    private Stopwatch messageTimer = new Stopwatch();

    // future future note: the chat handling came from simpletweaks i found out after enough digging, and they have some other fancy inturruptions,
    // that could possibly make you not need to use /gsm at all.

    public ChatManager(IChatGui clientChat, GagSpeakConfig config, IClientState clientState, IObjectTable objectTable,
                RealChatInteraction realChatInteraction, IFramework framework) {
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _objectTable = objectTable;
        _realChatInteraction = realChatInteraction;
        _framework = framework;

        // begin our framework check
        _framework.Update += framework_Update;
        // Begin our OnChatMessage Detection
        _clientChat.CheckMessageHandled += Chat_OnCheckMessageHandled;
        _clientChat.ChatMessage += Chat_OnChatMessage;
        }

    public void Dispose() {
        _framework.Update -= framework_Update;
        _clientChat.CheckMessageHandled -= Chat_OnCheckMessageHandled;
        _clientChat.ChatMessage -= Chat_OnChatMessage;
    }

    // Helper function List that stores all of the encodeed message keywords
    List<string> uniqueStringIdentifiers = new List<string> {
        "over your mouth as the", // gag apply
        "from her pocket and uses it to lock your", // gag lock
        "from her pocket and sets the combination password to", // gag lock password
        "reaches behind your neck, taking off the lock that was keeping your", // gag unlock
        "reaches behind your neck and sets the password to", // gag unlock password
        "reaches behind your neck and unfastens the buckle of your", // gag remove
        "reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.", // gag removeall
    };

    // Helper function to check if the tell is encoded for the plugin or not
    private bool IsEncodedMessage(string textVal) {
        // the gag apply encoded message
        if (textVal.Contains("from") && textVal.Contains("applies a")
            && textVal.Contains("over your mouth as the") && textVal.Contains("layer of your concealment*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING/INCOMING /gag ENCODED TELL");
            return true;
        // the gag lock encoded message and lock password
        } else if (textVal.Contains("from") && textVal.Contains("takes out a") &&
            ((textVal.Contains("from her pocket and sets the combination password to") &&
            textVal.Contains("before locking your") && textVal.Contains("layer gag*")) || (
            textVal.Contains("from her pocket and uses it to lock your") && textVal.Contains("gag*")))) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag lock ENCODED TELL");
            return true;
        // the gag unlock and unlock password encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
            ((textVal.Contains("and sets the password to") && textVal.Contains("on your") &&
            textVal.Contains("layer gagstrap, unlocking it.*")) || (
            textVal.Contains(", taking off the lock that was keeping your") &&
            textVal.Contains("gag layer fastened nice and tight.*")))) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag unlock ENCODED TELL");
            return true;
        // the gag remove encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
            textVal.Contains("and unfastens the buckle of your") &&
            textVal.Contains("gag layer strap, allowing your voice to be a little clearer.*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag remove ENCODED TELL");
            return true;
        // the gag removeall encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
            textVal.Contains("and unbuckles all of your gagstraps, allowing you to speak freely once more.*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag removeall ENCODED TELL");
            return true;
        }
        // any other message that isnt encoded.
        return false;
    }


    // FOR NOW EVERYTHING WILL BE STUFFED INTO HERE, AND LATER DIVIDED OUT INTO THE OTHER CHATS
    private void Chat_OnCheckMessageHandled(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool isHandled) {
        // we will want to make sure that if our message contains a combination of all words from each of our encoded message strings, to hide it entirely
        var textVal = message.TextValue;
        // See if it is an outgoing tell
        if ( type == XivChatType.TellOutgoing) {
            // Scan if the message contains all words from the /gag encoded tell
            if(IsEncodedMessage(textVal)) {
                // its the incoded message, so seet handled to true and print debug
                isHandled = true;
                return;
            }
        }
    }

    //// CHATGUI FUNCTIONS: ////
    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
        // NOTE: This may be able to be further optimized if we can find a way to compare sender to playername without doing all this beforehand.        
        // Still unsure about the spesifics of this, comment fully later
        var fmessage = new SeString(new List<Payload>());
        var nline = new SeString(new List<Payload>());
        nline.Payloads.Add(new TextPayload("\n"));
        PlayerPayload playerPayload; // make payload for the player
        List<char> toRemove = new() { //removes special characters in party listings [https://na.finalfantasyxiv.com/lodestone/character/10080203/blog/2891974/]
            '','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',
        };
        var sanitized = sender.ToString(); // convert the sender from SeString to String

        foreach(var c in toRemove) { sanitized = sanitized.Replace(c.ToString(), string.Empty); } // remove all special characters

        // COME BACK TO THIS JUMBLED MESS OF CRAP LATER CORDY IT IS OVER YOUR HEAD ATM
        if (sanitized == _clientState.LocalPlayer?.Name.TextValue) {
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            if (type == XivChatType.CustomEmote) {
                var playerName = new SeString(new List<Payload>());
                playerName.Payloads.Add(new TextPayload(_clientState.LocalPlayer.Name.TextValue));
                fmessage.Append(playerName);
            }
        } 
        else {
            if(type == XivChatType.StandardEmote) {
                playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload ?? 
                                chatmessage.Payloads.FirstOrDefault(x => x is PlayerPayload) as PlayerPayload;
            } 
            else {
                playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload; 
                if (type == XivChatType.CustomEmote) {
                    fmessage.Append(playerPayload.PlayerName);
                }
            }
        }

        fmessage.Append(chatmessage);
        var isEmoteType = type is XivChatType.CustomEmote or XivChatType.StandardEmote;
        if (isEmoteType) {
            fmessage.Payloads.Insert(0, new EmphasisItalicPayload(true));
            fmessage.Payloads.Add(new EmphasisItalicPayload(false));
        }

        var pName = playerPayload == default(PlayerPayload) ? _clientState.LocalPlayer?.Name.TextValue : playerPayload.PlayerName;
        var sName = sender.Payloads.SingleOrDefault( x => x is PlayerPayload) as PlayerPayload; // get the player payload from the sender 
        var senderName = sName?.PlayerName != null ? sName.PlayerName : pName; // if the sender name is not null, set it to the sender name, otherwise set it to the local player name


        // Precursor to condition2, if the message satisfied senderName == PlayerName && XivChatType != _config.CurrentChatType, change it
        if ((pName == senderName) && (_config._allowedChannels.Contains(type)) && (type != _config.CurrentChannel)) {
            _config.CurrentChannel = type; // log the current chatbox channel & save
            _config.Save();
        }

        // FILTER CONDITION TWO:
        //  - Is the chat message an incoming tell? If yes, proceed into the inner function, if not read over
        //    literally everything else that happens inside of it lol.
        if (type == XivChatType.TellIncoming) 
        {
            /* The message was an incoming tell, so now we must check if it is from a friend, party member, or whitelisted player.
            It is important to note that these will word on an OR basis, meaning that if we have FriendsOnly and PartyOnly checked,
            someone who is in your party, but not a friend, can sucessfully trigger the command. Additionally, if none of these
            options are checked, then we will just accept it regardless. [Basically TLDR if any of these are true we should exit] */
            switch (true) {
                // Logic commented on first case, left out on rest. All cases are the same, just with different conditions.
                case var _ when _config.friendsOnly && _config.partyOnly && _config.whitelistOnly: //  all 3 options are checked
                    // If a message is from a friend, or a party member, or a whitelisted player, it will become true,
                    // however, to make sure that we meet a condition that causes this to exit, we put a !() infront, to say
                    // they were a player outside of these parameters while the parameters were checked.
                    if (!(IsFriend(senderName)||IsPartyMember(senderName)||IsWhitelistedPlayer(senderName))) { return; } break;
                
                case var _ when _config.friendsOnly && _config.partyOnly && !_config.whitelistOnly: // When both friend and party are checked
                    if (!(IsFriend(senderName)||IsPartyMember(senderName))) { return; } break;
                
                case var _ when _config.friendsOnly && _config.whitelistOnly && !_config.partyOnly: // When both friend and whitelist are checked
                    if (!(IsFriend(senderName)||IsWhitelistedPlayer(senderName))) { return; } break;
                
                case var _ when _config.partyOnly && _config.whitelistOnly && !_config.friendsOnly: // When both party and whitelist are checked
                    if (!(IsPartyMember(senderName)||IsWhitelistedPlayer(senderName))) { return; } break;

                case var _ when _config.friendsOnly && !_config.partyOnly && !_config.whitelistOnly: // When only friend is checked
                    if (!(IsFriend(senderName))) { return; } break;

                case var _ when _config.partyOnly && !_config.friendsOnly && !_config.whitelistOnly: // When only party is checked
                    if (!(IsPartyMember(senderName))) { return; } break;

                case var _ when _config.whitelistOnly && !_config.friendsOnly && !_config.partyOnly: // When only whitelist is checked
                    if (!(IsWhitelistedPlayer(senderName))) { return; } break;

                default: // None of the filters were checked, so just accept the message anyways because it works for everyone.
                    break;
            }
            
            ////// Once we have reached this point, we know we have recieved a tell, and that it is from one of our filtered players. //////
            GagSpeak.Log.Debug($"Recieved tell from PNAME: {pName} | SNAME: {sName} | SenderName: {senderName} | Message: {fmessage}");

            // if the incoming tell is an encoded message, lets check if we are in dom mode before accepting changes
            if (IsEncodedMessage(chatmessage.TextValue)) {
                if (_config.InDomMode) {
                    GagSpeak.Log.Debug("Player attempted to gag you, but you are in Dominant mode, so ignoring");
                    isHandled = true;
                    return;
                }
            }
            // if we get here, we know we are in sub mode and under right filters.

            // get the type of command given to us based on the disguised message
            // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
            List<string> decodedMessageCommand = DetermineIncomingDiguisedMessageType(fmessage.ToString());

            // function that will determine what happens to the player as a result of the tell.
            if( DetermineMessageOutcome(fmessage.ToString(), decodedMessageCommand, isHandled) ) {
                isHandled = true; // make sure it doesnt display to the chat
            }

            _config.Save(); // save our config
        }
        // skipping to here if it isnt a tell, or it fails any conditions, optimizing the code (hopefully)
    }

    /// <summary>
    /// Oh the things we do for a little extra security...
    /// <para> Does a massive check on the incoming tell, parses out the keywords from the diguised message, and then returns it</para>
    /// </summary>
    /// <param name="recievedMessage"></param>
    /// <returns>A list(string) containing the keywords in the decoded message</returns
    private List<string> DetermineIncomingDiguisedMessageType(string recievedMessage) {
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
        List<string> decodedMessage = new List<string>{"", "", "", "", ""};

        /*if our message was [ *{playerPayload.PlayerName} from {playerPayload.World.Name} applies a {gagType} over your mouth as the {layer} layer of your consealment* ]
        then we will want to appent the string "apply" to our decodedMessage list, and then parse out all the key elements in {} (that can be a string of any length) to
        the decodedMessage string*/
        // unique string for /gag apply == "over your mouth as the"
        if (recievedMessage.Contains(uniqueStringIdentifiers[0])) { // Handle the /gag base command
            decodedMessage[0] = "apply";                // Assign "apply" to decodedMessage[0]
            recievedMessage = recievedMessage.Trim('*');                               // trim off the *'s from the message
            string[] messageParts = recievedMessage.Split("over your mouth as the");   // Message = {playerPayload.PlayerName} from {playerPayload.World.Name} applies a {gagType} && {layer} layer of your concealment
            string trimmedMessage = string.Empty;                                      // setting here for future use
            messageParts[1] = messageParts[1].Replace(" layer of your concealment", "");                 // trim off the "layers of your concealment" from the message     
            decodedMessage[1] = messageParts[1].Trim(); // Assign the layer to decodedMessage[1]
            trimmedMessage = messageParts[0].Trim();                                   // trim off the extra spaces from the message
            messageParts = trimmedMessage.Split("applies a");                          // split messageParts[0] by "applies a". Message = {playerPayload.PlayerName} from {playerPayload.World.Name} && {gagType}
            decodedMessage[2] = messageParts[1].Trim(); // Assign the gagtype to decodedMessage[2]
            trimmedMessage = messageParts[0].Trim();                                   // trim off the extra spaces from the message
            messageParts = trimmedMessage.Split("from");                               // split messageParts[0] by "from". Message = {playerPayload.PlayerName} && {playerPayload.World.Name}
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // Assign messageParts[0] + " " + messageParts[1] to decodedMessage[4]
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: APPLY || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // unique string for /gag lock = "from her pocket and uses it to lock your"
        else if (recievedMessage.Contains(uniqueStringIdentifiers[1])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {locktype} from her pocket and uses it to lock your {layer} gag*
            decodedMessage[0] = "lock";                 // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split("from her pocket and uses it to lock your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: LOCK || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // unique string for /gag lock password == "from her pocket and sets the combination password to"
        else if (recievedMessage.Contains(uniqueStringIdentifiers[2])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {locktype} from her pocket and sets the combination password to {password} before locking your {layer} layer gag*
            decodedMessage[0] = "lockPassword";         // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split("before locking your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" layer gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from her pocket and sets the combination password to");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: LOCK PASSWORD || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // unique string for /gag unlock == "reaches behind your neck, taking off the lock that was keeping your"
        else if (recievedMessage.Contains(uniqueStringIdentifiers[3])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck, taking off the lock that was keeping your {layer} gag layer fastened nice and tight.*
            decodedMessage[0] = "unlock";               // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("gag layer fastened nice and tight.", "");
            string[] messageParts = recievedMessage.Split("reaches behind your neck, taking off the lock that was keeping your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: UNLOCK || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // unique string for /gag unlock password == "reaches behind your neck and sets the password to"
        else if (recievedMessage.Contains(uniqueStringIdentifiers[4])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and sets the password to {password} on your {layer} layer gagstrap, unlocking it.*
            decodedMessage[0] = "unlockPassword";       // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("layer gagstrap, unlocking it.", "");
            string[] messageParts = recievedMessage.Split("on your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("reaches behind your neck and sets the password to");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: UNLOCK PASSWORD || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // unique string for /gag remove == "reaches behind your neck and unfastens the buckle of your"
        else if (recievedMessage.Contains(uniqueStringIdentifiers[5])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unfastens the buckle of your {layer} gag layer strap, allowing your voice to be a little clearer.*
            decodedMessage[0] = "remove";               // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("gag layer strap, allowing your voice to be a little clearer.", "");
            string[] messageParts = recievedMessage.Split("reaches behind your neck and unfastens the buckle of your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: REMOVE || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }
        // final condition means that it is a /gag removeall
        else if (recievedMessage.Contains(uniqueStringIdentifiers[6])) {
            // Template: *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.*
            decodedMessage[0] = "removeall";            // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.", "");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: REMOVEALL || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        } else {
            // should return a list of empty strings, letting us know it isnt any of the filters.
            decodedMessage[0] = "none";
            return decodedMessage;
        }
    }


    /// <summary>
    /// Will take in a message, and determine what to do with it based on the contents of the message.
    /// <list>
    /// <item><c>gag LAYER GAGTYPE | PLAYER</c> - Equip Gagtype to defined layer</item>
    /// <item><c>gag lock LAYER LOCKTYPE | PLAYER</c> - Lock Gagtype to defined layer</item>
    /// <item><c>gag lock LAYER LOCKTYPE | PASSWORD | PLAYER</c> - Lock Gagtype to defined layer with password</item>
    /// <item><c>gag unlock LAYER | PLAYER</c> - Unlock Gagtype from defined layer</item>
    /// <item><c>gag unlock LAYER | PASSWORD | PLAYER</c> - Unlock Gagtype from defined layer with password</item>
    /// <item><c>gag removeall | PLAYER</c> - Remove all gags from player only when parameters are met</item>
    /// <item><c>gag remove LAYER | PLAYER</c> - Remove gag from defined layer</item>
    /// <para><c>recievedMessage</c><param name="receivedMessage"> - The message that was recieved from the player</param></para>
    /// </summary>
    private bool DetermineMessageOutcome(string receivedMessage, List<string> decodedMessage, bool isHandled)
    {
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
        // if the parsed type is "lock" or "lockPassword"
        if (decodedMessage[0] == "lock" || decodedMessage[0] == "lockPassword") {
            // see if our layer is a valid layer
            if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
            if (!int.TryParse(decodedMessage[1], out int layer)) { 
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid layer value.");
                _clientChat.PrintError($"ERROR, Invalid layer value.");
                return true;
            }
            
            // Our layer is valid, but we also need to make sure that we have a gag on this layer
            if (_config.selectedGagTypes[layer-1] == "None") {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
                _clientChat.PrintError($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
                return true;
            }
            // if we do have a gag on this layer, make sure that we dont already have a lock here
            if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug($"ERROR, There is already a lock applied to gag layer {layer}!");
                _clientChat.PrintError($"ERROR, There is already a lock applied to gag layer {layer}!");
                return true;
            }
            // we already made sure that we applied a valid password in the command manager, so no need to check it here.
            if (decodedMessage[3] != "") {
                _config.selectedGagPadlocksPassword[layer-1] = decodedMessage[3]; // we have a password to set, so set it.
            }
            // and because everything above is valid, we can now set the lock type.
            if (Enum.TryParse(decodedMessage[2], out GagPadlocks parsedLockType)) {
                _config.selectedGagPadlocks[layer-1] = parsedLockType;
            } else {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid lock type sent in.");
                _clientChat.PrintError($"ERROR, Invalid lock type sent in.");
                return true;
            }
            // now that we have applied our gagtype, and potentially password, set the assigner to the player if it is a mistress padlock.
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
                _config.selectedGagPadlocksAssigner[layer-1] = decodedMessage[4];
            }
            GagSpeak.Log.Debug($"Determined income message as a [lock] type encoded message, hiding from chat!");
            return true; // sucessful parse
        }
        // if the parsed type is "unlock" or "unlockPassword"
        else if (decodedMessage[0] == "unlock" || decodedMessage[0] == "unlockPassword") {
            // see if our layer is a valid layer
            if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
            if (!int.TryParse(decodedMessage[1], out int layer)) { 
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid layer value.");
                _clientChat.PrintError($"ERROR, Invalid layer value.");
                return true;
            }
            // our layer is valid, but we also need to make sure that this layer has a lock on it
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.None) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
                _clientChat.PrintError($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
                return true;
            }
            // Case where it is just unlock
            if (decodedMessage[3] == "") {
                // Make sure it is not a MistressPadlock
                if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock && _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                    // hide original message & throw exception
                    isHandled = true;
                    GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                    _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                    return true;
                }
                // if we made it here, we can just remove the lock
                _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
                _config.selectedGagPadlocksAssigner[layer-1] = "None";
            } else {
                // if we do have a password, we need to make sure it matches the password on the lock
                if (_config.selectedGagPadlocksPassword[layer-1] != decodedMessage[3]) {
                    // hide original message & throw exception
                    isHandled = true;
                    GagSpeak.Log.Debug("ERROR, Invalid Password, failed to unlock.");
                    _clientChat.PrintError($"ERROR, Invalid Password, failed to unlock.");
                    return true;
                }
                // if the passwords do match, so remove the lock IF it is not a mistress padlock.
                if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock &&
                    _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                    // hide original message & throw exception
                    isHandled = true;
                    GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                    _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                    return true;
                }
                // if we made it here, we can remove the lock.
                _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
                _config.selectedGagPadlocksAssigner[layer-1] = "None";
            }
            GagSpeak.Log.Debug($"Determined income message as a [unlock] type encoded message, hiding from chat!");
            return true; // sucessful parse
        }
        // if the parsed type is "removeall"
        else if (decodedMessage[0] == "removeall") {
            // make sure all of our gagpadlocks are none, if they are not, throw exception
            if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove all gags while locks are on any of them.");
                _clientChat.PrintError($"ERROR, Cannot remove all gags while locks are on any of them.");
                return true;
            }
            // if we made it here, we can remove them all
            for (int i = 0; i < _config.selectedGagPadlocks.Count; i++) {
                _config.selectedGagTypes[i] = "None";
                _config.selectedGagPadlocks[i] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[i] = string.Empty;
                _config.selectedGagPadlocksAssigner[i] = "None";
            }
            GagSpeak.Log.Debug($"Determined income message as a [removeall] type encoded message, hiding from chat!");
            return true; // sucessful parse
        }
        // if the parsed type is "remove"
        else if (decodedMessage[0] == "remove") {
            // see if our layer is a valid layer
            if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
            if (!int.TryParse(decodedMessage[1], out int layer)) { 
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid layer value.");
                _clientChat.PrintError($"ERROR, Invalid layer value.");
                return true;
            }
            // our layer is valid, but we also need to make sure that this layer has a gag on it
            if (_config.selectedGagTypes[layer-1] == "None") {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
                _clientChat.PrintError($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
                return true;
            }
            // make sure there is no lock on that gags layer
            if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove a gag while the lock is on for this layer.");
                _clientChat.PrintError($"ERROR, Cannot remove a gag while the lock is on for this layer.");
                return true;
            }
            // if we made it here, we can remove the gag
            _config.selectedGagTypes[layer-1] = "None";
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "None";
            GagSpeak.Log.Debug($"Determined income message as a [remove] type encoded message, hiding from chat!");
            return true; // sucessful parse
        }
        else if (decodedMessage[0] == "apply") {
            // see if our layer is a valid layer
            if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
            if (!int.TryParse(decodedMessage[1], out int layer)) { 
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid layer value.");
                _clientChat.PrintError($"ERROR, Invalid layer value.");
                return true;
            }
            // see if our gagtype is in selectedGagTypes[layer-1]
            if (!_config.GagTypes.ContainsKey(decodedMessage[2])) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid gag type.");
                _clientChat.PrintError($"ERROR, Invalid gag type.");
                return true;
            }
            // make sure gagType is set to none
            if (_config.selectedGagTypes[layer-1] != "None") {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug($"ERROR, There is already a gag applied for gag layer {layer}!");
                _clientChat.PrintError($"ERROR, There is already a gag applied for gag layer {layer}!");
                return true;
            }
            // if we made it here, we can apply the gag
            _config.selectedGagTypes[layer-1] = decodedMessage[2];
            GagSpeak.Log.Debug($"Determined income message as a [applier] type encoded message, hiding from chat!");
            return true; // sucessful parse
        } else {
            // we have an invalid type
            GagSpeak.Log.Debug($"INVALID MESSAGE TYPE FOR GAGSPEAK, DISPLAYING MESSAGE NORMALLY");
            return false;
        }
    }

    /// <summary>
    /// Will search through the senders friend list to see if they are a friend or not.
    /// <list type="bullet">
    /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your friend list or not</param></item>
    /// </list></summary>
    private bool IsFriend(string nameInput) {
        // Check if it is possible for the client to grab the local player name, if so by default set to true.
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;

        // after, scan through each object in the object table
        foreach (var t in _objectTable) {
            // If the object is a player character (us), we found ourselves, so conmtinue on..
            if (!(t is PlayerCharacter pc)) continue;
            // If the player characters name matches the list of names from local players 
            if (pc.Name.TextValue == nameInput) {
                // See if they have a status of being a friend, if so return true, otherwise return false.
                return pc.StatusFlags.HasFlag(StatusFlags.Friend);
            }
        }
        return false;
    }

    /// <summary>
    /// Will search through the senders party list to see if they are a party member or not.
    /// <list type="bullet">
    /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your party list or not</param></item>
    /// </list></summary>
    private bool IsPartyMember(string nameInput) {
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
        foreach (var t in _objectTable) {
            if (!(t is PlayerCharacter pc)) continue;
            if (pc.Name.TextValue == nameInput)
                return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
        }
        return false;
    }

    /// <summary>
    /// Will search through the senders party list to see if they are a party member or not.
    /// <list type="bullet">
    /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your party list or not</param></item>
    /// </list></summary>
    private bool IsWhitelistedPlayer(string nameInput) {
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
        foreach (var t in _objectTable) {
            if (!(t is PlayerCharacter pc)) continue;
            if (pc.Name.TextValue == nameInput) {
                foreach (var whitelistChar in _config.Whitelist) {
                    // name in the whitelist is a part of the name string
                    GagSpeak.Log.Debug($"Whitelist name: {whitelistChar.name} | NameInput: {nameInput}");
                    if (whitelistChar.name.Contains(nameInput)) {
                        GagSpeak.Log.Debug($"Match Found!");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void SendRealMessage(string message) {
        try {
            _realChatInteraction.SendMessage(message);
        } catch (Exception e) {
            GagSpeak.Log.Warning($"{e},{e.Message}");
            GagSpeak.Log.Debug($"{e},{e.Message}");
        }
    }

    //Framework updater (handle with care)
    private void framework_Update(IFramework framework) {
        if (_config != null && _config.Enabled) {
            try {
                if (messageQueue.Count > 0 && _realChatInteraction != null) {
                    if (!messageTimer.IsRunning) {
                        messageTimer.Start();
                    } else {
                        if (messageTimer.ElapsedMilliseconds > 1000) {
                            try {
                                _realChatInteraction.SendMessage(messageQueue.Dequeue());
                            } catch (Exception e) {
                                GagSpeak.Log.Warning($"{e},{e.Message}");
                            }
                            messageTimer.Restart();
                        }
                    }
                }
            } catch (Exception e) {
                GagSpeak.Log.Warning($"{e},{e.Message}");
            }
        }
    } 
}

#pragma warning restore IDE1006 