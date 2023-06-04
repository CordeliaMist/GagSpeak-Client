# Dalamud-GagSpeak
This Project is the Development of a project which will eventually end up as a Dalamud plugin that can automatically translate sent messages to gagspeak / garbled messages.

# Our Project Goals
## Sprint Goal 1:
We want to create a plugin that has a simple list of 5 gags, where we give each gag a score for the garble. the total score will calculate the degree of the garble translation. The plugin window will have a textbox where a user can put in a message, and upon pressing a button, the message will be auto-translated and copied to the persons clipboard.
## Sprint Goal 2:
We want to make the plugin try to highlight and cut the message a user wishes to put into the chat and automatically translate it and paste it into the chat only if a checkbox is selected which enables the plugin.
## Sprint Goal 3:
We want to append a list similar to chatbubbles, which allows the garble to only function when the message is sent into a chat window type that the user allows it to.
Additionally if time allows, we add a option for the garble to only work on messages being sent to friends, and to disable in any instance as well, if not can cross over to sprint 4
## Sprint Goal 4:
We want to  append commands to our plugin the user can type that will automatically enable or disable their gags.
`/gag panel` for example, will enable the panelgag option in the gag list multiselection. Typing it again will disable it.
The user can also type `/ungagall` to uncheck all options, effectively disabling any garble in an instant.
## Sprint Goal 5: 
we want to try and improve upon the number of gags in our list, and make it sectioned well so we can add more in the future. We also want to add any extra security we may need, and look over our code to make sure everything works correctly
## Sprint Goal 6:
Peer pressure mare devs into allowing us to send information like this over or see if there is a way for people to create a list of trusted users in their config that would be able to gag them themselves (high security risk, may not implement), (think like glamourer fixed design list)


### References & Links:
Beginning of the code for gagspeak stuff
https://gitgud.io/BondageProjects/Bondage-College/-/blob/master/BondageClub/Scripts/Speech.js#L296

List of gag levels:
https://gist.github.com/bananarama92/9c7a11b8263bddd116a7f94973c9272c#file-gag_level-yaml
