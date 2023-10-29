
// Include nessisary libraries here.


namespace GagSpeak.Chat;

// Contains the Translation records. Used by the history services & history window to store and display translations
public class TranslateMessage
{
    // Set input and output variables
    public string Input { get; set; }
    public string Output { get; set; }

    // Initializes a new instance of "Translation" class.
    public TranslateMessage(string input, string output)
    {
        Input = input;
        Output = output;
    }
}