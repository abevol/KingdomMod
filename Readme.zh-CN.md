# KingdomMod - 现在支持真正的双冠了！

游戏《[王国：两位君主](https://store.steampowered.com/app/701160/)》的相关功能模组。

现已支持合作和在线模式！
现在同时支持`IL2CPP`和`Mono`版本的游戏了！

* 更好的支付升级 [BetterPayableUpgrade](https://github.com/abevol/KingdomMod#betterpayableupgrade)
* 开发者工具 [DevTools](https://github.com/abevol/KingdomMod#devtools)
* 活点地图 [OverlayMap](https://github.com/abevol/KingdomMod#overlaymap)
* 坐骑耐力条 [StaminaBar](https://github.com/abevol/KingdomMod#staminabar)

## 预览

![预览](https://github.com/abevol/KingdomMod/blob/main/preview.png?raw=true)

## 支持的语言

1. [中文](https://github.com/abevol/KingdomMod/blob/main/Readme.zh-CN.md)
2. [English](https://github.com/abevol/KingdomMod/blob/main/Readme.md)

## 安装

1. 首先确定你的游戏版本是`IL2CPP`版，还是`Mono`版。
   * `IL2CPP`版，游戏目录中含有文件夹`KingdomTwoCrowns_Data\il2cpp_data`
   * `Mono`版，游戏目录中含有文件夹`KingdomTwoCrowns_Data\Managed`
2. 根据您的游戏版本，下载特定版本的模组加载器。将所有文件解压至游戏根目录，确保 `BepInEx` 文件夹和 `winhttp.dll` 等文件与游戏主程序 `KingdomTwoCrowns.exe` 处在同一目录。
   * `IL2CPP`版，下载 [BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip)
   * 由于游戏更新导致的不兼容，`IL2CPP`版还需下载 [Cpp2IL.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip) 和 [Il2CppInterop.Patch](https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip)，解压至游戏根目录，覆盖 BepInEx 模组加载器的同名文件。
   * `Mono`版，下载 [BepInEx-Unity.Mono-win-x64-6.0.0-be.753](https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip)
3. 根据您的游戏版本，下载特定版本的模组。从 [Releases](https://github.com/abevol/KingdomMod/releases) 下载模组文件，将所有文件解压至 `Kingdom Two Crowns\BepInEx\plugins` 目录。
   * `IL2CPP`版，下载文件名中包含`BIE6_IL2CPP`的模组。
   * `Mono`版，下载文件名中包含`BIE6_Mono`的模组。
4. 部分文件路径示例：
   * `X:\SteamLibrary\steamapps\common\Kingdom Two Crowns\KingdomTwoCrowns.exe`
   * `X:\SteamLibrary\steamapps\common\Kingdom Two Crowns\BepInEx\core\0Harmony.dll`
   * `X:\SteamLibrary\steamapps\common\Kingdom Two Crowns\BepInEx\plugins\KingdomMod.OverlayMap\KingdomMod.OverlayMap.dll`
5. 现在已完成模组的安装，启动游戏后即可自动加载模组。

## BetterPayableUpgrade

更好的支付升级。调整了游戏内部分可支付升级对象的价格和下一级的对象以及建造的时间，使其更加合理和舒适。

### 调整详情

* 缩小了硬币的体积，现在你的钱袋可以容纳更多硬币了（大约100）。

| 等级 | 名称 | 升级价格 | 建造时间 |
|-----|------|---------|----------|
|0    |巨石桩| 3 => 2 | - |
|1    |岩石平台 -| 6 => - | 30 => - |
|2    |木制瞭望塔| 9 => 5 | 60 => 30 |
|3    |石塔| 12 => 8 | 90 => 50 |
|4    |三重塔 -| 15 => - | 120 => - |
|5    |有屋顶的三重塔| 18 => 12 | 150 => 80 |
|6    |四重塔| 18 => 16 | 180 => 120 |
|-    |市民之家招募| 5 => 3 | - |
|-    |制作油桶| 5 => 3 | - |
|-    |制作面包| 4 => 2 | - |

* `-` 表示不可用或已移除。
* 现在已支持所有模式与DLC。

## DevTools

开发者工具模组。包含一些方便制作MOD的功能。

### 详情

* 部分功能严重影响游戏平衡，强烈不建议普通玩家使用该模组。
* 出于平衡性考虑，所有功能仅在`Debug`构建版本中可用。

### 热键

1. `Ctrl + D` 激活 DevTools
2. `Home` 显示调试信息
3. `End` 显示对象信息
4. `Insert` 测试一些有趣的功能
5. `X` 转储游戏对象到 JSON 文件
6. `P` 打印预设件到控制台
7. `L` 打印关卡模块到控制台
8. `Delete` 砍伐当前选中的树木
9. `F1` 添加游民
10. `F2` 添加格里芬
11. ~~`F9` 激活游戏开发者调试工具箱~~（已禁用）
12. ~~`F10` 将硬币上限设置为1000~~（已失效）
13. `Space` 扔出巨石

## OverlayMap

地图模组。它标记了兴趣点，以及一些额外的细节。还有人口统计功能，它可以告诉你每种类型的人口有多少。

### 功能

1. 在屏幕上方添加一个地图浮层，并在上面标识游戏内的兴趣点，包括城堡、贫民窟、码头、悬崖等。
2. 默认情况下，只显示你已探索的区域内的标识，当然你也可以通过按下`Ctrl + F`键立即显示完整地图（不建议）。
3. 显示一些有用的统计信息，包括闲置的村民、工人、弓箭手、农民和农田的数量。
4. 显示额外的有用信息，比如当前时间、当前小岛、宝石数和金币数等等。
5. 一些快捷功能，比如快速保存或重载游戏，而不用退出游戏。
6. 现在已探索区域的进度可以随游戏存档保存了。

### 热键

1. `M` 显示地图浮层
2. `Ctrl + F` 显示完整地图（不建议使用，会降低游戏乐趣）
3. `F5` 重新加载已保存的游戏 (不用退出游戏): 体验电影蝴蝶效应的乐趣，直到有一天，你把F5错按成了F8(> <)
4. `F8` 保存游戏 (不用退出游戏)
5. `Ctrl + ← / →` 调整地图标识的偏移
6. `Ctrl + ↑ / ↓` 调整地图的缩放比例

### 符号与颜色

| 符号或颜色 | 涵义 |
|----|----|
|`♜`|城堡|
|`۩`|城墙|
|`∧`|土堆|
|`≈`|河流|
|`♣`|浆果丛|
|$\color{red}{红色}$|未解锁，未攻陷，已损坏|
|$\color{blue}{蓝色}$|建造中|
|$\color{green}{绿色}$|已解锁，安全的|

### 自定义风格和语言

现在地图模组已支持自定义风格和语言，您可以在游戏目录`Kingdom Two Crowns\BepInEx\config`找到它们的配置文件。

* `KingdomMod.OverlayMap.cfg`，配置当前使用的语言和风格文件名，
* `KingdomMod.OverlayMap.MarkerStyle.cfg`，地图模组的风格文件。您可以修改其中的文本颜色及标记符号。
* `KingdomMod.OverlayMap.Language.en-US.cfg`，地图模组的语言文件。您可以以该文件为模板制作您的母语语言文件。复制该文件，将文件名的`en-US`部分改为您自己国家/地区的语言代码，比如`KingdomMod.OverlayMap.Language.ru-RU.cfg`，然后对文件内的字符串进行本地化翻译。
* 欢迎分享您的语言和风格文件：[分享入口](https://github.com/abevol/KingdomMod/issues/3)。

## StaminaBar

耐力条模组。它通过绘制可视化的图形来直观展示坐骑的耐力值。

### 详情

* 蓝色部分表示耐力值，跑步会消耗耐力值，行走和站立会恢复耐力值，站立时恢复耐力值的速度比行走时快很多。
* 不同坐骑的跑步消耗和奔跑速度不同，该模组通过耐力条的长度来体现它们。
* 黄色部分表示完全吃饱状态的持续时间，在该状态下行动不会消耗耐力值。
* 不同坐骑的完全吃饱状态的持续时间不同，该模组通过黄色部分的长度来体现它。
* 即使不行动，完全吃饱状态的持续时间也会随着时间逐渐减少。
* 红色部分表示疲劳状态的持续时间，在该状态下不能跑步。
* 不同坐骑的疲劳状态的持续时间不同。

### 热键

1. `N` 显示/隐藏耐力条
