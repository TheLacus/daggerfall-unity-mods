=================================================================================================
▄▄▄▄ ▄███▄   ██   █           ▄▀  █▄▄▄▄ ██      ▄▄▄▄▄    ▄▄▄▄▄   
█  ▄▀ █▀   ▀  █ █  █         ▄▀    █  ▄▀ █ █    █     ▀▄ █     ▀▄ 
█▀▀▌  ██▄▄    █▄▄█ █         █ ▀▄  █▀▀▌  █▄▄█ ▄  ▀▀▀▀▄ ▄  ▀▀▀▀▄   
█  █  █▄   ▄▀ █  █ ███▄      █   █ █  █  █  █  ▀▄▄▄▄▀   ▀▄▄▄▄▀    
  █   ▀███▀      █     ▀      ███    █      █                     
 ▀              █                   ▀      █                      
               ▀                          ▀                      
2.9 for DaggerFall Unity 0.11.2
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
https://github.com/TheLacus/daggerfall-unity-realgrass/tree/master/RealGrassAssets/Textures

Use an appropriate resolution for desired performance requirements.

Uninstall
-------------------------------------------------------------------------------------------------
If you wish to uninstall, you can safely remove the mod from the StreamingAssets folder. 
It won't affect saves.

Mod Messages
-------------------------------------------------------------------------------------------------
The following mod messages can be used for communication  with other mods:

- message: "toggle", data: bool, callBack: null
    Toggles mod. When disabled, grass is removed from all terrains; when enabled,
    is added again starting from player terrain.

Credits
-------------------------------------------------------------------------------------------------
- Uncanny_Valley, creator of original mod inclued with early versions of DaggerFall Unity.
- TheLacus, current developer and maintainer of the standalone mod release.
- Midopa, contributions to original core mod.
- VMblast, author of textures 'Grass_tex.psd' and all 'GrassDetails_*.psd'; 'DesertGrass.psd' is an edited version
  of 'Grass.psd'. Use of these textures is authorized for this project only [RealGrass for Daggerfall Unity].

Some meshes and/or textures have been picked from the following free licensed packs:
- 60 CC0 Vegetation textures by rubberduck (License: CC0) at https://opengameart.org/content/60-cc0-vegetation-textures
- Butterfly (animated) by CDmir (License: CC0) at https://opengameart.org/content/butterfly-animated
- Free 3D plants models by yughues (License: CC0) at https://opengameart.org/content/free-3d-plants-models
- Free Handpainted Plants by yughues (License: CC0) at https://opengameart.org/content/free-handpainted-plants
- Free houseplants by yughues (License: CC0) at https://opengameart.org/content/free-houseplants
