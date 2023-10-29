using System;
using OtterGui.Classes;
using GagSpeak.UI;
//using GagSpeak.Styles;


//Practicing Modular Styles
namespace GagSpeak.Events;
 
/// <summary>
/// Triggered when an automated Styles is changed in any way.
/// <list type="number">
///     <item>Parameter is the tab to select. </item>
///     <item>Parameter is the Styles to select if the tab is the Styles tab. </item>
/// </list>
/// </summary>
public sealed class TabSelected //: EventWrapper<Action<MainWindow.TabType, Style?>, TabSelected.Priority>
{
    public enum Priority
    {
        /// <seealso cref="Gui.Tabs.StylesTab.StylesFileSystemSelector.OnTabSelected"/>
        Styleselector = 0,

        /// <seealso cref="Gui.MainWindow.OnTabSelected"/>
        MainWindow = 1,
    }

    public TabSelected()
        //: base(nameof(TabSelected))
    { }


//     // [ Styles? ] means that the Styles can be either a actual Styles or null.
//     public void Invoke(MainWindow.TabType type, Style? Styles)
//         => Invoke(this, type, Styles);
}
