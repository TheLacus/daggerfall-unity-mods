=================================================================================================
▄▄▄▄ ▄███▄   ██   █           ▄▀  █▄▄▄▄ ██      ▄▄▄▄▄    ▄▄▄▄▄   
█  ▄▀ █▀   ▀  █ █  █         ▄▀    █  ▄▀ █ █    █     ▀▄ █     ▀▄ 
█▀▀▌  ██▄▄    █▄▄█ █         █ ▀▄  █▀▀▌  █▄▄█ ▄  ▀▀▀▀▄ ▄  ▀▀▀▀▄   
█  █  █▄   ▄▀ █  █ ███▄      █   █ █  █  █  █  ▀▄▄▄▄▀   ▀▄▄▄▄▀    
  █   ▀███▀      █     ▀      ███    █      █                     
 ▀              █                   ▀      █                      
               ▀                          ▀                      
2.4 for DaggerFall Unity 0.7.x
=================================================================================================

Description
-------------------------------------------------------------------------------------------------
Adds grass and, optionally, water plants, stones and flowers to the terrain of Daggerfall Unity.

Installation
-------------------------------------------------------------------------------------------------
Put RealGrass.dfmod file inside 'DaggerfallUnity_Data/StreamingAssets/Mods'. 

Settings
-------------------------------------------------------------------------------------------------
* Grass

    - Size
    The size of grass is determined by a minimum and a maximum value for height and width.
    This is the size at the highest point: grass grows from snow during the first days
    of spring, and fade back to snow at the end of fall.

    - Density
    Thick and thin values affect the number of objects per terrain tile. 
    Higher or lower numbers determine the density, while the delta between the min and max
    affects the homogeneity or variety among all tiles. Excessive values can cause performance drops.

    - NoiseSpread
    The spread of the variety of grass, an higher number causes a more varied placement.
    It doesn't affect the number of objects.

    - Shader
    'Billboard' makes the grass images rotate so that they always face the camera; this is the cheapest option.
    When 'Mesh' is enabled, grass is shown in a more realistic manner, much like modern games.
    
    - Realistic
    Toggle between a simple grass texture for a classick look (great with ambient occlusion!) and somewhat
    more detailed textures as well as additional layers with flowers.

* WaterPlants

    Places vegetation near water areas, like lakes and river.
    There is a total of four different plants, for the same number of climate regions: mountain, temperate, 
    desert and swamp. They all have two variations, summer and winter. 
    You can choose to show the winter version during this season, or not to show them like the grass.
    
    Additionally it places waterlilies above the surface of temperate lakes and some tufts 
    of grass inside the mountain water zones.
    Plants bend in the wind and waterlilies move slightly on the water surface moved by the same wind.

* Others

    - Flowers   
    Flowers and bushes for different climates. Density varies randomically and, for little flowers,
    also with seasons (they grow up in spring and die in fall). Select a lower level of density
    to increase performance.
    
    - Stones 
    Places rocks on terrain as well as little stones on the cultivated grounds near farms.
    Select reduced density to improve performance.

* Advanced

    - DetailDistance
    The distance from camera beyond which the grass will be culled; this is the most expensive option.
    Lower values can increase performance while higher values reduce "popup".
    
    - DetailDensity
    Lower values result in less detail objects being rendered, increasing performance.

Texture Override
-------------------------------------------------------------------------------------------------
It's possible to import custom textures from StreamingAssets/Textures/Grass.

You can find textures names on github:
https://github.com/TheLacus/realgrass-du-mod/tree/master/Addons/RealGrass/Assets/Resources

Use an appropriate resolution for desired performance requirements.

How to Optimize
-------------------------------------------------------------------------------------------------
LOADING TIMES
* Disable additional details (flowers, stones etc.).

PERFORMANCE
* Reduce detail distance.
* Reduce grass/details density.
* Disable additional details (flowers, stones etc.).

MEMORY
* Disable additional details (flowers, stones etc.).
* Reduce terrain distance (game settings).

Uninstall
-------------------------------------------------------------------------------------------------
If you wish to uninstall, you can safely remove the mod from the StreamingAssets folder. 
It won't affect saves.

Compatibility
-------------------------------------------------------------------------------------------------
This mod is compatible with vegetation retextures as well as mods that replace existing vegetation
with 3d models, while it may be incompatible with mods that add new vegetation.
Be careful when installing other mods that affects terrain.

Known compatible mods:
* Distant Terrain, Nystul
* Vibrant Wind, TheLacus

Credits
-------------------------------------------------------------------------------------------------
* Contributors
    - Uncanny_Valley, creator of original mod inclued with early versions of DaggerFall Unity.
    - TheLacus, current developer and maintainer of the standalone mod release.
    - Midopa, contributions to original core mod.
    - VMblast, artist for textures 'Grass.psd' and 'GrassDetails_n.psd' (use authorized for this project only).
      'DesertGrass.psd' is an edited version of 'Grass.psd'.

* Third parties
Some meshes and/or textures have been picked from the following free licensed packs:

    - 60 CC0 Vegetation textures by rubberduck (License: CC0) at
        https://opengameart.org/content/60-cc0-vegetation-textures
    - Butterfly (animated) by CDmir (License: CC0) at
        https://opengameart.org/content/butterfly-animated
    - Free 3D plants models by yughues (License: CC0) at
        https://opengameart.org/content/free-3d-plants-models
    - Free Handpainted Plants by yughues (License: CC0) at
        https://opengameart.org/content/free-handpainted-plants
    - Free houseplants by yughues (License: CC0) at
        https://opengameart.org/content/free-houseplants
    - Plants textures Pack 01 by yughues (License: CC0) at
        https://opengameart.org/content/plants-textures-pack-01
    - Tropical plant 02 by yughues (License: CC0) at
        https://opengameart.org/content/tropical-plant-02-0
    - Tropical shrubs by yughues (License: CC0) at
        https://opengameart.org/content/tropical-shrubs

* Contacts
Daggerfall Unity forums: http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
Github: https://github.com/TheLacus/realgrass-du-mod

ChangeLog
-------------------------------------------------------------------------------------------------
Unreleased
* Added sparse grass and stones to desert locations.
* Improvements to terrain stones: winter version, new mesh and better disposition.
* Removed layer of flowers (deprecated by recent better additions) for lower memory usage.

2.4
* Improved proportions and scale for grass details when 'Realistic' option is enabled.
* Changes to featured grass plants with 'Realistic' option.
* Improvements to grass density and disposition.
* Merged rocks and stones layers, changed texture.
* Fixed exception thrown when inside a desert region if water plants are disabled.

2.3.1
* Fixed exception thrown in editor mode.
* Fixed exception thrown during winter if water plants are disabled.

2.3
* Make flying insects with particle systems at random spots on terrain.
* New grass textures by VMblast.
* Improvements to grass density and disposition.

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