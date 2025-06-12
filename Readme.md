# KingdomMod - Now support the real TWO crowns!

Some mods for the game "[Kingdom Two Crowns](https://store.steampowered.com/app/701160/)".

It's supported CO-OP and ONLINE modes now!

Both `IL2CPP` and `Mono` versions of the game are now supported!

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

1. First determine whether your game version is `IL2CPP`, or the`Mono` version.
    * `IL2CPP` version, the game directory contains the folder `KingdomTwoCrowns_Data\il2cpp_data`
    * `Mono` version, the game directory contains the folder `KingdomTwoCrowns_Data\Managed`
2. According to your game version, download the mod loader of a specific version. Unzip all files to the root directory of the game, and ensure that the `BepInEx` folder and `winhttp.dll` file are in the same directory with the game main program `KingdomTwoCrowns.exe`.
    * `IL2CPP` version, download [BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.735](https://builds.bepinex.dev/projects/bepinex_be/735/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.735%2B5fef357.zip)
    Due to incompatibility caused by game updates, for `IL2CPP` version you also need to download [Cpp2IL.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip), extract it to the game root directory, and overwrite the files with the same name in the BepInEx mod loader.
    * `Mono` version, download [BepInEx-Unity.Mono-win-x64-6.0.0-be.735](https://builds.bepinex.dev/projects/bepinex_be/735/BepInEx-Unity.Mono-win-x64-6.0.0-be.735%2B5fef357.zip)
3. Download the mod of a specific version according to your game version. Download the mod file from [Releases](https://github.com/abevol/KingdomMod/releases), unzip all files to the directory `Kingdom Two Crowns\BepInEx\plugins`.
    * `IL2CPP` version, download mods with `BIE6_IL2CPP` in the file name.
    * `Mono` version, download mods with `BIE6_Mono` in the file name.
4. Now mods are installed. After starting the game, mods can be automatically loaded.

## BetterPayableUpgrade

Better pay for upgrades. Adjusted the price of some payable upgrade objects in the game, the objects of the next level and the construction time to make them more reasonable and comfortable.

### Adjust details

* Reduced the size of the coin, now your coin bag can hold more coins (about 100).

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
* For balance reasons, all features are only available in `Debug` builds.

### Hotkeys

1. `Ctrl + D` Enable DevTools
2. `Home` Displays debug information
3. `End` Displays objects information
4. `Insert` Tests some interesting features
5. `X` Dumps game objects to JSON file
6. `P` Print presets to console
7. `L` Prints the level module to the console
8. `Delete` Cut down the currently selected tree
9. `F1` Add vagrant
10. `F2` Add Griffin
11. `F9` Activate the Game Developer Debug Toolkit
12. `F10` Set coin limit to 1000
13. `Space` Throws a boulder

## OverlayMap

A map mod. It marks points of interest, along with some extra details. There's also the demographics feature, which tells you how many of each type there are.

## Features

1. Add a map floating layer on the top of the screen, and marks points of interest in the game on it, including castles, slums, docks, cliffs, etc.
2. By default, only the markers in the area you have explored are displayed, of course you can also show the full map immediately by pressing the `Ctrl + F` key (Not recommended).
3. Displays some useful stats, including the number of idle villagers, workers, archers, farmers and farmland.
4. Display additional useful information, such as current time, current island, number of gems and coins, etc.
5. Some shortcut features, such as quick save or reload the game without exiting the game.
6. The progress of the exploring area now can be saved with the saving archive of the game.

## Hotkeys

1. `M` Display the map overlay
2. `Ctrl + F` Show the full map (Not recommended, it will reduce the fun of the game)
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

### Custom style and language

Map mods now support custom styles and languages, and you can find their configuration files in the game directory `Kingdom Two Crowns\BepInEx\config`.

* `KingdomMod.OverlayMap.cfg`, configure the current language and style file name,
* `KingdomMod.OverlayMap.Style.cfg`, the style file of the map mod. You can modify the text color and marker symbols in it.
* `KingdomMod.OverlayMap.Language.en-US.cfg`, the language file of the map mod. You can use this file as a template to make your native language files. Copy the file, change the `en-US` part of the file name to the language code of your own country, such as `KingdomMod.OverlayMap.Language.ru-RU.cfg`, and then localize the strings in the file translate.
* Welcome to share your language and style files: [Share Entry](https://github.com/abevol/KingdomMod/issues/3).

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
