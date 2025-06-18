# ProduceMore

Tired of waiting 8 hours for lab ovens? Configure station speed, employee work animation speed, stack limits, and more.

This mod is for anyone who wants to accelerate their production. Or if you like a more leisurely playthrough, you can also slow things down.


## Features

Supports modifying the speed of every station in the game.

Supports modifying the batch size of:

 - Mixing station
 - Drying rack
 - Packaging station

Supports modifying the mixing station start threshold up to the batch limit.

Supports modifying stack size of every item in the game by category, with overrides for individual items. This includes cash!

Supports accelerated employee work animations. This means if you want to dry 1000 leaves, you don't have to wait 15+ minutes for your botanist to hang them up.

Supports accelerated employee walk speed.

Supports buying up to 999999 items at a time from shops and the delivery app.


## Configuration

The configuration file is ProduceMoreSettings.json, and is generated in Schedule I's UserData directory when the mod is run for the first time. It can easily be edited by hand with any text editor (eg, Notepad). The default mod configuration is to use the same stack sizes and station speeds as the unaltered game, with unaltered employee walk and animation speeds.

The mod should automatically detect old settings files and update them to the new format, with new fields being filled with default values. After running the updated mod for the first time, you may need to alter a few fields to keep your play experience the same.

Setting employee walk speed too high may result in pathfinding issues. If your employees have trouble navigating tight spaces, consider turning down the multiplier.

The employee work animation speed is determined by the corresponding station speed setting, but employee animation acceleration must be enabled first.


## Compatibility

This mod is known to be incompatible with BetterStacks and FasterCauldrons.


## Source

This mod is open-source and hosted on GitHub.