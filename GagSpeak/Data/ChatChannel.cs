using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GagSpeak.Data;

public static class ChatChannel
{
    private static unsafe AgentChatLog* ChatlogAgent = (AgentChatLog*)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ChatLog);

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class EnumOrderAttribute : Attribute {
        public int Order { get; }
        public EnumOrderAttribute(int order) {
            Order = order;
        }
    }

    public enum ChatChannels
    {
           [EnumOrder(0)]
            Tell_In = 0,

            [EnumOrder(1)]
            Tell_Out = 17,

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

    public static ChatChannels GetChatChannel()
    {
        ChatChannels channel;
        unsafe
        {
            channel = (ChatChannels)ChatlogAgent->CurrentChannel;
        }
        return channel; //return the channel now using
    }

    public static IEnumerable<ChatChannels> GetOrderedChannels()
    {
        return Enum.GetValues(typeof(ChatChannels))
            .Cast<ChatChannels>()
            .OrderBy(e => GetOrder(e));
    }

    private static int GetOrder(ChatChannels channel)
    {
        var attribute = channel.GetType()
            .GetField(channel.ToString())
            .GetCustomAttributes(typeof(EnumOrderAttribute), false)
            .FirstOrDefault() as EnumOrderAttribute;

        return attribute?.Order ?? int.MaxValue;
    }

}

    




