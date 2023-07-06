# KingdomMod - Now support the real TWO crowns!

Some mods for the game "[Kingdom Two Crowns](https://store.steampowered.com/app/701160/)".

It's supported CO-OP and ONLINE modes now!

* [BetterPayableUpgrade](https://github.com/abevol/KingdomMod#betterpayableupgrade)
* [DevTools](https://github.com/abevol/KingdomMod#devtools)
* [OverlayMap](https://github.com/abevol/KingdomMod#overlaymap)
* [StaminaBar](https://github.com/abevol/KingdomMod#staminabar)

## Preview

![Preview](https://github.com/abevol/KingdomMod/blob/master/preview.png?raw=true)

## Supported languages

1. [中文](https://github.com/abevol/KingdomMod/blob/master/Readme.zh-CN.md)
2. [English](https://github.com/abevol/KingdomMod/blob/master/Readme.md)

## Install

1. Download [BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.670](https://builds.bepinex.dev/projects/bepinex_be/670/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.670%2B42a6727.zip)，unzip all the files to the root directory of the game, and ensure that the `BepInEx` folder and `winhttp.dll` file are in the same directory as the main game program `KingdomTwoCrowns.exe`.
2. Delete old version MOD. Check the game directory `Kingdom Two Crowns\BepInEx\plugins`, if there is an old version of the MOD, please delete it manually to avoid file conflicts.
3. Download the KingdomMod zip file from [Releases](https://github.com/abevol/KingdomMod/releases), and unzip all the files to the `Kingdom Two Crowns\BepInEx\plugins` directory.

## BetterPayableUpgrade

Better pay for upgrades. Adjusted the price of some payable upgrade objects in the game, the objects of the next level and the construction time to make them more reasonable.

### Adjust details

| Level | Name | Upgrade Price | Build Time |
|-----|------|---------|----------|
|0|monolith|3 => 2|-|
|1 |Rock Platform -| 6 => - | 30 => - |
|2 |Wooden Watchtower| 9 => 5 | 60 => 30|
|3 |Stone Tower| 12 => 8 | 90 => 50|
|4 |Triple Tower -| 15 => - | 120 => - |
|5 | triple tower with roof | 18 => 12 | 150 => 80 |
|6 |Four Towers| 18 => 16 | 180 => 120|
|- |Citizen Home Recruitment| 5 => 3 |- |
|- |Craft Oil Barrel| 5 => 3 |-|
|- |making bread| 4 => 2 |-|

* `-` means unavailable or removed.
* All modes and DLC are now supported.

## DevTools

Developer tools mod. Contains some functions that are convenient for making MODs.

### Details

* Some functions seriously affect the game balance, and it is strongly not recommended for ordinary players to use this mod.

### Hotkeys

1. `Home` Displays debug information
2. `End` Displays objects information
3. `Insert` Tests some interesting features
4. `X` Dumps game objects to JSON file
5. `P` Print presets to console
6. `L` Prints the level module to the console
7. `Delete` Deletes the current payable object
8. `F1` Add vagrant
9. `F2` Add Griffin
10. `F3` Add mound
11. `F4` Add boulder pile
12. `F9` Activate the Game Developer Debug Toolkit
13. `F10` Set coin limit to 1000
14. `R` Throws a boulder

## OverlayMap

A map mod. It marks points of interest, along with some extra details. There's also the demographics feature, which tells you how many of each type there are.

## Features

1. Add a map floating layer on the top of the screen, and marks points of interest in the game on it, including castles, slums, docks, cliffs, etc.
2. By default, only the markers in the area you have explored are displayed, of course you can also show the full map immediately by pressing the `F` key.
3. Displays some useful stats, including the number of idle villagers, workers, archers, farmers and farmland.
4. Display additional useful information, such as current time, current island, number of gems and coins, etc.
5. Some shortcut features, such as quick save or reload the game without exiting the game.

## Hotkeys

1. `M` Display the map overlay
2. `F` Show the full map
3. `F5` Reload saved game (without exiting game)
4. `F8` Save game (without exiting game)

## Signs and Colors

| Signs or Colors | Meanings |
|----|----|
|`♜`|Castle|
|`۩`|Wall|
|`∧`|Dirt mound|
|`≈`|River|
|`♣`|Berry Bush|
|$\color{red}{Red}$|Not unlocked, not captured, wrecked|
|$\color{blue}{Blue}$|Under construction|
|$\color{green}{Green}$|Unlocked, safe|

## StaminaBar

Stamina bar mod. It visually shows the stamina value of the mount by drawing a visual graph.

### Details

* The blue part represents stamina. Running will consume stamina, and walking and standing will restore stamina. Standing will restore stamina much faster than walking.
* The running consumption and running speed of different mounts are different, and this mod reflects them through the length of the stamina bar.
* The yellow part indicates the duration of the fully fed state, and actions in this state will not consume stamina.
* The duration of the fully fed state of different mounts is different, and this mod reflects it through the length of the yellow part.
* Even if you don't act, the duration of the fully fed state will gradually decrease over time.
* The red part indicates the duration of the fatigue state, in which you cannot run.
* The duration of the fatigue state of different mounts is different.

### Hotkeys

1. `N` show/hide stamina bar
