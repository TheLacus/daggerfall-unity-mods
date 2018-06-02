=================================================================================================
▄▄▄▄ ▄███▄   ██   █           ▄▀  █▄▄▄▄ ██      ▄▄▄▄▄    ▄▄▄▄▄   
█  ▄▀ █▀   ▀  █ █  █         ▄▀    █  ▄▀ █ █    █     ▀▄ █     ▀▄ 
█▀▀▌  ██▄▄    █▄▄█ █         █ ▀▄  █▀▀▌  █▄▄█ ▄  ▀▀▀▀▄ ▄  ▀▀▀▀▄   
█  █  █▄   ▄▀ █  █ ███▄      █   █ █  █  █  █  ▀▄▄▄▄▀   ▀▄▄▄▄▀    
  █   ▀███▀      █     ▀      ███    █      █                     
 ▀              █                   ▀      █                      
               ▀                          ▀                      
v2.2 for DaggerFall Unity 0.4.75
=================================================================================================

**DESCRIPTION**
-------------------------------------------------------------------------------------------------
Adds grass and, optionally, water plants, stones and flowers to the terrain of Daggerfall Unity.

**INSTALLATION**
-------------------------------------------------------------------------------------------------
Put the *.dfmod file inside 'DaggerfallUnity_Data/StreamingAssets/Mods'. 
Use the load order settings window to choose if you want water plants and stones or only grass, 
and customize the size and shape of grass.

**SETTINGS**
-------------------------------------------------------------------------------------------------

**Grass**

- Size
The size of grass is determined by a minimum and a maximum value for height and width.
This is the size at the highest point: grass grows from snow during the first days
of spring, 
and fade back to snow at the end of fall.

- Density
Thick and thin values affect the number of objects per terrain tile. 
Higher or lower numbers determine the density, while the delta between the min and max
affects the homogeneity or variety among all tiles.
Excessive values can cause performance drops.

- NoiseSpread
The spread of the variety of grass, an higher number causes a more varied placement.
It doesn't affect the number of objects.

- Shader
'Billboard' makes the grass images rotate so that they always face the camera; this is the cheapest option.
When 'Mesh' is enabled, grass is shown in a more ralistic manner, much like modern games.

**WaterPlants**

Places vegetation near water areas, like lakes and river.
There is a total of four different plants, for the same number of climate regions: mountain, temperate, 
desert and swamp. They all have two variations, summer and winter. 
You can choose to show the winter version during this season, or not to show them like the grass.

Additionally it places waterlilies above the surface of temperate lakes and some tufts 
of grass inside the mountain water zones.
Plants bend in the wind and waterlilies move slightly on the water surface moved by the same wind.

**Others**

- Flowers

Flowers and bushes for different climates. Density varies randomically and, for little flowers,
also with seasons (they grow up in spring and die in fall). Select a lower level of density
to increase performance.

- Stones 
Places rocks on terrain as well as little stones on the cultivated grounds near farms.
Select reduced density to improve performance.

**Advanced**

- DetailDistance
The distance from camera beyond which the grass will be culled; this is the most expensive option.
Lower values can increase performance while higher values reduce "popup".

- DetailDensity
Lower values result in less detail objects being rendered, increasing performance.

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

Use an appropriate resolution for desired performance requirements.

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
* Developers
- Uncanny_Valley (creator)
- TheLacus

* Contributors
- Midopa

* Third parties
Some meshes and/or textures have been picked from the following free licensed packs:

	- 60 CC0 Vegetation textures by rubberduck (License: CC0) at
		https://opengameart.org/content/60-cc0-vegetation-textures
    - Butterfly (animated) by CDmir (License: CC0) at
        https://opengameart.org/content/butterfly-animated
	- Bushes by yughues (License: CC0) at
		https://opengameart.org/content/bushes
	- Fern v2.1 (double-sided) Pack by yughues (License: CC0) at
		https://opengameart.org/content/fern-v21-double-sided-pack
	- Free 3D plants models by yughues (License: CC0) at
		https://opengameart.org/content/free-3d-plants-models
	- Free Handpainted Plants by yughues (License: CC0) at
		https://opengameart.org/content/free-handpainted-plants
	- [Free] HandPainted Plants 2 by yughues (License: CC0) at
		https://opengameart.org/content/free-handpainted-plants-2
	- Free houseplants by yughues (License: CC0) at
		https://opengameart.org/content/free-houseplants
	- Low poly rocks by para (License: CC0) at
		https://opengameart.org/content/low-poly-rocks
	- Plants textures Pack 01 by yughues (License: CC0) at
		https://opengameart.org/content/plants-textures-pack-01
	- Rocks by yughues (License: CC0) at
		https://opengameart.org/content/rocks-0
	- Tropical plant 02 by yughues (License: CC0) at
		https://opengameart.org/content/tropical-plant-02-0
	- Tropical shrubs by yughues (License: CC0) at
		https://opengameart.org/content/tropical-shrubs
	- Weathered rock pack by para (License: CC0) at
		https://opengameart.org/content/weathered-rock-pack

* Contacts
Daggerfall Unity forums: http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
Github: https://github.com/TheLacus/realgrass-du-mod

**CHANGELOG**
-------------------------------------------------------------------------------------------------
Unreleased
* Make flying insects with particle systems at random spots on terrain.

2.2
* Graphical improvements.
* Smooth transition from/to winter.
* Compatibility upgrade.

2.1

* Seasonal changes (grass color and size).
* Graphical improvements for plants and flowers.
* Additional groundcover (bushes and stones).

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

1.07
* Converted to mod system.
Place realgrass.dfmod inside 'StreamingAssets/Mods'.
* Fixed bug which caused the wrong kind of grass to appear 
when loading a save made in a different climate.

1.06
* Properly clear grass when moving into an season/climate without grass.

1.05
* Adds more variety to grass patches
* Tweaks density of grass so patches near non-grass terrain look more natural (no "grass walls")
* Refactoring, cleanup, and comments

1.04
* Fixes error messages when traveling during winter in grassy climates

1.03
* Fixes a few misplaced grass placements
* Added a new type of grass!
* Improved the green grass texture (I'm no artist but I try)
* Cleaned up the code and refined it a bit further 

1.02
* Updated to work with version 1.3 of DaggerFall Tools For Unity
* You no longer need to edit existing DFTFU files to use Real Grass
* Code improvements, the loading time for the grass should be significant faster than before