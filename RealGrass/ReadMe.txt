=================================================================================================
REAL GRASS                     
                    
v2.10 for DaggerFall Unity 0.13.0
=================================================================================================

Description
-------------------------------------------------------------------------------------------------
A groundcover for Daggerfall Unity with grass, stones and other detail layers. 

Style: Classic style with textures from original version by Uncanny_Valley, Full style with
multiple layers made by VMblast or Mixed.

Shader: Billboard (always face the camera) or Mesh.

Size/Density: A min and max values that affect size of grass and the number of grass objects.

DetailDistance: The distance from camera beyond which the grass is culled.

DetailDensity: Lower values result in less detail objects being rendered, increasing performance.

Installation
-------------------------------------------------------------------------------------------------
Put RealGrass.dfmod file inside 'DaggerfallUnity_Data/StreamingAssets/Mods'. 

Texture Override
-------------------------------------------------------------------------------------------------
It's possible to import custom textures from StreamingAssets/Textures/Grass.

You can find textures names on github:
https://github.com/TheLacus/daggerfall-unity-realgrass/tree/master/RealGrassAssets/Textures

Uninstall
-------------------------------------------------------------------------------------------------
This mod doesn't affect save files, you can safely remove it from the StreamingAssets folder. 

Mod Messages
-------------------------------------------------------------------------------------------------
The following mod messages can be used for communication  with other mods:

- message: "toggle", data: bool, callBack: null
    Toggles mod. When disabled, grass is removed from all terrains; when enabled,
    is added again starting from player terrain.

Credits
-------------------------------------------------------------------------------------------------
- Uncanny_Valley, creator of original mod included with early versions of DaggerFall Unity.
- TheLacus, standalone mod release.
- Midopa, contributions to original core mod.
- VMblast, author of textures 'Grass_tex.psd' and all 'GrassDetails_*.psd'; 'DesertGrass.psd' is an edited version
  of 'Grass.psd'. Use of these textures is authorized for this project only [RealGrass for Daggerfall Unity].

Some meshes and/or textures have been picked from the following free licensed packs:
- 60 CC0 Vegetation textures by rubberduck (License: CC0) at https://opengameart.org/content/60-cc0-vegetation-textures
- Butterfly (animated) by CDmir (License: CC0) at https://opengameart.org/content/butterfly-animated
- Free 3D plants models by yughues (License: CC0) at https://opengameart.org/content/free-3d-plants-models
- Free Handpainted Plants by yughues (License: CC0) at https://opengameart.org/content/free-handpainted-plants
