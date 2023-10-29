# Dalamud-GagSpeak 


![Banner Image](Assets/GagSpeakBannerAlt.png)
## Code Structure Documentation
**The Hierarchy of our code works as follows:**

Our code is based upon `GagSpeak.cs`, which is our highest level application.
For `GagSpeak.cs` to run, along with all other applications within it, it needs it's own set of **Services**

These Services are compiled into the **Services** Folder, where `ServiceHandler.cs` compiles all the services from `DalamudServices.cs`, along with all other services in our plugin.

>Think of a **Service** like a compiled class within our namespace. For this reason, when our `ServiceHandler.cs` Makes a service collection, it begins by taking `DalamudServices.cs`, but then adds all our other existing files into the service collection. These classes are compiled into catagories. Namely;
- Chat
- Data
- Events
- Services
- UI (Utils included)

### Chat Services 
>These are the classes that handle our chatGui's chatlog messages, scanning each message that goes through to detect if it meets any of our parameters. We use chat services for the following:
- We need to have a way to send trigger messages between whitelisted people, and so we will have **Coded messages sent through a disguised tell** These coded tells will be able to:
  - Allow others to use /gag (target) commands on other whitelisted players
  - Lock whitelisted players gags with spesified padlocks
  - provide passwords for things such as mistress padlocks
- We will need to have a way to **altar players chat messages in allowed chat types to garbled chat messages, (client side only)**
- We will need to **Print built SeStrings into current chat type as a chat message** based off of /gs

### Data Services
>These are the classes that handle our plugins configuration data, and command manager systems. We use our Data Services for the following:
- Dictating the way our commands work, their display, and functionality
- Allowing the Storing of information about the plugin and for the information to be retained from login to login
- Saving and interacting with config (configuration) data

### Events Services
> (Still fully figuring this stuff out) Events are the classes that trigger upon certain actions being executed. They are performed in their own classes seperate from other classes because they are more general purpose, and can be triggered in more than one catagory of the plugin.
- TabSelected is the only current event, may look into the view about this more later.


### Services Service
> The name may sound redundant, but there are still other services outside of the `ServiceHandler.cs` and `DalamudServices.cs` files, such as:
- Save Service: Which acts as a service to dictate what files we want to save and call upon the framework to execute its handler
- FilenameService: Which are what we use to dictate what files the SaveService saves
- HistoryService: Which acts as the service handler for storing the history of our translations from the `HistoryWindow.cs`, while we let the history windows tab simply worry about actually displaying the information.

### UI Services
> These are the classes which structure anything having to do with user interface. These are all managed by the `WindowManager.cs`, which builds the `MainWindow.cs` and the `HistoryWindow.cs`. Each of these windows are seperate windows that the plugin can display, independant to eachother.
- MainWindow contains 3 tabs;
  - GeneralTab, which displays a space for you to input your safeword, along with information about which gags are equipped, their gaglock type, and (potentially) a display of their gag icon.
  - WhitelistTab, which displays a list of players who you trust. These people will be able to interact with your plugins interface window, and assign gag commands to you, and also see any messages you write as garbled speak, if allowed.
  - ConfigSettingsTab, which gives you options to select filters for your text output, only allowing them to work under certain conditions you allow. Ontop of this, you can also isolate these commands to only work under certain channel types.
- HistoryWindow, which displays the history of all your garbled messages, including the original text and the translated text incase you want to copy and paste it somewhere else.


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
- Overview code, comment EVERYTHING, seriously. This shit is rarely documented at all, having at least one plugin to reference would be amazing, and this could be the first good example, so lets make it one.

## Gamplan to Tackle for translations:
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


### References & Links:
Beginning of the code for gagspeak stuff
https://gitgud.io/BondageProjects/Bondage-College/-/blob/master/BondageClub/Scripts/Speech.js#L296

List of gag levels:
https://gist.github.com/bananarama92/9c7a11b8263bddd116a7f94973c9272c#file-gag_level-yaml

