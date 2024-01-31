using System.Collections.Generic; // for lists
using System;                     // for basic C# types

namespace GagSpeak.Events;

/// <summary>
/// WatchList class extends List to fire an event every time an item in the list is replaced or modified.
/// </summary>
public class WatchList<T> : List<T>
{
    public delegate void ItemChangedEventHandler(object sender, ItemChangedEventArgs e); // Define the delegate for the ItemChanged event handler
    public event ItemChangedEventHandler?   ItemChanged;                                    // Define the ItemChanged event
    public bool                             IsSafewordCommandExecuting { get; set; }        // Indicate if safeword command is being executed 

    // Override the indexer to fire the ItemChanged event when an item is set
    public new T this[int index] {
        get => base[index]; // Get the item at the index
        set {
            var oldValue = base[index]; // Store the old value
            base[index] = value; // Set the new value
            // If the safeword command is not being executed, fire the ItemChanged event
            if (!IsSafewordCommandExecuting) {
                ItemChanged?.Invoke(this, new ItemChangedEventArgs(index, oldValue, value));
            }
        }
    }
}

/// <summary> ItemChangedEventArgs class to hold the details of the item changed event. </summary>
public class ItemChangedEventArgs : EventArgs
{
    // Index of the item that was changed
    public int Index { get; }
    // Old value of the item
    public object? OldValue { get; }
    // New value of the item
    public object? NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemChangedEventArgs"/> class.
    /// <list type="bullet">
    /// <item><c>index</c><param name="index"> - The index of the item that was changed.</param></item>
    /// <item><c>oldValue</c><param name="oldValue"> - The old value of the item.</param></item>
    /// <item><c>newValue</c><param name="newValue"> - The new value of the item.</param></item>
    /// </list> </summary>
    public ItemChangedEventArgs(int index, object? oldValue, object? newValue) {
        Index = index;
        OldValue = oldValue;
        NewValue = newValue;
    }
}