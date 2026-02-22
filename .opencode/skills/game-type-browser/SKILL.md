---
name: game-type-browser
description: 浏览和提取 Kingdom Two Crowns 游戏程序集的类型信息。用于搜索游戏类型（包括游戏基于 Unity 引擎实现的组件类）、提取类成员信息（方法、属性、字段）、查看方法实现代码。当 用户或 AI Agent 需要了解游戏 API、查找特定游戏类或查看游戏代码实现时使用此技能。触发场景包括："查找游戏中的某类型信息"、"提取某类的方法"、"查看某方法的实现"等。
---

# Game Type Browser

用于浏览和提取 Kingdom Two Crowns 游戏程序集的类型信息。

## 功能概述

1. **搜索类型**: 在游戏程序集中搜索特定类型或命名空间
2. **提取成员**: 获取类型的成员信息（方法、属性、字段、事件等）
3. **查看实现**: 反编译并查看方法的实现代码

## 核心工具

所有功能通过 `ilspycmd` 工具实现，该工具已在环境中安装。

## 使用流程

### 1. 搜索类型

**场景**: 用户询问"查找所有与地图相关的类"或"找到 PlayerController 类"

**步骤**:
1. 确定使用哪个程序集（IL2CPP 或 Mono）- 参见 [references/assemblies.md](references/assemblies.md)
2. 运行搜索脚本:

```bash
python scripts/search_types.py <assembly-path> "<search-pattern>"
```

**示例**:
```bash
# 搜索包含 "Map" 的所有类型
python scripts/search_types.py "D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_Mono\Managed\Assembly-CSharp-publicized.dll" "Map"

# 搜索以 "Controller" 结尾的类型
python scripts/search_types.py "D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_IL2CPP\interop\Assembly-CSharp.dll" ".*Controller$"
```

**输出**: Markdown 格式的搜索结果列表

### 2. 提取类型成员

**场景**: 用户询问"提取 PlayerController 的所有方法"或"列出某个类的成员"

**步骤**:
1. 使用完全限定的类型名称（如果搜索结果提供了命名空间）
2. 运行成员提取脚本:

```bash
python scripts/get_type_members.py <assembly-path> "<fully-qualified-type-name>"
```

**示例**:
```bash
# 提取 PlayerController 类的成员
python scripts/get_type_members.py "D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_Mono\Managed\Assembly-CSharp-publicized.dll" "PlayerController"

# 如果类型在命名空间中
python scripts/get_type_members.py "D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_IL2CPP\interop\Assembly-CSharp.dll" "Game.World.MapController"
```

**输出**: Markdown 格式的成员列表，按类别分组（构造函数、字段、属性、方法、事件）

### 3. 查看方法实现

**场景**: 用户询问"查看 PlayerController.Attack 方法的实现"或"这个方法是如何工作的"

**步骤**:
1. 运行实现代码提取脚本（提取整个类型的代码）:

```bash
python scripts/get_method_implementation.py <assembly-path> "<fully-qualified-type-name>"
```

2. 从输出的完整类代码中定位特定方法

**示例**:
```bash
# 获取 PlayerController 类的完整实现
python scripts/get_method_implementation.py "D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_Mono\Managed\Assembly-CSharp-publicized.dll" "PlayerController"
```

**输出**: Markdown 格式的反编译 C# 代码

**注意**: 
- 该脚本返回整个类的代码
- AI Agent 需要从输出中查找和提取特定方法
- 反编译的代码可能包含编译器生成的代码

## 程序集选择指南

详见 [references/assemblies.md](references/assemblies.md)

**快速选择**:
- **IL2CPP 模组开发**: 使用 `_libs\BIE6_IL2CPP\interop\Assembly-CSharp.dll`
- **Mono 模组开发或完整 API 探索**: 使用 `_libs\BIE6_Mono\Managed\Assembly-CSharp-publicized.dll`

## 常见工作流

### 工作流 1: 探索未知 API

1. 使用模糊搜索找到相关类型
2. 提取感兴趣的类型成员
3. 查看关键方法的实现

### 工作流 2: 查找特定功能

1. 基于功能关键词搜索（如 "Save", "Load", "Spawn" 等）
2. 缩小到最相关的类型
3. 提取成员以确认功能

### 工作流 3: 理解现有代码

1. 从 KingdomMod 代码中识别使用的游戏类型
2. 提取该类型的成员，了解可用 API
3. 查看实现以理解行为

## 脚本参考

### `scripts/search_types.py`
搜索程序集中匹配模式的类型。

**参数**:
- `<assembly-path>`: 程序集 DLL 文件的路径
- `<search-pattern>`: 正则表达式搜索模式（不区分大小写）

### `scripts/get_type_members.py`
提取类型的成员信息（签名，不含实现）。

**参数**:
- `<assembly-path>`: 程序集 DLL 文件的路径
- `<type-name>`: 完全限定的类型名称

### `scripts/get_method_implementation.py`
获取类型的完整反编译代码（包含方法实现）。

**参数**:
- `<assembly-path>`: 程序集 DLL 文件的路径
- `<type-name>`: 完全限定的类型名称

## 限制与注意事项

1. **编码问题**: Windows 环境中可能遇到控制台编码问题，脚本已配置 UTF-8
2. **类型名称**: 提取成员时必须使用完全限定的类型名称（包含命名空间）
3. **反编译质量**: IL2CPP 版本的反编译可能不如 Mono 版本完整
4. **性能**: 大型程序集的搜索可能需要几秒钟

## 故障排除

### 问题: "错误: ilspycmd 执行失败"
- 验证 ilspycmd 是否正确安装: `ilspycmd --version`
- 确认程序集路径正确且文件存在

### 问题: "未找到匹配的类型"
- 检查搜索模式是否正确
- 尝试更宽泛的搜索模式
- 确认使用了正确的程序集文件

### 问题: 类型名称错误
- 使用搜索功能获取完全限定的类型名称
- 确保包含命名空间（如果有）
