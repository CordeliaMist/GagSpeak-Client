namespace GagSpeak
{
/// <summary>
/// Translation records.
/// </summary>
public class Translation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Translation"/> class.
    /// </summary>
    /// <param name="input">Input text to be translated.</param>
    /// <param name="output">Output text from translation.</param>
    public Translation(string input, string output)
    {
        this.Input = input;
        this.Output = output;
    }

    /// <summary>
    /// Gets or sets input text to be translated.
    /// </summary>
    public string Input { get; set; }

    /// <summary>
    /// Gets or sets output text from translation.
    /// </summary>
    public string Output { get; set; }
}
}