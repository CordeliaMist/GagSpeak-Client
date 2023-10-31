using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ImGuiNET;
using OtterGui;
using OtterGui.Classes;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Globalization;
using Dalamud.Logging;
using Num = System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using ImPlotNET;
using OtterGui.Log;
using GagSpeak.Services;

using XivCommon;
using XivCommon.Functions;
using Dalamud.Game;

using System.Diagnostics;


// This serves as the hub for both:
// - OnChatMessage reading
// - Sending Garbled Messages
// - Sended tells to whitlisted players


namespace GagSpeak.Chat;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class ChatManager
{
    private readonly IChatGui _clientChat;
    private readonly GagSpeakConfig _config;
    private readonly IClientState _clientState;
    private readonly IObjectTable _objectTable;
    private readonly CommandManager _commandManager;
    private readonly RealChatInteraction _realChatInteraction;
    private readonly ISigScanner _sigScanner;
    private readonly IFramework _framework; // framework from XIVClientStructs
    private Queue<string> messageQueue = new Queue<string>();
    private Stopwatch messageTimer = new Stopwatch();
    

    public ChatManager(IChatGui clientChat, GagSpeakConfig config, IClientState clientState, IObjectTable objectTable,
    CommandManager commandManager, RealChatInteraction realChatInteraction, ISigScanner sigScanner, IFramework framework) {
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _objectTable = objectTable;
        _commandManager = commandManager;
        _realChatInteraction = realChatInteraction;
        _sigScanner = sigScanner;
        _framework = framework;
        
        // begin our realchatinteraction sigscanner
        _realChatInteraction = new RealChatInteraction(_sigScanner);

        // begin our framework check
        _framework.Update += framework_Update;
        // Begin our OnChatMessage Detection
        _clientChat.ChatMessage += Chat_OnChatMessage;
    }

    // FOR NOW EVERYTHING WILL BE STUFFED INTO HERE, AND LATER DIVIDED OUT INTO THE OTHER CHATS

    //// CHATGUI FUNCTIONS: ////
    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
        // FILTER CONDITION ONE:
        //  - If isHandled is true, we want to immidiately back out of the function. and abort, not nessisary to read the rest.
        //    Doing this at any point one our filters is not true is preferred to save on resources and runtime.
        if (isHandled) return;

        // FILTER CONDITION TWO:
        //  - Is the chat message an incoming tell? If yes, proceed into the inner function, if not read over
        //    literally everything else that happens inside of it lol.
        if (type == XivChatType.TellIncoming) 
        {
            // Still unsure about the spesifics of this, comment fully later
            var fmessage = new SeString(new List<Payload>());
            var nline = new SeString(new List<Payload>());
            nline.Payloads.Add(new TextPayload("\n"));

            PlayerPayload playerPayload; // make payload for the player

            List<char> toRemove = new() { //removes special characters in party listings [https://na.finalfantasyxiv.com/lodestone/character/10080203/blog/2891974/]
              '','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',
            };
            var sanitized = sender.ToString(); // convert the sender from SeString to String

            foreach(var c in toRemove) {
                sanitized = sanitized.Replace(c.ToString(), string.Empty); // remove all special characters
            }

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
            

            // Now that we have stripped the player name and sender name from the payload, let's filter if we need to worry about parsing at all.

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

            // see if the fmessage.toString() contains "gag" as the first word in the string, if not, return.
            if (!fmessage.ToString().ToLower().StartsWith("gag")) {
                GagSpeak.Log.Debug("Tell does not start with the word 'gag'!");
                return;
            }
            DetermineMessageOutcome(fmessage.ToString()); // function that will determine what happens to the player as a result of the tell.

            // Hide that tell recieved payload from the chat box, so it is not seen by the user.
            _config.Save(); // save our config

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
    private void DetermineMessageOutcome(string receivedMessage)
    {
        // Make our arguements split between everything before, and after the ' | ' in our message.
        string[] arguments = receivedMessage.Split('|');

        // Make our commandParts become an array of strings that is everything before the first | in the message.
        string[] commandParts = arguments[0].Trim().Split(' ');

        // set "command" = to the first word that comes after gag.
        string command = commandParts[1].Trim().ToLower();

        // set our identifiers we will need to assign while parsing the rest of the strings.
        string lockType = string.Empty;
        string password = string.Empty;
        string player = string.Empty;
        string gagtype = string.Empty;
        // if the word is lock, unlock, or removeall
        if (command == "lock") {
            if (commandParts.Length > 2) {
                string layerString = commandParts[2].Trim(); // Keeping layer as string outside the block
                if (!int.TryParse(layerString, out int layer)) {
                    // Handle if layerString is not a valid int
                    throw new Exception("Invalid layer value.");
                }

                if (_config.selectedGagPadlocks[layer] != GagPadlocks.None) {
                    throw new Exception("Gag is already applied for this layer.");
                }

                if (arguments.Length >= 2) {
                    string[] actionParts = arguments[1].Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
                    if (actionParts.Length >= 1) {
                        string lockTypeString = actionParts[0].Trim();

                        if (Enum.TryParse(lockTypeString, out GagPadlocks parsedLockType)) {
                            _config.selectedGagPadlocks[layer] = parsedLockType;
                        } else {
                            throw new Exception("Invalid lock type.");
                        }

                        // Continue with your logic for password and player
                        password = actionParts.Length >= 2 ? actionParts[1].Trim() : string.Empty;
                        player = actionParts.Length >= 3 ? actionParts[2].Trim() : string.Empty;

                        _config.selectedGagPadlocksAssigner[layer] = player;
                    }
                }
            }
        }
        else if (command == "unlock") {
            // it's /gag unlock [layer] | [From Player] message? (shouldn't need password, but maybe)
            // it's /gag unlock [layer] [password] | [From Player] message?
            // Extract the 'layer' information from the command parts
            string layerString = commandParts[2].Trim(); // Keeping layer as string outside the block
            if (!int.TryParse(layerString, out int layer)) {
                // Handle if layerString is not a valid int
                throw new Exception("Invalid layer value.");
            }
            // Extract the 'password' and 'player' information from the arguments
            if (arguments.Length >= 2) {
                string[] actionParts = arguments[1].Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries);

                // Check and assign the 'password' and 'player'
                if (actionParts.Length >= 1) {
                    if (actionParts[0].Contains('|')) {
                        password = actionParts[0].Trim();
                    } else {
                        player = actionParts[0].Trim();
                    }
                    // password required, check for match, and type of padlock (add this functionality later)
                    if (_config.selectedGagPadlocks[layer] != GagPadlocks.None) {
                        if (_config.selectedGagPadlocksPassword[layer] != password){
                            // if the passwords dont match, throw exception
                            throw new Exception("Invalid Password.");
                        }
                        // the passwords do match, so remove the gag.
                        // here we would add functionality for the mistress padlock
                    }
                }
                if (actionParts.Length >= 2) {
                    player = actionParts[1].Trim();
                    // no password required, remove the gag.
                    _config.selectedGagPadlocks[layer] = GagPadlocks.None;
                    _config.selectedGagPadlocksAssigner[layer] = string.Empty;
                }
            }
        }
        else if (command == "removeall") {
            // it's /gag removeall | [From Player] message? (only works if no locks on)
            // Extract the 'player' information from the arguments
            if (arguments.Length >= 2) { player = arguments[1].Trim(); }

            // make sure no locks are on, if they are, throw exception
            if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
                throw new Exception("Cannot remove all gags while locks are on.");
            }

            // Otherwise, remove them
            for (int i = 0; i < _config.selectedGagPadlocks.Count; i++) {
                _config.selectedGagTypes[i] = "None";
                _config.selectedGagPadlocks[i] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[i] = string.Empty;
                _config.selectedGagPadlocksAssigner[i] = "None";
            }

        }
        else if (command == "remove") {
            string layerString = commandParts[2].Trim(); // Keeping layer as string outside the block
            if (!int.TryParse(layerString, out int layer)) {
                // Handle if layerString is not a valid int
                throw new Exception("Invalid layer value.");
            }

            // make player equal arguments[1].trim
            if (arguments.Length >= 2) { player = arguments[1].Trim(); }
            
            if (_config.selectedGagPadlocks[layer] != GagPadlocks.None) {
                throw new Exception("Cannot remove a gag while the lock is on for this layer.");
            }

            _config.selectedGagTypes[layer] = "None";
            _config.selectedGagPadlocks[layer] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer] = "None";
        }
        else {
            // it's /gag [layer] [gagtype] | [From Player] message?
            // Extract the 'layer' information from the command parts
            string layerString = commandParts[1].Trim(); // Keeping layer as string outside the block
            GagSpeak.Log.Debug($"{layerString}");
            if (!int.TryParse(layerString, out int layer)) {
                // Handle if layerString is not a valid int
                throw new Exception("Invalid layer value.");
            }
            // extract the gag type, which should just be the rest of arguements[0] after the layer,
            // aka, commandParts[3] and beyond.
            string[] restCommandParts = commandParts[2..]; // Get all parts after layer
            gagtype = string.Join(" ", restCommandParts);

            // After this, arguements[1] should just contain the player, aka everything after the | we split from earlier.
            player = arguments[1].Trim();

            // see if our gagtype is in selectedGagTypes[layer]
            if (!_config.GagTypes.ContainsKey(gagtype)) {
                // if it is not, throw an exception
                throw new Exception("Invalid gag type.");
            }
            // Now, just set that gag type to the layer, and set the assigned player to the player.
            _config.selectedGagTypes[layer-1] = gagtype;

            GagSpeak.Log.Debug($"{gagtype} | {player}");
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
            if (pc.Name.TextValue == nameInput)
                // here is where we need to compare the nameinput to the whitelist to see if any match
                // current line below is not the proper way to do this, it was cloned from isPartyMember.
                return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
        }
        return false;
    }
    
    // If /gs [message] is sent, first translate [message], then send message to appropriate chat type (currently selected chat type in chat box)

    // if /gag [layer] [gagtype] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions

    // if /gag lock [layer] [locktype] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions

    // if /gag lock [layer] [locktype] | [password] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions
    
    // if /gag unlock [layer] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions
    
    // if /gag unlock [layer] | [password] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions

    // if /gag unlock [layer] | [player target] message is sent, construct the proper formatted tell to send to the player,
    // and then hide the tell via chatGUI functions

    // If updateplayerstatus is pressed on whitelist, trigger a tell request to the player to send them back their current status information,
    // then update the whitelist with the new information.


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