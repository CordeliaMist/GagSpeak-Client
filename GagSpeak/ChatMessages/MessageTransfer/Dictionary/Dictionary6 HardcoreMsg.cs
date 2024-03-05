namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupHardcoreMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {
        // The toggle the players blindfold permission [ ID == 43 ]
        if(textVal.Contains("wrapped the lace blindfold nicely around your head, blocking out almost all light from your eyes, yet still allowing just enough through to keep things exciting"))
        {
            decodedMessageMediator.encodedMsgIndex = 43;
            decodedMessageMediator.msgType = DecodedMessageType.Hardcore;
            return true;
        }

        // return false if none are present
        return false;
    } 
}