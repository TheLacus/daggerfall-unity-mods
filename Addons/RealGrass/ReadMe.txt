=================================================================================================
REAL GRASS

2.1 for DaggerFall Unity 0.4.36
=================================================================================================

**DESCRIPTION**
-------------------------------------------------------------------------------------------------
Adds grass and, optionally, water plants, stones and flowers to the terrain of Daggerfall Unity.

**REQUIREMENTS**
-------------------------------------------------------------------------------------------------
Daggerfall Unity 0.4.36

**INSTALLATION**
-------------------------------------------------------------------------------------------------
Put the *.dfmod file inside 'DaggerfallUnity_Data/StreamingAssets/Mods'. 
Use the load order settings window to choose if you want water plants and stones or only grass, 
and customize the size and shape of grass.

**CUSTOMIZATION**
-------------------------------------------------------------------------------------------------
It is possible to customize this mod from the mod settings in-game. 
Presets with default values are available, but it's possible to customize them individually.
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

- UseGrassShader
By default Real Grass uses the GrassBillboard render mode. This makes the grass images rotate
so that they always face the camera and it's the cheapest option.
When 'UseGrassShader' is enabled, the Grass render mode is used instead. 
This can help creating a more realistic grass, but it's up to personal preferences.

**Terrain**

- DetailDistance
The distance from camera beyond which the grass will be culled.
Lower values can increase performance.

- DetailDensity
1.0 is the original density, as decided by Thick and Thin. 
Lower values result in less detail objects being rendered, increasing performance.

**PRESETS**
-------------------------------------------------------------------------------------------------
Settings presets are ini files which contain a group of the settings descripted above.
Such ini file should have this section as an header:

[Internal]
PresetName = MyPreset
PresetAuthor = MyName
Description = Short Description.
SettingsVersion = 1.3

Settings version is not the version of the preset but the version of this mod settings
the preset is made for.

Then you can have your settings as they are in realgrass.ini. 
You don't need to include all sections or keys.

When you created your preset, call it 'realgrasspreset*.ini' (use any number or name instead of *)
and place it in the same folder with realgrass.ini.

**IMPORT TEXTURES**
-------------------------------------------------------------------------------------------------
It's possible to create a subfolder called 'Resources' to further customize the grass
and import custom textures. Inside this folder you can have as many subfolder as you want.
For example:

- Mods/RealGrass/realgrass.dfmod
- Mods/RealGrass/Resources/MyTexturePack

In settings, enable Textures Import and write the name of folder (ex: Pack = MyTexturePack).
You can find textures names on github:
https://github.com/TheLacus/realgrass-du-mod/tree/master/Addons/RealGrass/Assets/Resources

**FAQ**
-------------------------------------------------------------------------------------------------
- How can I improve performance?
* Reducing 'DetailDistance' is the single most impactful option on game performance.
* Disable 'UseGrassShader'
* Reduce density of grass through *Lower and *Higher and/or with the general setting 'DetailDensity'.
* If you imported custom textures, reduce their resolution.

**UNINSTALL**
-------------------------------------------------------------------------------------------------
If you wish to uninstall, you can safely remove the mod from the StreamingAssets folder. 
It won't affect saves.

**COMPATIBILITY**
-------------------------------------------------------------------------------------------------
This mod is compatible with vegetation retextures as well as mods that replace existing vegetation
with 3d models, while it may be incompatible with mods that add new vegetation.
Be careful when installing other mods that affects terrain.

Known compatible mods:
* Distant Terrain, Nystul
* Vibrant Wind, TheLacus

**CREDITS**
-------------------------------------------------------------------------------------------------
Uncanny_Valley - creator, original developer
TheLacus - developer, maintainer
Midopa - additional changes

Daggerfall Unity forums: http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
Github: https://github.com/TheLacus/realgrass-du-mod

Additional art:
    - Free Handpainted Plants by yughues (License: CC0) at
        https://opengameart.org/content/free-handpainted-plants

**CHANGELOG**
-------------------------------------------------------------------------------------------------
2.1
* Seasonal changes (grass color and size).
* Graphical improvements for plants and flowers.

2.0
* Support for flowers
* Settings presets are more user-friendly
* Texture import is more user-friendly

1.11
* Optimization of DetailPrototypes terrain setting.
* Graphic enhancements of water plants.
* Added console commands: realgrass_toggle, realgrass_distance.

1.10
* Support Grass shader in addition to GrassBillboard shader
* The max distance for detail render can be customized for different performance requirements
* Plants and grass position remains unchanged between sessions for the same terrain
* Experimental support for flowers

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