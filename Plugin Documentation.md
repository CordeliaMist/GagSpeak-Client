# Documentation for Aspiring Plugin Developers

Hello all, im sure for anyone reading this that wants to get an understanding into plugin development, you may find that often the sample plugin provided by dalamud, coupled by the plugin-dev channel in the dalamud discord may be enough for those adapt in c#, but is not helpful enouguh for those looking to get into c# for plugin development.

Additionally, you may find that most, if not all plugins are very sparse in comments, so being able to link together why things connect they way they are, why things are set up in a particular manner they are, or how everything works as a whole.

**Well my friends, i can promise i can answer all of these to the best of my ability, but even if i dont know how everything works, i want to do my best to share what i DO know.**

## Main Points

* How Dalamud Plugins executable processes your code
  * Recommended code structures to follow
  * What you must -vs- should include
  * Linking namespaces properly
* The Importance of Modularization
* Project organization
  * Creating savable configuration files
  * What to and to not include in config files
  * Keeping your files clean and not messy
* Commenting formats
  
## How To Use this Plugin's code as a baseline to understand the dalamud plugin structure

### Getting Started

Thankfully for you, i have heavily commented ALL of my code, and all you need to do it browse through it on a local copy in your chosen IDE and let me walk you through it.

I personally used VSCode to write the code in, and VS to build my executables, but you can do what you wish! But, without any further to do though, let's begin.

### Prerequisites

This plugin documentation assumes all the following prerequisites are met:

* XIVLauncher, FINAL FANTASY XIV, and Dalamud have all been installed and the game has been run with Dalamud at least once.
* XIVLauncher is installed to its default directories and configurations.
* You have some prior coding knowledge of any language (as usually for programmers adapting to another language is only limited by ones resources at their disposal to help adapt them to it)

## Getting Familiar with the GagSpeak Code Structure
To understand how dalamud plugins execute their code, you will first need to understand the way namespaces in c# work, and what must be included -vs what should be included.

### How Namespaces In C# Work
Namespaces are basically what you use to define what purpose you have in your overarching code is, along with also helping in assist the compiler to know the hierarchy of your structure.

For example, let's say that i want to make a plugin for counting the number of times i enter a message into anyones message book at a house.

1. Your highest level program should be called something like `MsgBookEntryCounter.cs` and inside of it have the namespace `namespace MsgBookEntryCounter`
2. The class within this file should then be called `public class MsgBookEntryCounter : IDalamudPlugin`
   
   This implement the `interface Dalamud.Plugin.IDalamudPlugin`, This interface represents a basic Dalamud plugin. All plugins have to implement this interface.

3. you can use namespaces to identify sections of your code so that they all interact together. If you want to store the data of each entry, you may have a class storing that data, so you can give it the namespace `namespace MsgBookEntryCounter.Data` and the class name be like `EntryData` or something. Then, back in your highest level .cs file, you make sure at the top to put `using MsgBookEntryCounter.Data` so that you can be sure to include it when you reference it.

### Class types in c#
You can create classes with the following keywords:
- `Public Class` - the defauly class case
- `Protected Class` - Only visible to other classes within it
- `Private Class` - Not visible to other classes 

Each of these can have these keywords embedded into them

- `Partial` - This class can access variables from other files in the same namespace that also have this class keyword
- `Unsafe` - This class cna execute unsafe operations, its probably not worth using this unless you know what you're dealing with
- `Internal` - This class can now operate on internal functions. These happen internally and can not be called upon by anything else (to my knowledge)

I'd honestly just recommend you make all your classes public and not become lazy use modularization over just making everything partial classes. I say this because the bigger your code gets the messier handling variables across partial classes becomes.

### What you MUST include -vs what you SHOULD include.

You must include the following for your plugin to build:

- Pretty much copy paste the code from the following files into your own, and just change out everhwere it says GagSpeak to your plugin name (it should match the name of your primary namespace) and reset the version down to 0.0.0.0
  - GagSpeak.csproj
  - Dalamud.Plugin.Bootstrap.targets
  - GagSpeak.json
  - Packages.lock.json
  - GagSpeak.sln
  - repo.json

From here, what to change should be self explanitory in the respective files. Im sure this much you can figure out.

One thing to note is that in the itemgroups / references part of the csproj, you can modify them to remove what you know your plugin wont need, and add what they will need. This basically tells the compiler when it makes a debug and release build, what to include in it and what not to.

---
What SHOULD be included:

- MODULARIZATION!!!! - This is huge, so do this PLEASE, you will thank me a million the moment you plugin starts to get complex. It will be 100x easier to debug and align things with eachother! How? Simple:
  - Ask yourself what your plugin will be about, and make your structure revolving around it. A good example strucutre to rely on is something like this:
    - **Assets** (used for containing anything like images or files you may want to include)
    - **Data** (for storing information based classes that you dont need to have saved in your config (usually))
    - **Events** (for tracking actions and what should happen when they are triggered)
    - **Interop** (if any, used to help refernece the bridge of execution between different applications)
    - **Services** (these are the structures behind your classes that helps organize and keep things taped together, quite literally)
    - **UI** (the files included for your interface elements)
    - **Utils** (files containing functions used all across your code to help with modularization and clean files, to prevent mass copy pasting across all your files with similar functions. Simply include `using YOUR_NAMESPACE.Utils` at the start of every file you want to call these functions from and they will be included!)
- Service collections - These are not nessisary, but I would highly recommend you use them, as they keep your plugin instanced. In other words, it has an individual execution for each time the plugin is loaded or unloaded, helping optimize your memory usage and keep everything aligned. (I seriously absolutely highly recommend you use this, it makes your life so much easier)

### The Big Question (and what made it all click for me), How does the Execution of Dalamud Plugins Work?

Dalamud plugins work like so: They are effectively one fat loop, that executes several times.

In other words, the main .cs file you have will execute once, setting up all of your services. Once they are setup, you are going to usually have a startup for your windows / UI, aka your UIBuilder. This UIBuilder is what is used to make your window system, and your interface elements. However, something very vitally important that I never understood until i figured it out myself, is that everything made by the UIBuilder is looped over several times a second to maintain a constant display on your screen.

This means, anything you define within these windows `DrawContent()` function is called several times a second. You should keep this in mind when making your plugin, because if you do something like:
```csharp
public void DrawContent() {
  string newstring = "";
  // some code...
  newstring = enteredstring;  
}
```
This is going to take up more resources than if you did
```csharp
string newstring = "";
public void DrawContent() {
  // some code...
  newstring = enteredstring;  
}
```
Because now it wont be making a new string several times a second.

In other words, if you are going to be creating any large variables or class arrays, probably dont do them inside of these functions, or add if statements to only change them when they are flagged as a update change or new entry.

Understanding this will save you a shit ton of resources, seriously. My plugin used to be 6000% higher in resource usage because i tried to create a new image to render INSIDE of my drawcontent loop to display, the moment i brought them out of the loop my usage rate went back down 6000%. It is seriously important knowledge to know and will save you an incredible headache down the line.

---
With all that being said, I believe you know enough now to start browsing through my plugin. I recommend any time you see me make use of a object with a class name label, you go into that class to see what it is doing, and see how everything is interconnected.

I also highly recommend that you take a look over the service collection and how that is set up, along with the config save and load service operations.

I hope this gives you great strides in starting up your plugin development, enjoy ♥