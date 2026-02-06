# Kingdom Two Crowns 游戏程序集引用

## 程序集位置

### IL2CPP 版本
```
D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_IL2CPP\interop\Assembly-CSharp.dll
```

### Mono 版本
```
D:\workspace\games\KingdomTwoCrowns\KingdomMapModDev\_libs\BIE6_Mono\Managed\Assembly-CSharp-publicized.dll
```

## 常用命名空间

根据 KingdomMod 项目的代码模式，以下是常见的游戏命名空间：

- **核心游戏逻辑**: 通常在根命名空间或全局命名空间
- **UI 系统**: 可能包含 `UI`, `GUI`, `Menu` 等
- **地图系统**: 可能包含 `Map`, `World`, `Level` 等
- **玩家系统**: 可能包含 `Player`, `Character`, `Controller` 等
- **建筑系统**: 可能包含 `Building`, `Structure`, `Construction` 等

## 选择程序集版本的建议

- **开发 IL2CPP 模组**: 使用 IL2CPP 版本的 DLL
- **开发 Mono 模组**: 使用 Mono 版本的 DLL（publicized 版本暴露了内部成员）
- **查看完整 API**: Mono 的 publicized 版本通常包含更多可访问的成员

## 搜索技巧

### 模糊搜索
使用正则表达式进行模糊搜索：
- `"Map"` - 包含 "Map" 的所有类型
- `".*Controller$"` - 以 "Controller" 结尾的类型
- `"^Player"` - 以 "Player" 开头的类型
- `"UI.*Button"` - UI 相关的 Button 类型

### 完全限定名称
提取成员时需要使用完全限定的类型名称：
- `Namespace.ClassName` 而非 `ClassName`
- 示例: `Game.PlayerController` 而非 `PlayerController`
