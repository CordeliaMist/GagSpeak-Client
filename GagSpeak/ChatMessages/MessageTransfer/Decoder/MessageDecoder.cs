using System.Collections.Generic;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {

    public void DecodeMsgToList(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // if the type is gagspeak message then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.GagSpeak) {
            DecodeGagSpeakMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is relation then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.Relationship) {
            DecodeRelationshipMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is wardrobe then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.Wardrobe) {
            DecodeWardrobeMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is puppeteer then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.Puppeteer) {
            DecodePuppeteerMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is toybox then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.Toybox) {
            DecodeToyboxMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is info exchange then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.InfoExchange) {
            DecodeInfoExchangeMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // if the type is hardcore then process it here
        if(decodedMessageMediator.msgType == DecodedMessageType.Hardcore) {
            DecodeHardcoreMsg(recievedMessage, decodedMessageMediator);
            return;
        }

        // we should never reach here, but if we do, then return the empty list
        return;
    }
}
