using System;

// This is only intended for RP purposes, and is not acceptable to be exploited in any other means nessisary.
[AttributeUsage(AttributeTargets.Method)]
internal class ClickNameAttribute : Attribute
{
    public ClickNameAttribute(string name) { this.Name = name; }
    public string Name { get; init; }
}
