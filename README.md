# Dalamud-GagSpeak 
This Project is the Development of a project which will eventually end up as a Dalamud plugin that can automatically translate sent messages to gagspeak / garbled messages.

![](https://github.com/CordeliaMist/Dalamud-GagSpeak/Assets/GagSpeakBanner alt.png)
# Our Project Goals
## Current Goals Accomplished:
- All Gags and Gag Types and Lock Types are appended
- Code has had full overhaul and now organized into corrisponding folders and conventional structure
- Code now is also diverse and handles many settings
- UI is coming along nicely
- Icon displays properly
- Garbler fully implemented
  
## Goals Left:
- Get the garbler to actually send the translated message back and append it to the payload
- Find a way to intercept the payload before it is sent out to the server, so both player and other players around can see the message (USE CAUTION FOR THIS)
- polish up the UI and find a formal way to do it, will need to use OtterGUI for convention
- Make buttons actually functional, and get the debug menu properly working.
- Avoid any C++ or C habits, such as spamming arrays and strings everywhere, maintain to lists and dictionaries where possible.
- Overview code, comment EVERYTHING, seriously. This shit is rarely documented at all, having at least one plugin to reference would be amazing, and this could be the first good example, so lets make it one.


### References & Links:
Beginning of the code for gagspeak stuff
https://gitgud.io/BondageProjects/Bondage-College/-/blob/master/BondageClub/Scripts/Speech.js#L296

List of gag levels:
https://gist.github.com/bananarama92/9c7a11b8263bddd116a7f94973c9272c#file-gag_level-yaml

