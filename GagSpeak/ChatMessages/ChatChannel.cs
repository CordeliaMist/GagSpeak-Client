using FFXIVClientStructs.FFXIV.Client.UI.Agent;         // this is the agent that handles the chatlog
using FFXIVClientStructs.FFXIV.Client.System.Framework; // this is the framework that the game uses to handle all of its UI
using System;                                           // this is used for the enum
using System.Collections.Generic;                       // this is used for the lists
using System.Linq;
using Dalamud.Game.Text;                                      // this is used for the lists

namespace GagSpeak.ChatMessages;

/// <summary> This class is used to handle the chat channels for the GagSpeak plugin. It makes use of chatlog agent pointers, and is fairly complex, so would recommend not using yourself until you know why it points to what it does. </summary>
public static class ChatChannel
{
    // this is the agent that handles the chatlog
    private static unsafe AgentChatLog* ChatlogAgent = (AgentChatLog*)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ChatLog);

    // this is the enum that handles the chat channels
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class EnumOrderAttribute : Attribute {
        public int Order { get; }
        public EnumOrderAttribute(int order) {
            Order = order;
        }
    }

    /// <summary> This enum is used to handle the chat channels. </summary>
    public enum ChatChannels
    {
        [EnumOrder(0)]
        Tell_In = 0,

        [EnumOrder(1)]
        Tell = 17,

        [EnumOrder(2)]
        Say = 1,

        [EnumOrder(3)]
        Party = 2,

        [EnumOrder(4)]
        Alliance = 3,

        [EnumOrder(5)]
        Yell = 4,

        [EnumOrder(6)]
        Shout = 5,

        [EnumOrder(7)]
        FreeCompany = 6,

        [EnumOrder(8)]
        NoviceNetwork = 8,

        [EnumOrder(9)]
        CWL1 = 9,

        [EnumOrder(10)]
        CWL2 = 10,

        [EnumOrder(11)]
        CWL3 = 11,

        [EnumOrder(12)]
        CWL4 = 12,

        [EnumOrder(13)]
        CWL5 = 13,

        [EnumOrder(14)]
        CWL6 = 14,

        [EnumOrder(15)]
        CWL7 = 15,

        [EnumOrder(16)]
        CWL8 = 16,

        [EnumOrder(17)]
        LS1 = 19,

        [EnumOrder(18)]
        LS2 = 20,

        [EnumOrder(19)]
        LS3 = 21,

        [EnumOrder(20)]
        LS4 = 22,

        [EnumOrder(21)]
        LS5 = 23,

        [EnumOrder(22)]
        LS6 = 24,

        [EnumOrder(23)]
        LS7 = 25,

        [EnumOrder(24)]
        LS8 = 26,
    }

    /// <summary> This method is used to get the current chat channel. </summary>
    public static ChatChannels GetChatChannel() {
        // this is the channel that we are going to return
        ChatChannels channel;
        // this is unsafe code, so we need to use unsafe
        unsafe {
            channel = (ChatChannels)ChatlogAgent->CurrentChannel;
        }
        //return the channel now using
        return channel;
    }

    /// <summary> This method is used to get the ordered list of channels. </summary>
    public static IEnumerable<ChatChannels> GetOrderedChannels() {
        return Enum.GetValues(typeof(ChatChannels))
                .Cast<ChatChannels>()
                .Where(e => e != ChatChannels.Tell_In && e != ChatChannels.NoviceNetwork)
                .OrderBy(e => GetOrder(e));
    }

    // Match Channel types with command aliases for them
    public static string[] GetChannelAlias(this ChatChannels channel) => channel switch
    {
        ChatChannels.Tell => new[] { "/t", "/tell"},
        ChatChannels.Say => new[] { "/s", "/say" },
        ChatChannels.Party => new[] { "/p", "/party" },
        ChatChannels.Alliance => new[] { "/a", "/alliance" },
        ChatChannels.Yell => new[] { "/y", "/yell" },
        ChatChannels.Shout => new[] { "/sh", "/shout" },
        ChatChannels.FreeCompany => new[] { "/fc", "/freecompany" },
        ChatChannels.NoviceNetwork => new[] { "/n", "/novice" },
        ChatChannels.CWL1 => new[] { "/cwl1", "/cwlinkshell1" },
        ChatChannels.CWL2 => new[] { "/cwl2", "/cwlinkshell2" },
        ChatChannels.CWL3 => new[] { "/cwl3", "/cwlinkshell3" },
        ChatChannels.CWL4 => new[] { "/cwl4", "/cwlinkshell4" },
        ChatChannels.CWL5 => new[] { "/cwl5", "/cwlinkshell5" },
        ChatChannels.CWL6 => new[] { "/cwl6", "/cwlinkshell6" },
        ChatChannels.CWL7 => new[] { "/cwl7", "/cwlinkshell7" },
        ChatChannels.CWL8 => new[] { "/cwl8", "/cwlinkshell8" },
        ChatChannels.LS1 => new[] { "/l1", "/linkshell1" },
        ChatChannels.LS2 => new[] { "/l2", "/linkshell2" },
        ChatChannels.LS3 => new[] { "/l3", "/linkshell3" },
        ChatChannels.LS4 => new[] { "/l4", "/linkshell4" },
        ChatChannels.LS5 => new[] { "/l5", "/linkshell5" },
        ChatChannels.LS6 => new[] { "/l6", "/linkshell6" },
        ChatChannels.LS7 => new[] { "/l7", "/linkshell7" },
        ChatChannels.LS8 => new[] { "/l8", "/linkshell8" },
        _ => Array.Empty<string>(),
    };

    // Get a commands list for given channelList(config) and add extra space for matching to avoid matching emotes.
    public static List<string> GetChatChannelsListAliases(this IEnumerable<ChatChannels> chatChannelsList)
    {
        var result = new List<string>();
        foreach (ChatChannels chatChannel in chatChannelsList)
        {
            result.AddRange(chatChannel.GetChannelAlias().Select(str => str + " "));
        }
        return result;
    }

    // see if the passed in alias is present as an alias in any of our existing channels
    public static bool IsAliasForAnyActiveChannel(this IEnumerable<ChatChannels> enabledChannels, string alias)
    {
        return enabledChannels.Any(channel => channel.GetChannelAlias().Contains(alias));
    }

    // get the chat channel type from the XIVChatType
    public static ChatChannels? GetChatChannelFromXivChatType(XivChatType type) {
        return type switch
        {
            XivChatType.TellIncoming    => ChatChannels.Tell,
            XivChatType.TellOutgoing    => ChatChannels.Tell,
            XivChatType.Say             => ChatChannels.Say,
            XivChatType.Party           => ChatChannels.Party,
            XivChatType.Alliance        => ChatChannels.Alliance,
            XivChatType.Yell            => ChatChannels.Yell,
            XivChatType.Shout           => ChatChannels.Shout,
            XivChatType.FreeCompany     => ChatChannels.FreeCompany,
            XivChatType.NoviceNetwork   => ChatChannels.NoviceNetwork,
            XivChatType.Ls1             => ChatChannels.LS1,
            XivChatType.Ls2             => ChatChannels.LS2,
            XivChatType.Ls3             => ChatChannels.LS3,
            XivChatType.Ls4             => ChatChannels.LS4,
            XivChatType.Ls5             => ChatChannels.LS5,
            XivChatType.Ls6             => ChatChannels.LS6,
            XivChatType.Ls7             => ChatChannels.LS7,
            XivChatType.Ls8             => ChatChannels.LS8,
            XivChatType.CrossLinkShell1 => ChatChannels.CWL1,
            XivChatType.CrossLinkShell2 => ChatChannels.CWL2,
            XivChatType.CrossLinkShell3 => ChatChannels.CWL3,
            XivChatType.CrossLinkShell4 => ChatChannels.CWL4,
            XivChatType.CrossLinkShell5 => ChatChannels.CWL5,
            XivChatType.CrossLinkShell6 => ChatChannels.CWL6,
            XivChatType.CrossLinkShell7 => ChatChannels.CWL7,
            XivChatType.CrossLinkShell8 => ChatChannels.CWL8,
            _ => null
        };
    }

    /// <summary> This method is used to get the order of the enum, which is then given to getOrderedChannels. </summary>
    private static int GetOrder(ChatChannels channel) {
        // get the attribute of the channel
        var attribute = channel.GetType()
            .GetField(channel.ToString())
            ?.GetCustomAttributes(typeof(EnumOrderAttribute), false)
            .FirstOrDefault() as EnumOrderAttribute;
        // return the order of the channel, or if it doesnt have one, return the max value
        return attribute?.Order ?? int.MaxValue;
    }
}
