# KingdomMapMod

游戏《[王国：两位君主](https://store.steampowered.com/app/701160/)》的地图模组。它标记了兴趣点，以及一些额外的细节。还有人口统计功能，它可以告诉你每种类型的人口有多少。

## 支持的语言

1. [中文](https://github.com/abevol/KingdomMapMod/blob/master/Readme.zh-CN.md)
2. [English](https://github.com/abevol/KingdomMapMod/blob/master/Readme.md)

## 功能

1. 在屏幕上方添加一个地图浮层，并在上面标识游戏内的兴趣点，包括城堡、贫民窟、码头、悬崖等。
2. 默认情况下，只显示你已探索的区域内的标识，当然你也可以通过按下`F`键立即显示完整地图。
3. 显示一些有用的统计信息，包括闲置的村民、工人、弓箭手、农民和农田的数量。
4. 显示额外的有用信息，比如当前时间、当前小岛、宝石数和金币数等等。
5. 一些快捷功能，比如快速保存或重载游戏，而不用退出游戏。
6. 一些调试功能，方便制作插件。

## 热键

1. `Home` 显示地图浮层
2. `End` 显示调试信息
3. `Insert` 测试一些有趣的功能
4. `F` 显示完整地图
5. `X` 转储游戏对象到 JSON 文件
6. `P` 转储预设件到控制台
7. `F5` 重新加载已保存的游戏 (不用退出游戏): 体验电影蝴蝶效应的乐趣，直到有一天，你把F5错按成了F8(> <)
8. `F8` 保存游戏 (不用退出游戏)

## 符号与颜色

| 符号或颜色 | 涵义 |
|----|----|
|`♜`|城堡|
|`۩`|城墙|
|`∧`|地基|
|`≈`|河流|
|`♣`|浆果丛|
|$\color{red}{红色}$|未解锁，未攻陷，已损坏|
|$\color{blue}{蓝色}$|建造中|
|$\color{green}{绿色}$|已解锁，安全的|

## 安装

1. 下载 [BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.668](https://builds.bepinex.dev/projects/bepinex_be/668/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.668%2B46e297f.zip)，将所有文件解压至游戏根目录，确保 `BepInEx` 文件夹和 `winhttp.dll` 等文件与游戏主程序 `KingdomTwoCrowns.exe` 处在同一目录。
2. 从 [Releases](https://github.com/abevol/KingdomMapMod/releases) 下载 KingdomMapMod 压缩包文件，将所有文件解压至 `Kingdom Two Crowns\BepInEx\plugins` 目录。

## 预览

![预览](https://github.com/abevol/KingdomMapMod/blob/master/preview.png?raw=true)
