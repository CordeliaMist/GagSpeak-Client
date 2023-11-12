using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace GagSpeak.Data;

public static class ChatChannel
{
    private static unsafe AgentChatLog* ChatlogAgent = (AgentChatLog*)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ChatLog);

    public enum ChatChannels
    {
        //Need Confirm (still missing a lot)
        Tell = 0,
        Say = 1,
        Party = 2,
        Yell = 4,
        Shout = 5,
        FreeCompany = 6,
        Alliance = 3,
        NoviceNetwork = 8,

        CWL1 = 9,
        CWL2 = 10,
        CWL3 = 11,
        CWL4 = 12,
        CWL5 = 13,
        CWL6 = 14,
        CWL7 = 15,
        CWL8 = 16,

        LS1 = 19,
        LS2 = 20,
        LS3 = 21,
        LS4 = 22,
        LS5 = 23,
        LS6 = 24,
        LS7 = 25,
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

}

    




