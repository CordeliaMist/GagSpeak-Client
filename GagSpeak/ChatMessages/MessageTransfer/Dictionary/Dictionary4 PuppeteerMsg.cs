namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupPuppeteerMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {       
        // The toggle allowing sit requests [ ID == 27 ]
        if(textVal.Contains("approached their submissive, \"Say now my love, how would you like to grant me access "+
        "to control where you can and cant sit down?\""))
        {
            decodedMessageMediator.encodedMsgIndex = 27;
            decodedMessageMediator.msgType = DecodedMessageType.Puppeteer;
            return true;
        }

        // The toggle allowing motion requests [ ID == 28 ]
        if(textVal.Contains("approached their submissive, \"Say now my love, how would you like to submit yourself "+
        "to move about and dance for me whenever I say the word?\""))
        {
            decodedMessageMediator.encodedMsgIndex = 28;
            decodedMessageMediator.msgType = DecodedMessageType.Puppeteer;
            return true;
        }

        // The toggle allowing all commands [ ID == 29 ]
        if(textVal.Contains("approached their submissive, \"We both know you've submitted yourself to me fully, so "+
        "why not accept that you'll do whatever I say without a second thought?\""))
        {
            decodedMessageMediator.encodedMsgIndex = 29;
            decodedMessageMediator.msgType = DecodedMessageType.Puppeteer;
            return true;
        }

        // return false if none are present
        return false;
    }
}
