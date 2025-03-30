# PlayerScaling
This mod changes the scaling of difficulty and various factors to create better gameplay for larger groups. Scaling begins after 4 players and in general you can expect a multiple every additional 4, so 1x scaling at 4 players, 2x at 8 players, 3x at 12, etc.

## Config
<details>
  <summary>Click me to expand</summary>

### Global Scaling Multiplier
Multiplies the player scaling factor by this number.
### Player Scaling Minimum
The player scaling factor is set to 1 unless there are at least this many players.
### Player Scaling Divisor
The player scaling factor will go up by 1 each time the number of players goes up by this amount.
### Map Scaling Enabled
If this is disabled, the map size will not be affected by player count.
### Map Scaling Multiplier
Multiplies the map size by this amount. Applies regardless of whether or not the Map Scaling is enabled.
### Enemies Scaling Enabled
If this is disabled, the number of enemies will not be affected by player count.
### Enemies Scaling Multiplier
Multiplies the number of enemies by this amount. Applies regardless of whether or not the Enemies Scaling is enabled.
### Valuables Scaling Enabled
If this is disabled, the amount of valuables will not be affected by player count.
### Valuables Scaling Multiplier
Multiplies the amount of valuables by this amount. Applies regardless of whether or not the Valuables Scaling is enabled.
### Valuables Scaling Offset
Increasing this value will result in slightly more and more difficulty valuables being spawned. Applies regardless of whether or not the Valuables Scaling is enabled.
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
[MorePlayers](https://thunderstore.io/c/repo/p/zelofi/MorePlayers/) is a mod by Zelofi that allows you to break past the normal cap of 6 players. <br>
[MoreShopItems](https://thunderstore.io/c/repo/p/GalaxyMods/MoreShopItems/) is a mod by GalaxyMods that increases the number of available items in the shop.