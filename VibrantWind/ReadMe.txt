VIBRANT WIND

Version: 0.2

Description 
-----------
Change strength of wind according to current weather.

Prerequisites
-------------
Daggerfall Unity 0.4.12 or higher.
While there are no other technical requirements, this mod is 
supposed to be a complementary to a vegetation mod, such as RealGrass.

Installation
------------
Move vibrantwind.dfmod inside 'StreamingAssets/Mods'.

Settings
--------
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
- set_weather 0-6
Daggerfall Unity; Changes weather with index from pleasants to non pleasants (0: sunny, 6: snow).

- vwind_getstrength
Get current strength of wind.

- vwind_setstrength
Set a new value for the strength of wind. Changes are applied immediately.
Must be higher than zero, values higher than one look "funny".

Changelog
---------
* 0.2
- Console commands

* 0.1
- First release.

Credits
-------
TheLacus (TheLacus@yandex.com)