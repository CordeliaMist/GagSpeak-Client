using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gagspeak.Translator.ParseIPA
{
  public class IpaParserCantonese
  {
    private string IpaResult = "";

    public async Task UpdateResult() {
      var cWords = GetIpaTextBox().Split(' ');

      SetIpaTextBox("loading....");

      var obj = await GetIpaDb();

      string str = "";

      for (int i = 0; i < cWords.Length; i++) {
        if (obj.ContainsKey(cWords[i])) {
          /* check if allow_words_search is checked */
          if (true) {
            var sWords = new string[6];
            sWords[0] = cWords[i];
            sWords[1] = sWords[0] + cWords[i + 1];
            sWords[2] = sWords[1] + cWords[i + 2];
            sWords[3] = sWords[2] + cWords[i + 3];
            sWords[4] = sWords[3] + cWords[i + 4];
            sWords[5] = sWords[4] + cWords[i + 5];

            int wordsIndex = 0;
            if (obj.ContainsKey(sWords[5])) wordsIndex = 5;
            else if (obj.ContainsKey(sWords[4])) wordsIndex = 4;
            else if (obj.ContainsKey(sWords[3])) wordsIndex = 3;
            else if (obj.ContainsKey(sWords[2])) wordsIndex = 2;
            else if (obj.ContainsKey(sWords[1])) wordsIndex = 1;
            else if (obj.ContainsKey(sWords[0])) wordsIndex = 0;

            var searchWords = sWords[wordsIndex];
            
            str += $"({searchWords} /{obj[searchWords]}/ )";
            
            i += wordsIndex;
          } else {
            str += $"{cWords[i]}/{obj[cWords[i]]}/ ";

          }
        } else {
          str += $"{cWords[i]} ";
        }
      }
      SetIpaTextBox(str);
    }

    private async Task<Dictionary<string, string>> GetIpaDb() {
      var response = await client.GetStringAsync("./yue.json");
      var myObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

      return myObj;
    }
    private string GetIpaTextBox() {
      // This depends on your UI framework
      // For example, in WPF, you might do something like this:
      // return cWords_tBox.Text;
      return "";
    }

    private void SetIpaTextBox(string value = null) {
      // Set the value of IPA_tBox
      // This depends on your UI framework
      // For example, in WPF, you might do something like this:
      // IPA_tBox.Text = FormatMain(value ?? IpaResult);
    }

    private string FormatMain(string tStr) {
      string fStr = tStr;
      // Replace the following if checks with the appropriate checks for your UI framework
      if (/* check if IPA_num is checked */) fStr = FormatIpaNum(tStr);
      else if (/* check if IPA_org is checked */) fStr = FormatIpaOrg(tStr);
      else if (/* check if Jyutping is checked */) fStr = FormatJyutping(tStr);

      return fStr;
    }

    private string FormatIpaOrg(string x)
    {
      return x;
    }
    private string FormatIpaNum(string x)
    {
      x = x.Replace("˥", "5");
      x = x.Replace("˧", "3");
      x = x.Replace("˨", "2");
      x = x.Replace("˩", "1");
      x = x.Replace(":", "");
      return x;
    }

    private string FormatJyutping(string x)
    {
      x = x.Replace("˥˧", "1");
      x = x.Replace("˥˥", "1");
      x = x.Replace("˧˥", "2");
      x = x.Replace("˧˥", "2");
      x = x.Replace("˧˧", "3");
      x = x.Replace("˨˩", "4");
      x = x.Replace("˩˩", "4");
      x = x.Replace("˩˧", "5");
      x = x.Replace("˨˧", "5");
      x = x.Replace("˨˨", "6");

      x = x.Replace("k˥", "k7");
      x = x.Replace("k˧", "k8");
      x = x.Replace("k˨", "k9");

      x = x.Replace("t˥", "t7");
      x = x.Replace("t˧", "t8");
      x = x.Replace("t˨", "t9");

      x = x.Replace("p˥", "p7");
      x = x.Replace("p˧", "p8");
      x = x.Replace("p˨", "p9");

      x = x.Replace("˥", "1");
      x = x.Replace("˧", "3");
      x = x.Replace("˨", "6");

      x = x.Replace(":", "");
      return x;
    }
  }
}