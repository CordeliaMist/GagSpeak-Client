using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GagSpeak.Hardcore;

public interface ITextNode
{
    public string Name { get; }
}

public class TextEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;
    [JsonIgnore]
    public string Name { get { return Text; } }
    public string Text { get; set; } = string.Empty;
    public string[] Options { get; set; } = new string[]{"Yes", "No"};
    public int SelectThisIndex { get; set; } = 1;
}

public class TextFolderNode : ITextNode
{
    public string Name { get; set; } = string.Empty;

    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<ITextNode> Children { get; } = new();

    // helper function to prune any empty entires
    public void PruneEmpty() {
        // remove any child enrty where the type is a TextEntryNode and it has empty text
        Children.RemoveAll(x => x is TextEntryNode folder && folder.Text == string.Empty);
    }

    public void CheckAndInsertRequired() {
        // if there are no entries in the children with the text "Leave Estate" with selection "no" and "Leave Private Chambers" with selection "nothing.",
        // then append them to the start of the node list
        if(Children.Count == 0) {
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Text = "Leave the estate hall?",
                Options = new string[]{"Yes", "No"},
                SelectThisIndex = 1
            });
            Children.Add(new TextEntryNode {
                Enabled = true,
                Text = "Leave private chambers.",
                Options = new string[]{"Leave private chambers.", "Move to specified private chambers.", "Nothing."},
                SelectThisIndex = 2
            });
        }
        // otherwise, scan to make sure we have them
        if (Children.All(x => x.Name != "Leave the estate hall?")) {
            // then insert the leave estate marker at the start of the list
            Children.Insert(0, new TextEntryNode { 
                Enabled = true,
                Text = "Leave the estate hall?",
                Options = new string[]{"Yes", "No"},
                SelectThisIndex = 1
            });
        }
        // now check for the leave private chambers
        if (Children.All(x => x.Name != "Leave private chambers.")) {
            Children.Insert(0, new TextEntryNode {
                Enabled = true,
                Text = "Leave private chambers.",
                Options = new string[]{"Leave private chambers.", "Move to specified private chambers.", "Nothing."},
                SelectThisIndex = 2
            });
        }
    }
}

// the class to handle the custom serialization to file
public class ConcreteNodeConverter : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(ITextNode);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var jType = jObject["$type"]!.Value<string>();

        if (jType == SimpleName(typeof(TextEntryNode)))
        {
            return CreateObject<TextEntryNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(TextFolderNode)))
        {
            return CreateObject<TextFolderNode>(jObject, serializer);
        }
        else
        {
            throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    private static T CreateObject<T>(JObject jObject, JsonSerializer serializer) where T : new()
    {
        var obj = new T();
        serializer.Populate(jObject.CreateReader(), obj);
        return obj;
    }

    private static string SimpleName(Type type)
    {
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}
