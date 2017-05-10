=================================================================================================
REAL GRASS

1.09 for DaggerFall Unity 0.4.12
=================================================================================================

**DESCRIPTION**
-------------------------------------------------------------------------------------------------
Adds grass and, optionally, water plants to the terrain of Daggerfall Unity.
There are two variants of grass, varying for different regions.

It's available in two versions:

*RealGrass
This is the original mod made by Uncanny_Valley and improved by midopa.

*GrassAndPlants
A revision by TheLacus which also adds water grass and plants, according to differents climates, 
and little stones on farms ground.

You can choose which version to use from the mod settings.

**REQUIREMENTS**
-------------------------------------------------------------------------------------------------
Daggerfall Unity 0.4.12

**INSTALLATION**
-------------------------------------------------------------------------------------------------
Put the *.dfmod file 'DaggerfallUnity_Data/StreamingAssets/Mods'. 
Use the load order settings window to choose if you want water plants and stones or only grass, 
and customize the size and shape of grass.

**CUSTOMIZATION**
-------------------------------------------------------------------------------------------------
It is possible to customize this mod from the mod settings in-game. 
Presets with default values are avilable, but it's possible to customize them individually.
If you create an interesting combination, do not forget to share your own preset!

- WaterPlants
Places vegetation near water areas, like lakes and river.
There is a total of four different plants, for the same number of climate regions: mountain, temperate, 
desert and swamp. They all have two variations, summer and winter. 
You can choose to show the winter version during this season, or not to show them like the grass.

Additionally it places waterlilies above the surface of temperate lakes and some tufts 
of grass inside the mountain water zones.
Plants bend in the wind and waterlilies move slightly on the water surface moved by the same wind. 

- TerrainStones 
Places little stones on the cultivated grounds near farms.

**Grass, plants and stones**

- Size
The size of grass is determined by a minimum and a maximum value for height and width. 

- Density
Thick and thin values (separated for grass, water plants, desert plants and stones) affect
the number of objects per terrain tile. Higher or lower numbers determine the density, while
the delta between the min and max affect the homogeneity or variety among all tiles.

- NoiseSpread
The spread of the variety of grass, an higher number causes a more varied placement.
It doesn't affect the number of objects.

**Terrain**

- DetailDistance
The distance from camera beyond which the grass will be culled.
Lower values can increase performance.

- DetailDensity
1.0 is the original density, as decided by Thick and Thin. 
Lower values result in less detail objects being rendered, increasing performance.

**Wind**

- WavingAmount, WavingSpeed and WavingStrength. 
Affect how much the wind moves the grass.
All three accept values in a range between 0.0 (no wind) and 1.0.

**PRESETS**
-------------------------------------------------------------------------------------------------
Settings presets are ini files which contain a group of the settings descripted above.
Such ini file should have this section as an header:

[Internal]
PresetName = MyPreset
PresetAuthor = MyName
Description = Short Description.
SettingsVersion = 1.1

Settings version is not the version of the preset but the version of this mod settings
the preset is made for.

Then you can have your settings as they are in realgrass.ini. 
You don't need to include all sections or keys.

When you created your preset, call it 'realgrasspreset*.ini' (use any number or name instead of *)
and place it in the same folder with realgrass.ini.

**RESOURCES FOLDER**
-------------------------------------------------------------------------------------------------
It's possible to create a subfolder called 'Resources' to further customize the grass
and import custom textures. For example:

- Mods/RealGrass/realgrass.dfmod
- Mods/RealGrass/Resources

These are the names for the textures:
- tex_BrownGrass.png
- tex_GreenGrass.png
- tex_DesertGrass.png
- tex_MountainGrass.png
- tex_MountainGrassWinter.png
- tex_stone.png
- tex_SwampGrass.png
- tex_SwampGrassWinter.png
- tex_TemperateGrass.png
- tex_TemperateGrassWinter.png
- tex_waterlilies.png
- tex_WaterMountainGrass.png

**UNINSTALL**
-------------------------------------------------------------------------------------------------
If you wish to uninstall, you can safely remove the mod from the StreamingAssets folder. 
It won't affect saves.

**CREDITS**
-------------------------------------------------------------------------------------------------
Main developer: Uncanny_Valley
Additional changes: midopa
GrassAndPlants and conversion to mod-system: TheLacus

Real Grass on Daggerfall Unity forums: http://forums.dfworkshop.net/viewtopic.php?f=14&t=17

**COMPATIBILITY**
-------------------------------------------------------------------------------------------------
This mod is compatible with Nystul Distant Terrain. 
Be careful when installing other mods that affects terrain.

**CHANGELOG**
-------------------------------------------------------------------------------------------------
1.09
* Added presets.
* Customize textures from disk.

1.08
* Real Grass and Grass&Plants are now packed in the same mod for increased ease of use and maintenance.
* (optional) Added winter version of water plants.
* Customize size and shape of grass from the settings window.
* Minor improvements.

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