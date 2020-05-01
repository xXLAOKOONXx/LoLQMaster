# LoLQMaster
Changing league of legends settings based on the queue you are in. (e.g. Poro Icon for Poro King)

## Disclaimer
This application is not reviewed by Riot yet and therefore not open for use.  
  
On the Korean server applications using the LCU Interface are not allowed. As this application uses LCU Interfaces you are not allowed to use the application on Korean server. [Riot Article](https://www.riotgames.com/en/DevRel/changes-to-the-lcu-api-policy)
  
LolQMaster isn't endorsed by Riot Games and doesn't reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games, and all associated properties are trademarks or registered trademarks of Riot Games, Inc.

## User Guide

### Installation
In [Releases](https://github.com/xXLaokoonXx/LoLQMaster/releases "LolQMaster/Releases") you can find the latest build ready to use for Windows.
1. Download latest zip folder.
2. Unzip the folder whereever you would like to store the application.
3. Use 'LolQMaster.exe' inside the folder to start the application.
4. The application need to be open and running (as window visible for you) to work. If you close the window all connected services will shut down as well.

### Step-by-Step Example
![Main Window](https://github.com/xXLaokoonXx/LoLQMaster/blob/master/images/UI_MainWindow.png?raw=true "Main Window")
On the left side you can see:
- Your current summoner icon
- Your summoner name
- Information about the connection to League Client
You can click on the different queues to unfold them.
![Main Window - Extended View](https://github.com/xXLaokoonXx/LoLQMaster/blob/master/images/UI_MainWindow_extendedView.png?raw=true "Main Window - Extended View")
There you can either delete the queues (except of default) or change the image.  
When you click on "Add Queue" you land on another window.
![Add Queue](https://github.com/xXLaokoonXx/LoLQMaster/blob/master/images/UI_AddQueueWindow.png?raw=true "Add Queue")
On the left side you can select the Queue you would like to add.  
On the right side you can select the summoner icon you would like to use.  
Whether you click on "Pick icon" on this window or you click "Change icon" on the main window you land in the icon picker.
![Icon Picker](https://github.com/xXLaokoonXx/LoLQMaster/blob/master/images/UI_IconPicker.png?raw=true "Icon Picker")
With a click on the summoner icon of your choice you will get back to your last window.  
  
  
![Zac Passive](https://github.com/xXLaokoonXx/LoLQMaster/blob/master/images/ZacPassive.png?raw=true "Zac Passive")
!IMPORTANT! You will notice Zac's passive icon is there as well. Unfortunately you can not have that one as your summoner icon, but instead this icon represents not changing your icon.
  
  
Have Fun!

# DEV Stuff
## Nuget Packages used
- WebsocketSharp
- RestSharp
- NewtonSoft.Json
- RiotSharp (included in localNuggetPackages folder as there is no direct way to draw the current version)
## LCU EndPoints used
- GET /lol-summoner/v1/current-summoner
- PUT /lol-summoner/v1/current-summoner/icon
- /wamp (WebSocket)
  - OnJsonApiEvent_lol-summoner_v1_current-summoner
  - OnJsonApiEvent_lol-login_v1_login-data-packet
  - OnJsonApiEvent_lol-gameflow_v1_session
