# KingdomMapMod

A map mod for the game "[Kingdom Two Crowns](https://store.steampowered.com/app/701160/)". It marks points of interest, along with some extra details. There's also the demographics feature, which tells you how many of each type there are.

## Supported languages

1. [中文](https://github.com/abevol/KingdomMapMod/blob/master/Readme.zh-CN.md)
2. [English](https://github.com/abevol/KingdomMapMod/blob/master/Readme.md)

## Features

1. Add a map floating layer on the top of the screen, and marks points of interest in the game on it, including castles, slums, docks, cliffs, etc.
2. By default, only the markers in the area you have explored are displayed, of course you can also show the full map immediately by pressing the `F` key.
3. Displays some useful stats, including the number of idle villagers, workers, archers, farmers and farmland.
4. Display additional useful information, such as current time, current island, number of gems and coins, etc.
5. Some shortcut features, such as quick save or reload the game without exiting the game.
6. Some debugging features are convenient for making mod.

## Hotkeys

1. `Home` Display the map overlay
2. `End` Display debug information
3. `Insert` Test some interesting features
4. `F` Show the full map
5. `X` Dump game objects to JSON file
6. `P` Dump Prefabs to console
7. `F5` Reload saved game (without exiting game)
8. `F8` Save game (without exiting game)

## Signs and Colors

| Signs or Colors | Meanings |
|----|----|
|`♜`|Castle|
|`۩`|Wall|
|`∧`|Wall Foundation|
|`≈`|River|
|`♣`|Berry Bush|
|$\color{red}{Red}$|Not unlocked, not captured, wrecked|
|$\color{blue}{Blue}$|Building|
|$\color{green}{Green}$|Unlocked, safe|

## Install

1. Download [BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.668](https://builds.bepinex.dev/projects/bepinex_be/668/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.668%2B46e297f.zip)，unzip all the files to the root directory of the game, and ensure that the `BepInEx` folder and `winhttp.dll` file are in the same directory as the main game program `KingdomTwoCrowns.exe`.
2. Download the KingdomMapMod zip file from [Releases](https://github.com/abevol/KingdomMapMod/releases), and unzip all the files to the `Kingdom Two Crowns\BepInEx\plugins` directory.

## Preview

![Preview](https://github.com/abevol/KingdomMapMod/blob/master/preview.png?raw=true)
