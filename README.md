# PlayerScaling
This mod was created to allow for the map to be expanded to accomodate larger groups, and also has options for making the 

## Config
<details>
  <summary>Click me to expand</summary>

### Global Scaling Multiplier
Multiplies the player scaling factor by this number.
### Player Scaling Minimum
The player scaling factor is set to 1 unless there are at least this many players, or downscaling is enabled.
### Player Scaling Divisor
The player scaling factor will go up by 1 each time the number of players goes up by this amount. Decrease for more player scaling, increase for less.
### Down Scaling Enabled
Disabled by default, enable if you want the map to get smaller for fewer players.
### Minimum Downscaling Multiplier
The lowest the multiplier will go for downscaling, default 0.5x. I don't recommend putting this below 0.5 as it's untested and likely to break the game.
### Map Scaling Enabled
If this is disabled, the map size will not be affected by player count.
### Map Scaling Multiplier
Multiplies the map size by this amount. Applies regardless of whether or not the Map Scaling is enabled.
### Enemies Scaling Multiplier
Changes how quickly enemies increase in number. Higher means they will grow in numbers more quickly, but will not change their maximum amount.
### Max Enemy Density
This affects the maximum number of total enemies you will see per map module (the big squares on the map). Default is in line with vanilla. Set higher to have the enemy count continue scaling past round 11.
### Valuables Scaling Multiplier
Multiplies the amount of valuables by this amount.
### Difficulty Scaling Enabled
If this is disabled, difficulty will not change with player count.
### Difficulty Scaling Multiplier
Multiplies the difficulty which mostly affects room types and enemy spawning delay. Applies regardless of whether or not Difficulty scaling is enabled.
### Difficulty Scaling Offset
Adds to the difficulty to have it start slightly higher.
</details>

## Map Size
The map size scales linearly with players and will break past the normal limits. You will also see more than 4 extracts on these larger maps.

## Enemy Count and Valuables
Enemies and Valuables are multiplied in accordance with the map size changes, so you can expect to see a similar density of enemies and valuables as with a normal map, but there will be more overall.

## General Difficulty Scaling
The Game's general difficulty scaling will be accelerated for larger groups, but more subtly than other factors. Specifically the difficulty is multiplied by the square root of the player scaling factor.

## Other recommended mods
I highly recommend all of these mods for large groups for a better experience! <br>
[UpgradeEveryRound](https://thunderstore.io/c/repo/p/Redfops/UpgradeEveryRound/) is another mod by me that allows each player to get one free upgrade every time you visit the shop. <br>
[RoboUnion](https://thunderstore.io/c/repo/p/linkoid/RoboUnion/) is a mod by Linkoid that allows you to break past the normal cap of 6 players. <br>
[MoreShopItems](https://thunderstore.io/c/repo/p/GalaxyMods/MoreShopItems/) is a mod by GalaxyMods that increases the number of available items in the shop.