# PatchReqRes
KSP mod to profile resource handling

This project uses Mono.Cecil to insert timing code into certain functions in the main KSP assembly.  A simple KSP plugin 
then displays a graph of the percentage of each realtime second that is spent in the functions.

Setup of the "mod" is a little complex because I can't be bothered to make it any nicer.

* Load solution into VS2013
* Build Release configuration
* Find your KSP_Data\Managed folder
* Copy Assembly-CSharp.dll as Assembly-CSharp.orig.dll
* Copy TimerLib\bin\Release\TimerLib.dll into KSP_Data\Managed
* Copy PatchAsm\bin\Release\PatchAsm.exe into KSP_Data\Managed and run it
* Copy ReqResGraph\bin\Release\ReqResGraph.dll into GameData
* Run KSP and hit Mod-Minus at any time to toggle the display of the graph
