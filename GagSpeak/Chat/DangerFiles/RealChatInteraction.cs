using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using GagSpeak.Services;
using GagSpeak.Chat;
// practicing modular design
namespace XivCommon.Functions;
    /// <summary>
    /// This class, in essence, is modifying a packet BEFORE it gets sent to the server,
    /// allowing us to send a message to the chat itself, and not just to dalamud chat.
    /// This is NOT sending chat messages to the client, and as such should be used VERY CAREFULLY
    /// <para><b>If you plan on ever using this code anywhere else, make damn sure you know how it is working. </b></para>
    /// </summary>
    public class RealChatInteraction {
        private static class Signatures { // First we will create the Signatures for sending a message to the chatbox
            internal const string SendChat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";
        }

        // Next we need to process the chatbox delgate, meaning we need to get the pointer for the uimodule, message, unused information and byte data
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);

        // Now we need to get the pointer for the uimodule, message, unused information and byte data from above
        private ProcessChatBoxDelegate? ProcessChatBox { get; }

        /// <summary>
        /// By being an internal constructor, it means that this class can only be accessed by the same assembly.
        /// </summary>
        internal RealChatInteraction(ISigScanner scanner) {
            // Now we need to scan for the signature of the chatbox, to see if it is valid
            if (scanner.TryScanText(Signatures.SendChat, out var processChatBoxPtr)) {
                // If it is valid, we need to get the delegate for the chatbox as a function pointer.
                this.ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(processChatBoxPtr);
            }
        }

        /// <summary>
        /// <para>Send a given message to the chat box. <b>This can send chat to the server.</b></para>
        /// <para>
        /// This method will throw exceptions when a chat message is longer than it should be or when it is empty,
        /// and it will also filter out any symbols that normally should not be sent. Somewhat "Sanatizing" the message.
        /// However, it can still make mistakes, so use with caution.
        /// </para>
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentException">If <paramref name="message"/> is empty or longer than 500 bytes in UTF-8.</exception>
        /// <exception cref="InvalidOperationException">If the signature for this function could not be found</exception>
        public void SendMessage(string message) {
            // May be safe to sanatize here before we incode the message to UTF8

            // Get the number of bytes our message contains.
            var bytes = Encoding.UTF8.GetBytes(message);

            // First, let us be sure that our message is not empty
            if (bytes.Length == 0) { // If it is, don't send the message and throw a message empty exception instead.
                throw new ArgumentException("message is empty", nameof(message));
            }

            // Next, let us be sure that our message is not larger than the ammount of bytes a message should be allowed to send
            if (bytes.Length > 500) { // If it is, dont send the message and throw a message too long exception instead.
                throw new ArgumentException($"message is longer than 500 bytes, and is {bytes.Length}", nameof(message));
            }

            // Finally, we want to be sure that our processchatbox sucessfully got the delegate for our function pointer.
            if (this.ProcessChatBox == null) {
                throw new InvalidOperationException("Could not find signature for chat sending");
            }

            // Finally, we will need to make sure that our message is not sending any symbols that should not be sent, so we should sanatize them out.

            // Assuming it meets the correct conditions, we can begin to obtain the UI module pointer for the chatbox within the framework instance
            this.SendMessageUnsafe(bytes);
        }

        /// <summary>
        /// <para>Send a given message to the chat box. <b>This can send chat to the server.</b></para>
        /// <para>
        /// This method does not throw any exceptions, and should be handled with fucking caution,
        /// it is primarily used to initialize the actual sending of chat to the server, hince the
        /// unsafe method, and can not be merged with the sendMessage function.
        /// </para>
        /// </summary>
        public unsafe void SendMessageUnsafe(byte[] message) {
            // To be extra safe, double check our processchatbox has its delegate correctly.
            if (this.ProcessChatBox == null) {
                throw new InvalidOperationException("Could not find signature for chat sending");
            }
            // Assuming it meets the correct conditions, we can begin to obtain the UI module pointer for the chatbox within the framework instance
            var uiModule = (IntPtr) Framework.Instance()->GetUiModule();
                
            // create a payload for our chat message
            using var payload = new ChatPayload(message);
            /// MARSHAL -provides a collection of methods for allocating unmanaged memory, copying unmanaged memory blocks, 
            ///   and converting managed to unmanaged types & miscellaneous methods used when interacting with unmanaged code.
            /// AllocHGlobal - Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
            /// Returns: A pointer to the newly allocated memory. This memory must be released using the Marshal.FreeHGlobal(nint) method.
            // Marshal the payload to a pointer in memory
            var mem1 = Marshal.AllocHGlobal(400);
            // StructureToPtr - Marshals data from a managed object to an unmanaged block of memory.
            Marshal.StructureToPtr(payload, mem1, false);
            // Finally, we can send our message to the chatbox
            this.ProcessChatBox(uiModule, mem1, IntPtr.Zero, 0);
            // and dont forget to free back up our memory
            Marshal.FreeHGlobal(mem1);
        }

        [StructLayout(LayoutKind.Explicit)] // Lets us control the physical layout of the data fields of a class or structure in memory.
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")] // We need to keep the pointer alive

        // the chatpayload struct format, includes the text pointer, text length, and two unknowns, we must set these to the appropriate field offsets to ensure the correct data is sent
        private readonly struct ChatPayload : IDisposable {
            [FieldOffset(0)]
            private readonly IntPtr textPtr;

            [FieldOffset(16)]
            private readonly ulong textLen;

            [FieldOffset(8)]
            private readonly ulong unk1;

            [FieldOffset(24)]
            private readonly ulong unk2;

            // The constructor for the chatpayload struct, we need to allocate memory for the string bytes, and then copy the string bytes to the text pointer
            internal ChatPayload(byte[] stringBytes) {
                // AllocHGlobal - Allocates memory from the unmanaged memory of the process by using the specified number of bytes.
                this.textPtr = Marshal.AllocHGlobal(stringBytes.Length + 30);
                // Copy - Copies data from a managed array to an unmanaged memory pointer, or from an unmanaged memory pointer to a managed array.
                Marshal.Copy(stringBytes, 0, this.textPtr, stringBytes.Length);
                // WriteByte - Writes a single byte value to unmanaged memory.
                Marshal.WriteByte(this.textPtr + stringBytes.Length, 0);

                // Set the text length to the length of the string bytes + 1
                this.textLen = (ulong) (stringBytes.Length + 1);
                // Set the unknowns to 64 and 0, as they should be for chat message sending.
                this.unk1 = 64;
                this.unk2 = 0;
            }

            // when we dispose of our chat payload, we must be sure to free the memory we allowed in this.textPtr
            public void Dispose() {
                Marshal.FreeHGlobal(this.textPtr);
            }
        }
    }