VIBRANT WIND

Version: 0.3

Description 
-----------
Change strength of wind according to current weather.

Prerequisites
-------------
Daggerfall Unity 0.4.36 or higher.
While there are no other technical requirements, this mod is 
supposed to be a complementary to a vegetation mod, such as RealGrass.

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
The force of the wind as it blows trees and particles.

Settings keys
-------------
- Range
Min: The strength of the wind when there is light/no wind. Must be bigger than zero and lower than max.
Max: The highest strength of the wind, used during strong storms. Should be lower than one.

- Interpolation
Affects how the values for each climate are chosen.
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
- set_weather {0-6}
Daggerfall Unity; Changes weather with index from pleasants to non pleasants (0: sunny, 6: snow).

- vwind_getstrength
Get current strength of wind.

- vwind_forceweather
Set a weather for this mod; does not affect game weather.

- vwind_toggle
Disable mod. When the game loads new terrains, they will use a default of 0.5 for all values.
Use the same command to re-enable it; changes are applied immediately.

Compatibility
-------------
This mod is compatible with every mod, including eventual weather mods, 
provided that it doesn't affect terrainData wind settings or WindZone.

Changelog
---------
* 0.4
- Support for WindZone (trees, particles).

* 0.3
- Set speed, bending and size separately.

* 0.2
- Console commands.

* 0.1
- First release.

Credits
-------
TheLacus (TheLacus@yandex.com)