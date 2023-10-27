# Dalamud-GagSpeak 
This Project is the Development of a project which will eventually end up as a Dalamud plugin that can automatically translate sent messages to gagspeak / garbled messages.

![Banner Image](Assets/GagSpeakBannerAlt.png)
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

# Gamplan to Tackle for translations:
- Until there is a way to modify chat messages on being sent, the only way to translation messages is to do /gs (message).
- In order to give commands to other people, we will make use of a whitelist filter
- It makes more sence to use SeString building over payload manipulation, as payload manip is only client side.

How to tackle commands that order other players?
- Convert the command into a tell that is sent to the respective player
- The tell should only go through if the person is on your whitelist
- The player should be added on both ends for this to work
- So long as the player is filtered on your whitelist, the tell will be hidden from the chat on both ends client.
- The tell will then be intercepted and issued as a command 

How to tackle chat mufflers.
- use /gs (message), everything after /gs is taken in.
- Use the message as the "arguements", and pass it in as a text to be appended to a payload that can be sent as a message into your respective currently selected chat window type

How to tackle gag selections and UI things.
- For now im not quite sure, searchable dropdowns broke, and im getting lots of bugs with that kind of shit.
- Just make sure when a type is selected, that its selection stays visable. Look more into basic combo dropdowns at the moment, worry about search filters later.

https://github.com/Caraxi/SimpleTweaksPlugin/blob/8157cd81a9dccfc21d93234568b8d20902e3f612/Tweaks/CombatMovementControl.cs#L9
link for a sample of enum-based combo dropdowns with selection saving.



### References & Links:
Beginning of the code for gagspeak stuff
https://gitgud.io/BondageProjects/Bondage-College/-/blob/master/BondageClub/Scripts/Speech.js#L296

List of gag levels:
https://gist.github.com/bananarama92/9c7a11b8263bddd116a7f94973c9272c#file-gag_level-yaml

