---
name: game-type-browser
description: 浏览 Kingdom Two Crowns 游戏程序集。搜索类型、提取成员、查看方法实现。触发："查找游戏类型"、"查找游戏组件"、"提取某类成员"、"查看方法实现"。
---

# Game Type Browser

浏览 Kingdom Two Crowns 游戏程序集的类型信息。

## 程序集路径

| 版本 | 路径 |
|------|------|
| IL2CPP | `_libs/BIE6_IL2CPP/interop/Assembly-CSharp.dll` |
| Mono | `_libs/BIE6_Mono/Managed/Assembly-CSharp-publicized.dll` |

**选择**: IL2CPP 模组用 IL2CPP 版本，Mono 模组或完整 API 探索用 Mono 版本（publicized 暴露内部成员）。

## 命令

### 搜索类型

```bash
python scripts/search_types.py <assembly-path> "<pattern>"
```

示例:
```bash
# 搜索包含 "Map" 的类型
python scripts/search_types.py "_libs/BIE6_Mono/Managed/Assembly-CSharp-publicized.dll" "Map"

# 正则搜索
python scripts/search_types.py "_libs/BIE6_IL2CPP/interop/Assembly-CSharp.dll" ".*Controller$"
```

### 提取成员

```bash
python scripts/get_type_members.py <assembly-path> "<type-name>"
```

**注意**: `<type-name>` 必须是完全限定名（含命名空间）。

示例:
```bash
python scripts/get_type_members.py "_libs/BIE6_Mono/Managed/Assembly-CSharp-publicized.dll" "PlayerController"
```

### 查看实现

```bash
python scripts/get_method_implementation.py <assembly-path> "<type-name>"
```

输出整个类型的反编译代码，从中定位特定方法。

## 故障排除

| 问题 | 解决 |
|------|------|
| ilspycmd 执行失败 | 验证安装: `ilspycmd --version` |
| 未找到类型 | 检查模式、尝试更宽泛搜索、确认程序集正确 |
| 类型名称错误 | 使用搜索获取完全限定名（含命名空间） |
