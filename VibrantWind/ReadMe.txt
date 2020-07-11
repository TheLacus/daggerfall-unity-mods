=================================================================================================
 __     ___ _                     _    __        ___           _ 
 \ \   / (_) |__  _ __ __ _ _ __ | |_  \ \      / (_)_ __   __| |
  \ \ / /| | '_ \| '__/ _` | '_ \| __|  \ \ /\ / /| | '_ \ / _` |
   \ V / | | |_) | | | (_| | | | | |_    \ V  V / | | | | | (_| |
    \_/  |_|_.__/|_|  \__,_|_| |_|\__|    \_/\_/  |_|_| |_|\__,_|
                                                                 
v0.5 for DaggerFall Unity 0.5.33
=================================================================================================

Description 
-----------
This mod changes strength of wind according to current weather.

Purpose is to add more realism to Daggerfall weather system, bending grass,
tree leaves and particle systems (chimney smog on buildings etc.) with a
different intensity for different conditions.

Installation
------------
Move vibrantwind.dfmod inside 'StreamingAssets/Mods'.

Settings
--------
- Speed
The speed (or strength) of the wind as it blows grass.
The higher this is, the faster the grass will move back and forth.

- Bending
The degree to which grass objects are bent over by the wind.
Excessively high values deform the grass mesh, while too low values make it looks static.

- Size
The size of the 'ripples' on grassy areas. An higher value means a bigger zone 
affected by the wind at the same time.

- Force
The force (or intensity) of the wind as it blows trees and particles.

Settings keys
-------------
- Range
Min: The strength of the wind when there is light/no wind. Must be bigger than zero and lower than max.
Max: The highest strength of the wind, used during strong storms. Should be lower than one.

- Interpolation
Affects how the values for each weather are chosen.
* 0: Lerp
A simple linear interpolation; the difference between each value is the same.
* 1: Sinerp
Eases out near 1 using sine. The variations among pleasant weathers is bigger than the non-pleasants.
* 2: Coserp
Eases in near 0 using cosine. The variations among non-pleasant weathers is bigger than the pleasants.
* 3: SmoothStep
The interpolation will gradually speed up from the start and slow down toward the end; 
middle values have the biggest variations while the start and the end are 'smoothed'.

Console Commands
----------------
- set_weather {index}
Changes weather with index from pleasants to non pleasants (0: sunny, 6: snow).

- vwind_toggle
Disable mod. When the game loads new terrains, they will use a default of 0.5 for all values.
Use the same command to re-enable it; new values are applied immediately.

- vwind_debug {mode}
0: Get current wind strength.
1: Set weather for wind strength; does not affet game weather.
2: Test all weathers in succession and print all values to log.

Compatibility
-------------
Vibrant Wind show its best with mods that add terrain details (grass)
and wind-supported models for the Trees system.
Works great with RealGrass by Uncanny_Valley and TheLacus.

Particle systems must include "External Forces" module to support
wind (and be affected by this mod).

It's probably not compatible with other mods that affect TerrainData waving
settings or WindZone settings.

Changelog
---------
* Unreleased
- Compatibility upgrade.

* 0.5
- Compatibility upgrade.

* 0.4
- Support for WindZone (trees, particles).
- Reorganization of console commands.

* 0.3
- Set speed, bending and size separately.

* 0.2
- Console commands.

* 0.1
- First release.

Credits
-------
TheLacus (TheLacus@yandex.com)