# <div align="center">MouseMasterVR</div>
<div align="center">


![https://i.imgur.com/KqsIJLy.png](https://i.imgur.com/KqsIJLy.png)
#### by SemlerPDX
## Mouse Cursor Centering & Scroll Wheel Rebind App 
</div>

This is a very simple mouse cursor centering app which forces the mouse to remain centered when the target application is in focus, set to Falcon BMS by default.  This feature is designed for use with certain software in VR where constant mouse centering is desireable, such as the new VR mode for Falcon BMS.

When this program is running, and the ON/OFF toggle switch is ON, and the target program is in focus, the mouse cannot be moved from center - use ALT+TAB to switch to this app to toggle the main 'power' switch.  You can use the hotkeys CTRL+Arrow Key [any] to snap the app to the center of the primary screen.  **When used with Falcon BMS as the target, mouse centering will only engage when flying.  Of course, latest BMS update (U1) includes this centering option natively, and so this functionality example is dated at time of release, and will simply remain as a learning project and open source application demonstration, or may even develop into a more complete mouse utility app.**

To extend the usefulness of this application, users can also rebind the mouse scroll wheel up or down actions to any single keypress.  A half-second debounce delay lets us scroll forward or back several ticks in a single action to output a single keypress, preventing the wheel from issuing a string of keypresses - no need to carefully tick the scroll wheel just one bump.


### __**Current features:**__
- Mouse Cursor Centering when target process is in focus, and only if main toggle is ON
- Mouse Centering for Falcon BMS only when flying in cockpit via BMS Shared Memory data
- Mouse Scroll Up/Down can be rebound to issue any (single) keypress, no modifier keys (yet)
- Mouse Scroll rebinds are not 'one keypress per detent', with a default 0.5 second debounce
- Hotkeys when app in focus, CTRL+Arrow Key [any] to center app, ALT+F4 to gracefully close
- Manual or Automatic Updates will present pop-up choice if update found on app launch
   

<div align="center"><br>

### ***Click link to DOWNLOAD, or clone the repository above & compile the app!***
__[https://veterans-gaming.com/semlerpdx/vglabs/apps/mousemastervr/](https://veterans-gaming.com/semlerpdx/vglabs/apps/mousemastervr/)__

#### Latest Changelog & Checksum:  [CLICK HERE](https://veterans-gaming.com/semlerpdx/vglabs/apps/changelog_mousemastervr.html)

![https://i.imgur.com/hzMiFjt.png](https://i.imgur.com/hzMiFjt.png)
![https://i.imgur.com/ErQA3X2.png](https://i.imgur.com/ErQA3X2.png)
![https://i.imgur.com/zb0PLPl.png](https://i.imgur.com/zb0PLPl.png)
![https://i.imgur.com/gfPcJ5Q.png](https://i.imgur.com/gfPcJ5Q.png)
</div>

<div align="center" style="color:#8a0000">


### This app is in a Public Beta Test as of Jan2023 - Please report any bugs or issues!
![https://i.imgur.com/kYtxqur_d.jpg?maxwidth=520&shape=thumb&fidelity=high](https://i.imgur.com/kYtxqur_d.jpg?maxwidth=520&shape=thumb&fidelity=high)
</div>

This little app does just a few things, for some fairly specific circumstances such as VR applications without a native mouse cursor centering option, or for rare occasions where we'd like to rebind our mouse scroll wheel up and down actions to a keypress of our choosing.  I've had more than a few mice with included rebinding software, and have never seen an option for setting a scroll wheel keypress rebind for games. I'd entertain any ideas for related mouse features to add, or things I can improve.

### __**Planned features:**__
- Mouse Rebinds which could allow any joystick button to be set as Mouse Left or Right Click
- Mouse Scroll Up/Down rebind option to output any existing joystick or Xbox controller button
- Generic Mouse Button rebind for those cheap mice with thumb buttons but no rebind software

____

**Questions I have for this Public Beta:**

***Does this work and work well?  Should I improve or change anything (beyond the planned features above)?  Should I include any other features? Am I using MVVM concepts properly?***
____

***Authors Note***

*I have past experience with Windows Forms Apps and C#, but this is my first real use of GitHub and my first WPF app - I'm writing in Visual Studio Community 2019, and I use PhotoShop for my images.  While I'm as self-conscious as any self-taught coder new to something, I very much want feedback on my structure and use of methods as it relates to best practices or common solutions in WPF, and my attempt to follow an MVVM structure. I expect I have made several odd choices and potentially non-standard methods. I hope to continue making WPF apps, and that this is the first of many such open source projects.*

*Thanks for checking out MouseMasterVR!*
</div>
<div align="right">

*- SemlerPDX Jan2023 -*</div>

