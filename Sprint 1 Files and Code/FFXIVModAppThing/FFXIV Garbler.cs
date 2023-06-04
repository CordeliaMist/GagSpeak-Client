using System;

namespace FFXIVModAppThing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void garbleButton_Click(object sender, EventArgs e)
        {
            if(inputTextBox.Text != "" && levelTextBox.Text != "") 
            {
                string inputString = inputTextBox.Text;
                int levelInt;
                bool parseSuccess = int.TryParse(levelTextBox.Text, out levelInt);
                if (levelInt > 0 && levelInt <= 20 && parseSuccess)
                {
                    outputTextBox.Text = garbleFunction(inputString, levelInt);
                    if (clipboardCheckbox.Checked)
                    {
                        Clipboard.SetText(outputTextBox.Text);
                    }
                }
                else
                {
                    outputTextBox.Text = "Error: Level out of bounds (1~20) or not a number.";
                }

            }
            else
            {
                outputTextBox.Text = "Error: Input box or Level box probably empty.";
            }
            

        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            if(outputTextBox.Text != "")
            {
                Clipboard.SetText(outputTextBox.Text);
            }
        }

        private static string garbleFunction(string input, int level)
        {
            string beginString = input;
            string endString = null;
            beginString = beginString.ToLower();
            for (int ind = 0; ind < beginString.Length; ind++)
            {
                char currentChar = beginString[ind];
                if (level >= 20)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("zqjxkvbywgpfucdlhr".Contains(currentChar)) { endString += " "; }
                    else { endString += "m"; }
                }
                else if (level >= 16)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("zqjxkvbywgpf".Contains(currentChar)) { endString += " "; }
                    else { endString += "m"; }
                }
                else if (level >= 12)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("zqjxkv".Contains(currentChar)) { endString += " "; }
                    else { endString += "m"; }
                }
                else if (level >= 8)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else { endString += "m"; }
                }
                else if (level >= 7)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if (currentChar == 'b') { endString += currentChar; }
                    else if ("aeiouy".Contains(currentChar)) { endString += "e"; }
                    else if ("jklr".Contains(currentChar)) { endString += "a"; }
                    else if ("szh".Contains(currentChar)) { endString += "h"; }
                    else if ("dfgnmwtcqxpv".Contains(currentChar)) { endString += "m"; }
                    else { endString += currentChar; }
                }
                else if (level >= 6)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("aeiouyt".Contains(currentChar)) { endString += "e"; }
                    else if ("jklrw".Contains(currentChar)) { endString += "a"; }
                    else if ("szh".Contains(currentChar)) { endString += "h"; }
                    else if ("dfgnm".Contains(currentChar)) { endString += "m"; }
                    else if ("cqx".Contains(currentChar)) { endString += "k"; }
                    else if ("bpv".Contains(currentChar)) { endString += "f"; }
                    else { endString += currentChar; }
                }
                else if (level >= 5)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("eiouyt".Contains(currentChar)) { endString += "e"; }
                    else if ("jlrwa".Contains(currentChar)) { endString += "a"; }
                    else if ("szh".Contains(currentChar)) { endString += "h"; }
                    else if ("dfgm".Contains(currentChar)) { endString += "m"; }
                    else if ("cqxk".Contains(currentChar)) { endString += "k"; }
                    else if ("bpv".Contains(currentChar)) { endString += "f"; }
                    else { endString += currentChar; }
                }
                else if (level >= 4)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("vbct".Contains(currentChar)) { endString += "e"; }
                    else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                    else if ("sz".Contains(currentChar)) { endString += "h"; }
                    else if ("df".Contains(currentChar)) { endString += "m"; }
                    else if ("qkx".Contains(currentChar)) { endString += "k"; }
                    else if (currentChar == 'p') { endString += "f"; }
                    else if (currentChar == 'g') { endString += "n"; }
                    else { endString += currentChar; }
                }
                else if (level >= 3)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("vbct".Contains(currentChar)) { endString += "e"; }
                    else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                    else if ("sz".Contains(currentChar)) { endString += "s"; }
                    else if ("qkx".Contains(currentChar)) { endString += "k"; }
                    else if (currentChar == 'd') { endString += "m"; }
                    else if (currentChar == 'p') { endString += "f"; }
                    else if (currentChar == 'g') { endString += "h"; }
                    else { endString += currentChar; }
                }
                else if (level >= 2)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("ct".Contains(currentChar)) { endString += "e"; }
                    else if ("jlr".Contains(currentChar)) { endString += "a"; }
                    else if ("qkx".Contains(currentChar)) { endString += "k"; }
                    else if ("dmg".Contains(currentChar)) { endString += "m"; }
                    else if (currentChar == 's') { endString += "z"; }
                    else if (currentChar == 'z') { endString += "s"; }
                    else if (currentChar == 'f') { endString += "h"; }
                    else { endString += currentChar; }
                }
                else if (level >= 1)
                {
                    if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                    else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                    else if ("jlr".Contains(currentChar)) { endString += "a"; }
                    else if ("cqkx".Contains(currentChar)) { endString += "k"; }
                    else if ("dmg".Contains(currentChar)) { endString += "m"; }
                    else if (currentChar == 't') { endString += "e"; }
                    else if (currentChar == 'z') { endString += "s"; }
                    else if (currentChar == 'f') { endString += "h"; }
                    else { endString += currentChar; }
                }
            }
            return endString;
        }
    }
}