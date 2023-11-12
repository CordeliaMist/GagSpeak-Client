using GagSpeak.Services;
using System.Collections.Generic;


// An Interface to store information about the gags and padlocks.
namespace GagSpeak.Data;

public interface IGagPadlocksData
{
    List<string> SelectedGagTypes { get; set; }
    List<GagPadlocks> SelectedGagPadlocks { get; set; }
    List<string> SelectedGagPadlocksPassword { get; set; }
    List<string> SelectedGagPadlocksAssigner { get; set; }
}
