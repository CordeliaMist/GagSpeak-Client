using System.Collections.Generic;
using System;

namespace GagSpeak.Events;
// by defauly, action events for lists will only fire whenever the list is replaced by an entirely new list.
// Because we dont want this kind of action detection, but rather detect whenever an index within a list is changed, we make this event.
// This event will instead fire an event every time that an INDEX item in our list is either replaced or modified
public class ObservableList<T> : List<T>
{
    public delegate void ItemChangedEventHandler(object sender, ItemChangedEventArgs e); // define the event handler
    public event ItemChangedEventHandler ItemChanged; // define the event

    public new T this[int index] { // override the indexer
        get => base[index]; // get the item
        set {               // set the item
            var oldValue = base[index];
            base[index] = value;
            ItemChanged?.Invoke(this, new ItemChangedEventArgs(index, oldValue, value)); // but this time, fire the invoker too.
        }
    }
}

// define the event args, also store the information about the old and new values at that index when changed.
public class ItemChangedEventArgs : EventArgs
{
    public int Index { get; }
    public object OldValue { get; }
    public object NewValue { get; }

    public ItemChangedEventArgs(int index, object oldValue, object newValue) {
        Index = index;
        OldValue = oldValue;
        NewValue = newValue;
    }
}