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
using GagSpeak.Chat.MsgDictionary;
using GagSpeak.Chat.MsgDecoder;
using GagSpeak.Chat.MsgResultLogic;

namespace GagSpeak.Chat;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class ChatManager
{
    private readonly IChatGui _clientChat;
    private readonly GagSpeakConfig _config;
    private readonly IClientState _clientState;
    private readonly IObjectTable _objectTable;
    private readonly RealChatInteraction _realChatInteraction;
    private readonly MessageDecoder _messageDecoder;
    private readonly MessageResultLogic _msgResultLogic;
    private readonly IFramework _framework; // framework from XIVClientStructs
    private Queue<string> messageQueue = new Queue<string>();
    private Stopwatch messageTimer = new Stopwatch();

    // future future note: the chat handling came from simpletweaks i found out after enough digging, and they have some other fancy inturruptions,
    // that could possibly make you not need to use /gsm at all.

    public ChatManager(IChatGui clientChat, GagSpeakConfig config, IClientState clientState, IObjectTable objectTable,
                RealChatInteraction realChatInteraction, MessageDecoder messageDecoder, MessageResultLogic messageResultLogic ,IFramework framework) {
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _objectTable = objectTable;
        _realChatInteraction = realChatInteraction;
        _messageDecoder = messageDecoder;
        _msgResultLogic = messageResultLogic;
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

    // FOR NOW EVERYTHING WILL BE STUFFED INTO HERE, AND LATER DIVIDED OUT INTO THE OTHER CHATS
    private void Chat_OnCheckMessageHandled(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool isHandled) {
        if ( type == XivChatType.TellOutgoing) { // handles the hiding of outgoing tells if they are encoded
            // Scan if the message contains all words from the an ncoded tell message
            if(MessageDictionary.EncodedMsgDictionary(message.TextValue)) {
                isHandled = true;
                _config.Save();
                return;
            }
        }
    }

    //// CHATGUI FUNCTIONS: ////
    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
        var fmessage = new SeString(new List<Payload>());
        var nline = new SeString(new List<Payload>());
        nline.Payloads.Add(new TextPayload("\n"));
        PlayerPayload playerPayload; // make payload for the player
        List<char> toRemove = new() { //removes special characters in party listings [https://na.finalfantasyxiv.com/lodestone/character/10080203/blog/2891974/]
            '','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',
        };
        var sanitized = sender.ToString(); // convert the sender from SeString to String

        foreach(var c in toRemove) { sanitized = sanitized.Replace(c.ToString(), string.Empty); } // remove all special characters

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
        var senderName = sName?.PlayerName != null ? sName.PlayerName : pName;

        if (type == XivChatType.TellIncoming) 
        {
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
            int encodedMsgIndex = 0; // get a index to know which encoded msg it is, if any
            if (MessageDictionary.EncodedMsgDictionary(chatmessage.TextValue, ref encodedMsgIndex)) {
                // if in dom mode, back out, none of this will have any significance
                if (_config.InDomMode && encodedMsgIndex > 0 && encodedMsgIndex <= 7) {
                    GagSpeak.Log.Debug("Player attempted to gag you, but you are in Dominant mode, so ignoring");
                    isHandled = true;
                    return;
                }
                // if our encodedmsgindex is > 1 && < 6, we have a encoded message via command
                else if (encodedMsgIndex > 0 && encodedMsgIndex <= 7) {
                    List<string> decodedCommandMsg = _messageDecoder.DecodeMsgToList(fmessage.ToString(), encodedMsgIndex);
                    // function that will determine what happens to the player as a result of the tell.
                    if(_msgResultLogic.CommandMsgResLogic(fmessage.ToString(), decodedCommandMsg, isHandled, _clientChat, _config) ) {
                        isHandled = true; // logic sucessfully parsed, so hide from chat
                    }
                    _config.Save(); // save our config
                
                // for now at least, anything beyond 7 is a whitelist exchange message
                } else if (encodedMsgIndex > 7) {
                    GagSpeak.Log.Debug("This is a whitelist exchange message!");
                    List<string> decodedWhitelistMsg = _messageDecoder.DecodeMsgToList(fmessage.ToString(), encodedMsgIndex);
                    // function that will determine what happens to the player as a result of the tell.
                    if(_msgResultLogic.WhitelistMsgResLogic(fmessage.ToString(), decodedWhitelistMsg, isHandled, _clientChat, _config) ) {
                        isHandled = true; // logic sucessfully parsed, so hide from chat
                    }
                    
                    isHandled = true;
                    return;
                }
            } // skipped to this point if not encoded message
        } // skips directly to here if not a tell
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

    /// <summary>
    /// will send a real message to the chat, server side, USE WITH CAUTION
    /// <list type="bullet">
    /// <item><c>message</c><param name="message"> - The message to be sent to the server</param></item>
    /// </list></summary>
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