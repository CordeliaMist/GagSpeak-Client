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
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string[] Options { get; set; } = new string[]{"No", "Yes"};
    public int SelectThisIndex { get; set; } = 0;
}

public class TextFolderNode : ITextNode
{
    public string Name { get; set; } = string.Empty;

    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<TextEntryNode> Children { get; } = new();

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
                Label = "Exit Apartment Room (Other)",
                Text = "Exit",
                Options = new string[] {
                    "Go outside the building",
                    "Go to specified apartment",
                    "Go to your apartment",
                    "Go to the lobby",
                    "Cancel"},
                SelectThisIndex = 4
            });
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Label = "Exit Apartment Room (Yours)",
                Text = "Exit",
                Options = new string[] {
                    "Go outside the building",
                    "Go to specified apartment",
                    "Go to the lobby",
                    "Cancel"},
                SelectThisIndex = 3
            });
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Label = "(FC House / Any Personal House)",
                Text = "Leave the estate hall?",
                Options = new string[] {"No", "Yes"},
                SelectThisIndex = 0
            });
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Label = "Leave Private Chambers (Other)",
                Text = "Exit",
                Options = new string[] {
                    "Leave private chambers.",
                    "Move to specified private chambers.",
                    "Nothing."},
                SelectThisIndex = 2
            });
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Label = "Leave Private Chambers (With Your Chamber Option)",
                Text = "Exit",
                Options = new string[] {
                    "Leave private chambers.",
                    "Move to specified private chambers.",
                    "Move to your private chambers.",
                    "Nothing."},
                SelectThisIndex = 3
            });
            Children.Add(new TextEntryNode { 
                Enabled = true,
                Label = "(Prevents logout escape)",
                Text = "Enter the estate hall?",
                Options = new string[] {"No", "Yes"},
                SelectThisIndex = 1
            });
        }

        // Check for "Exit Apartment Room (Other)"
        if (Children.All(x => x.Label != "Exit Apartment Room (Other)")) {
            Children.Insert(0, new TextEntryNode { 
                Enabled = true,
                Label = "Exit Apartment Room (Other)",
                Text = "Exit",
                Options = new string[] {
                    "Go outside the building",
                    "Go to specified apartment",
                    "Go to your apartment",
                    "Go to the lobby",
                    "Cancel"},
                SelectThisIndex = 4
            });
        }
        // Check for "Exit Apartment Room (Yours)"
        if (Children.All(x => x.Label != "Exit Apartment Room (Yours)")) {
            Children.Insert(1, new TextEntryNode { 
                Enabled = true,
                Label = "Exit Apartment Room (Yours)",
                Text = "Exit",
                Options = new string[] {
                    "Go outside the building",
                    "Go to specified apartment",
                    "Go to the lobby",
                    "Cancel"},
                SelectThisIndex = 3
            });
        }
        // Check for "(FC House / Any Personal House)"
        if (Children.All(x => x.Label != "(FC House / Any Personal House)")) {
            Children.Insert(2, new TextEntryNode { 
                Enabled = true,
                Label = "(FC House / Any Personal House)",
                Text = "Leave the estate hall?",
                Options = new string[] {"No", "Yes"},
                SelectThisIndex = 0
            });
        }
        // Check for "Leave Private Chambers (Other)"
        if (Children.All(x => x.Label != "Leave Private Chambers (Other)")) {
            Children.Insert(3, new TextEntryNode { 
                Enabled = true,
                Label = "Leave Private Chambers (Other)",
                Text = "Exit",
                Options = new string[] {
                    "Leave private chambers.",
                    "Move to specified private chambers.",
                    "Nothing."},
                SelectThisIndex = 2
            });
        }

        // Check for "Leave Private Chambers (With Your Chamber Option)"
        if (Children.All(x => x.Label != "Leave Private Chambers (With Your Chamber Option)")) {
            Children.Insert(4, new TextEntryNode { 
                Enabled = true,
                Label = "Leave Private Chambers (With Your Chamber Option)",
                Text = "Exit",
                Options = new string[] {
                    "Leave private chambers.",
                    "Move to specified private chambers.",
                    "Move to your private chambers.",
                    "Nothing."},
                SelectThisIndex = 3
            });
        }

        // Check for "(Prevents logout escape)"
        if (Children.All(x => x.Label != "(Prevents logout escape)")) {
            Children.Insert(5, new TextEntryNode { 
                Enabled = true,
                Label = "(Prevents logout escape)",
                Text = "Enter the estate hall?",
                Options = new string[] {"No", "Yes"},
                SelectThisIndex = 1
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

        if (jObject["Children"] != null)
        {
            return CreateObject<TextFolderNode>(jObject, serializer);
        }
        else
        {
            return CreateObject<TextEntryNode>(jObject, serializer);
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
