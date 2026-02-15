# PayableTeleporter地图标记实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 为游戏中的传送阵（PayableTeleporter）添加地图标记显示功能

**Architecture:** 采用现有的Resolver+Mapper架构，通过SimpleResolver识别组件类型，独立的PayableTeleporterMapper处理渲染逻辑

**Tech Stack:** C#, Unity, BepInEx, Harmony

**设计文档参考:** `docs/plans/2026-02-16-payableteleporter-marker-design.md`

---

## 前置检查

在开始实施前，请确认：
1. 当前位于dev分支
2. 工作目录干净（无未提交的修改）
3. 可以正常构建项目: `dotnet build -c Debug`

---

## Task 1: 添加MapMarkerType枚举值

**Files:**
- Modify: `OverlayMap/Gui/TopMap/MapMarkerType.cs`

**Step 1: 在传送点区域添加Teleporter枚举**

在`MapMarkerType.cs`的"传送点/特殊建筑"区域（约第60-63行之间）添加：

```csharp
/// <summary>传送阵</summary>
Teleporter,
```

**Step 2: 验证修改**

检查文件确保：
- 枚举值添加在合适的位置（与Portal、TeleporterExit一起）
- 包含XML文档注释
- 使用正确的缩进（4个空格）

**Step 3: 提交**

```bash
git add OverlayMap/Gui/TopMap/MapMarkerType.cs
git commit -m "feat: add Teleporter to MapMarkerType enum"
```

---

## Task 2: 添加PayableTeleporterResolver

**Files:**
- Modify: `OverlayMap/Gui/TopMap/Resolvers/SimpleResolvers.cs`

**Step 1: 在文件末尾添加Resolver类**

在`SimpleResolvers.cs`文件末尾（LighthouseResolver类之后）添加：

```csharp
/// <summary>传送阵解析器</summary>
public class PayableTeleporterResolver : SimpleResolver
{
    public PayableTeleporterResolver() : base(typeof(PayableTeleporter), MapMarkerType.Teleporter) { }
}
```

**Step 2: 验证修改**

- 类继承自`SimpleResolver`
- 目标类型为`typeof(PayableTeleporter)`
- 标记类型为`MapMarkerType.Teleporter`

**Step 3: 提交**

```bash
git add OverlayMap/Gui/TopMap/Resolvers/SimpleResolvers.cs
git commit -m "feat: add PayableTeleporterResolver"
```

---

## Task 3: 添加MarkerStyle配置

**Files:**
- Modify: `OverlayMap/Config/MarkerStyle.cs`

**Step 1: 添加静态字段**

在`MarkerStyle`类的静态字段区域（约第66行之后，Lighthouse字段附近）添加：

```csharp
public static MarkerConfig Teleporter;
```

**Step 2: 在ConfigBind中添加配置绑定**

在`ConfigBind`方法中（约第240行之后，HephaestusForge配置之后）添加：

```csharp
Teleporter.Color = config.Bind("Teleporter", "Color", "0.62,0,1,1", "");
Teleporter.Sign = config.Bind("Teleporter", "Sign", "⧉", "");
```

**Step 3: 验证修改**

- 字段声明位置合适
- ConfigBind中配置项格式正确
- 颜色值为"0.62,0,1,1"（紫色）
- 符号为"⧉"

**Step 4: 提交**

```bash
git add OverlayMap/Config/MarkerStyle.cs
git commit -m "feat: add Teleporter marker style configuration"
```

---

## Task 4: 添加Strings文本配置

**Files:**
- Modify: `OverlayMap/Config/Strings.cs`

**Step 1: 添加静态字段**

在`Strings`类的静态字段区域（约第134行之后）添加：

```csharp
public static ConfigEntryWrapper<string> Teleporter;
```

**Step 2: 在ConfigBind中添加配置绑定**

在`ConfigBind`方法中（约第262行之后，Wharf配置之后）添加：

```csharp
Teleporter = config.Bind("Strings", "Teleporter", "Teleporter", "");
```

**Step 3: 验证修改**

- 字段声明正确
- 配置绑定格式正确
- 默认值为"Teleporter"

**Step 4: 提交**

```bash
git add OverlayMap/Config/Strings.cs
git commit -m "feat: add Teleporter string configuration"
```

---

## Task 5: 创建PayableTeleporterMapper

**Files:**
- Create: `OverlayMap/Gui/TopMap/Mappers/PayableTeleporterMapper.cs`

**Step 1: 创建Mapper文件**

创建文件并添加以下内容：

```csharp
using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableTeleporterMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            view.TryAddMapMarker(
                component,
                MarkerStyle.Teleporter.Color,
                MarkerStyle.Teleporter.Sign,
                Strings.Teleporter
            );
        }

        [HarmonyPatch(typeof(PayableTeleporter), nameof(PayableTeleporter.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(PayableTeleporter __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(PayableTeleporter), nameof(PayableTeleporter.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(PayableTeleporter __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
```

**Step 2: 验证代码**

- 使用 Harmony 特性标记 Patch 类
- 正确处理 OnEnable/OnDisable 事件
- 调用 view.TryAddMapMarker 时参数正确

**Step 3: 提交**

```bash
git add OverlayMap/Gui/TopMap/Mappers/PayableTeleporterMapper.cs
git commit -m "feat: add PayableTeleporterMapper"
```

---

## Task 6: 在MapperInitializer中注册

**Files:**
- Modify: `OverlayMap/Gui/TopMap/MapperInitializer.cs`

**Step 1: 注册Resolver**

在`Initialize`方法的resolver注册区域（约第85行之后，MerchantSpawnerResolver之后）添加：

```csharp
RegisterResolver(resolvers, new Resolvers.PayableTeleporterResolver());
```

**Step 2: 注册Mapper**

在mappers字典初始化区域（约第157行之后，MerchantSpawner条目之后）添加：

```csharp
{ MapMarkerType.Teleporter, new Mappers.PayableTeleporterMapper(view) },
```

**Step 3: 验证修改**

- Resolver注册在合适的位置
- Mapper字典条目语法正确（注意逗号）
- 类型引用正确（Resolvers.PayableTeleporterResolver, Mappers.PayableTeleporterMapper）

**Step 4: 提交**

```bash
git add OverlayMap/Gui/TopMap/MapperInitializer.cs
git commit -m "feat: register PayableTeleporter resolver and mapper"
```

---

## Task 7: 构建验证

**Files:**
- 所有修改过的文件

**Step 1: 执行构建**

```bash
dotnet build -c Debug
```

**预期输出:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Step 2: 如果有错误，修复后重新构建**

常见错误：
- 缺少using语句
- 类型名称拼写错误
- 语法错误（缺少分号、括号不匹配等）

**Step 3: 提交修复（如有）**

```bash
git add .
git commit -m "fix: resolve build errors"
```

---

## Task 8: 功能验证

**Files:**
- 无需修改文件

**Step 1: 部署测试**

1. 构建成功后，DLL会自动复制到游戏插件目录（通过MSBuild配置）
2. 启动游戏
3. 进入包含传送阵的岛屿

**Step 2: 验证功能**

- [ ] 打开地图（按M键或对应快捷键）
- [ ] 传送阵在地图上显示为紫色 `⧉` 符号
- [ ] 鼠标悬停显示"Teleporter"提示
- [ ] 建造中的传送阵仍由脚手架标记显示

**Step 3: 问题排查**

如果标记未显示：
1. 检查BepInEx日志：`BepInEx/LogOutput.log`
2. 确认PayableTeleporterResolver被正确调用
3. 检查是否有其他错误阻止了标记创建

---

## 完成总结

实施完成后，代码库将包含：

1. ✅ MapMarkerType.Teleporter 枚举
2. ✅ PayableTeleporterResolver 识别组件
3. ✅ PayableTeleporterMapper 渲染标记
4. ✅ MarkerStyle.Teleporter 样式配置
5. ✅ Strings.Teleporter 文本配置
6. ✅ MapperInitializer 注册

所有更改遵循现有代码风格和架构模式，保持向后兼容。

---

## 附录：参考文件

- **设计文档**: `docs/plans/2026-02-16-payableteleporter-marker-design.md`
- **类似实现**: `PortalMapper.cs`, `TeleporterExitMapper.cs`
- **配置参考**: `MarkerStyle.cs`, `Strings.cs`
