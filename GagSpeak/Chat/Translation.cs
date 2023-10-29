

// Very likely may go once I find a way to functionalize this further

namespace GagSpeak
{
    // Contains the Translation records. Used by the history services & history window to store and display translations
    public class Translation
    {
        // Set input and output variables
        public string Input { get; set; }
        public string Output { get; set; }


        // Initializes a new instance of "Translation" class.
        public Translation(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }
}