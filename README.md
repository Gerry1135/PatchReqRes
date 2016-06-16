# ProfileGraph
KSP mod to profile stock code

This project uses Mono.Cecil to insert timing code into functions in the main KSP assembly.  A simple KSP plugin then displays a graph of the percentage of each realtime second that is spent in the functions.

The profiling is currently very simple but it does correctly handle nested functions totalling the time spent inside any of the profiled functions.

## Projects
### PatchAsm
This is the patcher program that inserts calls to the timing functions in ProfileGraph into functions in KSP.  It reads which functions to patch from the profile.cfg file in the ProfileGraph mod folder.  It applies the patches to a backup copy of the main KSP assembly and then writes out the modified one to the standard name.

### ProfileGraph
This is a simple KSP plugin mod that displays a graph of the percentage of time spent in the profiled functions.  It uses the same profile.cfg file to provide useful names for the individual graphs. It measures the elapsed real time in FixedUpdate, Update and LateUpdate and, once at least a second has passed, it calculates the percentage of time spent in each function.  The graphs are 100 pixels high and shows the percentage directly.  The name of the channel and the last percentage value is displayed below the graph.

## Setup
Installation of the "mod" is very simple.  Copy the ProfileGraph folder from the installation zip into your GameData folder.  Copy PatchAsm.exe into KSP_Data or KSP_x64_Data depending on which version you are running and then run it.  It should display information about what it has patched.

If you wish to change the functions being profiled then you will need to quit KSP, edit the profile.cfg file in the ProfileGraph\PluginData\ProfileGraph folder and then run PatchAsm.exe again.  Each line in the file corresponds to a single channel and is a comma separated list, starting with the name to display under the graph and followed by pairs of strings giving the type name and function name of the functions to be included in the channel.
