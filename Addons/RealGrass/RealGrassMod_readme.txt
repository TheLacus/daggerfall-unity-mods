=================================================================================================
REAL GRASS

1.08 for DaggerFall Unity 0.4.12
=================================================================================================

**DESCRIPTION**
Adds grass and, optionally, water plants to the terrain of Daggerfall Unity.

It's available in two versions:

*RealGrass
This is the original mod made by Uncanny_Valley and improved by midopa.

*GrassAndPlants
A revision by TheLacus which also adds water grass and plants, according to differents climates, 
and little stones on farms ground.

**REQUIREMENTS**
Daggerfall Unity 0.4.12

**INSTALLATION**
Put the *.dfmod file 'DaggerfallUnity_Data/StreamingAssets/Mods'. 
Use the load order settings window to choose if you want water plants and stones or only grass, 
and customize the size and shape of grass.

**UNINSTALL**
If you wish to uninstall, you can safely remove the mod from the StreamingAssets folder. 
It won't affect saves.

**CREDITS**
Main developer: Uncanny_Valley
Additional changes: midopa
GrassAndPlants and conversion to mod-system: TheLacus

Real Grass on Daggerfall Unity forums: http://forums.dfworkshop.net/viewtopic.php?f=14&t=17

**COMPATIBILITY**
This mod is compatible with Nystul Distant Terrain. 
Be careful when installing other mods that affects terrain.

**CHANGELOG**
1.08
* Real Grass and Grass&Plants are now packed in the same mod for increased ease of use and maintenance.
* (optional) Added winter version of water plants.
* Customize size and shape of grass from the settings window.
* Minor improvements.

=================================================================================================
Original Grass&Plants Changelog
=================================================================================================
*1.0
Conversion from "build mod" to "mod-system mod".

=================================================================================================
Original Real Grass Readme
=================================================================================================

Name: Real Grass 
Made by: Uncanny_Valley
Additional changes: midopa
Mod version: TheLacus
For:  DaggerFall Tools For Unity (1.3.31)
Version: 1.07

This mod adds animated billboard grass in DaggerFall

1.07
*Converted to mod system.
Place realgrass.dfmod inside 'StreamingAssets/Mods'.
*Fixed bug which caused the wrong kind of grass to appear 
when loading a save made in a different climate.

1.06
*Properly clear grass when moving into an season/climate without grass.

1.05
*Adds more variety to grass patches
*Tweaks density of grass so patches near non-grass terrain look more natural (no "grass walls")
*Refactoring, cleanup, and comments

1.04
*Fixes error messages when traveling during winter in grassy climates

1.03
*Fixes a few misplaced grass placements
*Added a new type of grass!
*Improved the green grass texture (I'm no artist but I try)
*Cleaned up the code and refined it a bit further 


1.02
*Updated to work with version 1.3 of DaggerFall Tools For Unity
*You no longer need to edit existing DFTFU files to use Real Grass
*Code improvements, the loading time for the grass should be significant faster than before

How to use:
1. Add the prefab (pref_RealGrass) to the StreamingWorld Scene
2. Play and enjoy 