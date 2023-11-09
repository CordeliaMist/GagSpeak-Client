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
        Alliance = 3,
        Yell = 4,
        Shout = 5,
        FreeCompany = 6,
        //NoviceNetwork = 


        CWL1 = 9,
        CWL2 = 10,
        CWL3 = 11,
        CWL4 = 12,
        CWL5 = 13,
        CWL6 = 14,
        CWL7 = 15,
        CWL8 = 16,

        Ls1 = 19,
        Ls2 = 20,
        Ls3 = 21,
        Ls4 = 22,
        Ls5 = 23,
        Ls6 = 24,
        Ls7 = 25,
        Ls8 = 26,
    }

    public static ChatChannels GetChatChannel()
    {
        ChatChannels channel;
        unsafe
        {
            channel = (ChatChannels)ChatlogAgent->CurrentChannel;
        }
        return channel;
    }

}

    




