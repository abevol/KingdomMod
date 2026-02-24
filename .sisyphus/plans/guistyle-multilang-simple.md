# GuiStyle 多语言支持重构计划（简洁方案）

## TL;DR

> **核心目标**：重构 OverlayMap 模组的 GuiStyle 配置架构，使其支持英文、俄文、中文三种语言环境的自动切换。
> 
> **设计方案**：使用语言特定的配置文件（`GuiStyle.{lang}.cfg`），完全替代单一 GuiStyle.cfg。
> 
> **交付物**：
> - 3 个语言特定的 GuiStyle 配置文件
> - GuiStyle.cs 添加语言配置加载逻辑
> - Global.cs 的 OnLanguageChanged 触发配置重载
> 
> **预计工作量**：小规模重构
> **并行执行**：是 - 2 个阶段
> **关键路径**：配置文件 → 代码修改 → 测试验证

---

## Context

### 原始需求
目前 GuiStyle 的字体设置仅针对中文环境硬编码，需要重构以支持英文、俄文、中文三种预置语言的自动切换。要求优先使用游戏内置字体和系统预装字体。

### 用户偏好
- **架构选择**：使用 `GuiStyle.{lang}.cfg` 架构（如 `GuiStyle.en-US.cfg`）
- **理由**：最简洁，改动最小
- **支持语言**：英文 (en-US)、俄文 (ru-RU)、中文 (zh-CN)

### 当前架构分析

**当前字体配置**（GuiStyle.cfg）：
```ini
TopMap.Count.Font = arial.ttf
TopMap.Sign.Font = arial.ttf
TopMap.Sign.FallbackFonts = seguisym.ttf
TopMap.Title.Font = fonts/notosanssc-medium
StatsInfo.Text.Font = fonts/notosanssc-medium
ExtraInfo.Text.Font = fonts/notosanssc-medium
```

**问题**：
1. 字体配置是全局的，不随语言变化
2. `notosanssc-medium` 仅支持中文，不适合英文/俄文
3. 没有语言切换时的字体重载机制

### 游戏内置字体资源
```
fonts/kingdom              - 游戏主字体
fonts/kingdommenu          - 菜单字体
fonts/notonaskharabic-medium - 阿拉伯文
fonts/notosans-medium      - 通用拉丁文（支持英文、俄文）
fonts/notosansbengali-medium - 孟加拉文
fonts/notosansdevanagari-medium - 印地文
fonts/notosansjp-medium    - 日文
fonts/notosanskr-medium    - 韩文
fonts/notosanssc-medium    - 简体中文
fonts/notoserifhebrew-medium - 希伯来文
fonts/notoserifthai-medium - 泰文
```

---

## Work Objectives

### 核心目标
实现 GuiStyle 配置的语言感知能力，当用户切换语言时自动加载对应语言的合适字体配置。

### 具体交付物
1. **GuiStyle.en-US.cfg** - 英文字体配置
2. **GuiStyle.ru-RU.cfg** - 俄文字体配置
3. **GuiStyle.zh-CN.cfg** - 中文字体配置
4. **GuiStyle.cs** - 添加语言配置加载方法
5. **Global.cs** - 在 `OnLanguageChanged` 中触发配置重载

### 完成定义
- [ ] 切换语言后，GUI 字体自动更新为对应语言的合适字体
- [ ] 所有三种语言都有默认字体配置
- [ ] 优先使用游戏内置字体，系统字体作为备选
- [ ] 配置变更事件正确触发字体重载
- [ ] 现有功能不受影响（向后兼容）

### 必须实现
- 语言特定的 GuiStyle 配置文件（`GuiStyle.{lang}.cfg`）
- 字体配置的动态重载机制
- 每种语言的合理默认字体选择

### 禁止实现（防护栏）
- **不要**修改现有的语言文件（Language.*.cfg）
- **不要**改变字体渲染的核心逻辑（FontManager.CreateFontAsset 等）
- **不要**添加新的外部字体文件依赖
- **不要**保留旧的 GuiStyle.cfg（完全迁移到新架构）

---

## Verification Strategy

### 测试决策
- **自动化测试**：无（项目无单元测试基础设施）
- **验证方式**：Agent-Executed QA Scenarios（每个任务必须包含）

### QA 策略
每个任务必须包含 Agent-Executed QA 场景：
- **配置验证**：检查配置文件语法正确性
- **编译验证**：`dotnet build` 成功
- **功能验证**：语言切换时字体配置正确重载

---

## Execution Strategy

### 并行执行波浪

```
Wave 1（立即开始 - 配置文件）：
├── 任务 1：创建英文字体配置文件 (GuiStyle.en-US.cfg) [quick]
├── 任务 2：创建俄文字体配置文件 (GuiStyle.ru-RU.cfg) [quick]
└── 任务 3：创建中文字体配置文件 (GuiStyle.zh-CN.cfg) [quick]

Wave 2（Wave 1 之后 - 代码修改）：
├── 任务 4：修改 GuiStyle.cs 添加语言配置加载 [deep]
├── 任务 5：修改 Global.cs 添加配置重载触发 [unspecified-high]
└── 任务 6：可选 - 更新 FontManager 支持动态重载 [unspecified-high]

Wave 3（Wave 2 之后 - 集成验证）：
├── 任务 7：编译验证 [quick]
└── 任务 8：语言切换功能验证 [deep]

Wave FINAL（并行审查）：
├── F1：代码质量审查 [unspecified-high]
└── F2：配置合规性检查 [oracle]

关键路径：任务 1 → 任务 4 → 任务 5 → 任务 7 → 任务 8 → F1
并行加速：约 65% 快于顺序执行
最大并发：3（Wave 1）
```

### 依赖矩阵
- **1-3**: — — 4, 5
- **4**: 1-3 — 5, 7
- **5**: 4 — 7, 8
- **6**: 4 — 7
- **7**: 4-6 — 8, F1
- **8**: 7 — F1, F2

---

## TODOs

- [ ] 1. 创建英文字体配置文件 (GuiStyle.en-US.cfg)

  **做什么**：
  - 创建 `OverlayMap/ConfigPrefabs/GuiStyle.en-US.cfg`
  - 为英文环境配置合适的字体
  - 使用游戏内置字体优先：`fonts/notosans-medium`
  - 配置备用字体：系统字体 `arial.ttf`
  - 包含所有必需的 GuiStyle 配置项（背景、字体等）

  **禁止做**：
  - 不要使用中文专用字体（如 notosanssc-medium）
  - 不要添加外部字体文件引用
  - 不要遗漏配置项（必须包含完整的 GuiStyle 配置）

  **推荐 Agent Profile**：
  - **Category**: `quick`
  - **Skills**: `[]`
  - **理由**：简单的配置文件创建

  **并行化**：
  - **可并行运行**：YES
  - **并行组**：Wave 1（与任务 2-3）
  - **阻塞**：任务 4-5
  - **被阻塞于**：无

  **参考**：
  - `OverlayMap/ConfigPrefabs/GuiStyle.cfg` - 当前配置格式参考
  - `OverlayMap/Config/GuiStyle.cs` - 配置项定义
  - 游戏内置字体列表（计划文档上文）

  **验收标准**：
  - [ ] 配置文件语法正确（INI 格式）
  - [ ] 使用 `fonts/notosans-medium` 作为主字体
  - [ ] 配置 `arial.ttf` 作为备用字体
  - [ ] 包含所有必需的 GuiStyle 配置项
  - [ ] 文件命名符合规范

  **QA 场景**：

  ```
  场景：验证配置文件完整性
  工具：Read
  步骤：
    1. 读取 OverlayMap/ConfigPrefabs/GuiStyle.en-US.cfg
    2. 检查文件格式是否符合 INI 规范
    3. 检查是否包含所有必需的配置项（TopMap.*, StatsInfo.*, ExtraInfo.*）
    4. 检查字体路径是否正确
  预期结果：配置文件格式正确，配置项完整
  证据：.sisyphus/evidence/task-1-en-config.png
  ```

  **提交**：YES
  - 消息：`config(overlay): 添加英文字体配置`
  - 文件：`OverlayMap/ConfigPrefabs/GuiStyle.en-US.cfg`

---

- [ ] 2. 创建俄文字体配置文件 (GuiStyle.ru-RU.cfg)

  **做什么**：
  - 创建 `OverlayMap/ConfigPrefabs/GuiStyle.ru-RU.cfg`
  - 为俄文环境配置合适的字体
  - 使用游戏内置字体：`fonts/notosans-medium`（支持西里尔字母）
  - 配置备用字体
  - 包含所有必需的 GuiStyle 配置项

  **禁止做**：
  - 不要使用不支持西里尔字母的字体
  - 不要添加外部字体文件引用
  - 不要遗漏配置项

  **推荐 Agent Profile**：
  - **Category**: `quick`
  - **Skills**: `[]`
  - **理由**：简单的配置文件创建

  **并行化**：
  - **可并行运行**：YES
  - **并行组**：Wave 1（与任务 1, 3）
  - **阻塞**：任务 4-5
  - **被阻塞于**：无

  **参考**：
  - `OverlayMap/ConfigPrefabs/GuiStyle.cfg` - 配置文件格式参考
  - `OverlayMap/Config/GuiStyle.cs` - 配置项定义
  - `OverlayMap/ConfigPrefabs/Language.ru-RU.cfg` - 俄文语言文件参考

  **验收标准**：
  - [ ] 配置文件语法正确
  - [ ] 使用支持西里尔字母的字体
  - [ ] 包含所有必需的 GuiStyle 配置项
  - [ ] 文件命名符合规范

  **QA 场景**：

  ```
  场景：验证俄文配置文件
  工具：Read
  步骤：
    1. 读取 OverlayMap/ConfigPrefabs/GuiStyle.ru-RU.cfg
    2. 检查字体是否支持西里尔字母
    3. 检查文件格式和配置项完整性
  预期结果：配置文件格式正确，字体选择合理，配置完整
  证据：.sisyphus/evidence/task-2-ru-config.png
  ```

  **提交**：YES
  - 消息：`config(overlay): 添加俄文字体配置`
  - 文件：`OverlayMap/ConfigPrefabs/GuiStyle.ru-RU.cfg`

---

- [ ] 3. 创建中文字体配置文件 (GuiStyle.zh-CN.cfg)

  **做什么**：
  - 创建 `OverlayMap/ConfigPrefabs/GuiStyle.zh-CN.cfg`
  - 为中文环境配置合适的字体
  - 使用游戏内置字体：`fonts/notosanssc-medium`
  - 配置备用字体：`arial.ttf`, `seguisym.ttf`
  - 包含所有必需的 GuiStyle 配置项

  **禁止做**：
  - 不要使用不支持中文的字体
  - 不要添加外部字体文件引用
  - 不要遗漏配置项

  **推荐 Agent Profile**：
  - **Category**: `quick`
  - **Skills**: `[]`
  - **理由**：简单的配置文件创建

  **并行化**：
  - **可并行运行**：YES
  - **并行组**：Wave 1（与任务 1-2）
  - **阻塞**：任务 4-5
  - **被阻塞于**：无

  **参考**：
  - `OverlayMap/ConfigPrefabs/GuiStyle.cfg` - 当前配置参考
  - `OverlayMap/Config/GuiStyle.cs` - 配置项定义
  - `OverlayMap/ConfigPrefabs/Language.zh-CN.cfg` - 中文语言文件参考

  **验收标准**：
  - [ ] 配置文件语法正确
  - [ ] 使用 `fonts/notosanssc-medium` 作为主字体
  - [ ] 配置合理的备用字体
  - [ ] 包含所有必需的 GuiStyle 配置项
  - [ ] 文件命名符合规范

  **QA 场景**：

  ```
  场景：验证中文配置文件
  工具：Read
  步骤：
    1. 读取 OverlayMap/ConfigPrefabs/GuiStyle.zh-CN.cfg
    2. 检查字体是否支持中文
    3. 检查文件格式和配置项完整性
  预期结果：配置文件格式正确，字体选择合理，配置完整
  证据：.sisyphus/evidence/task-3-zh-config.png
  ```

  **提交**：YES
  - 消息：`config(overlay): 添加中文字体配置`
  - 文件：`OverlayMap/ConfigPrefabs/GuiStyle.zh-CN.cfg`

---

- [ ] 4. 修改 GuiStyle.cs 添加语言配置加载支持

  **做什么**：
  - 在 `GuiStyle` 类中添加语言配置加载方法 `LoadLanguageConfig(string languageCode)`
  - 实现从 `GuiStyle.{lang}.cfg` 加载配置的逻辑
  - 添加配置重载事件处理
  - 添加 `OnConfigFileChanged` 事件订阅/取消订阅
  - 保留现有的配置绑定逻辑（作为默认配置）

  **禁止做**：
  - 不要修改字体渲染逻辑（FontManager）
  - 不要破坏现有的配置绑定逻辑
  - 不要移除现有的配置项（保持向后兼容）

  **推荐 Agent Profile**：
  - **Category**: `deep`
  - **Skills**: `[]`
  - **理由**：需要理解配置系统和事件流

  **并行化**：
  - **可并行运行**：NO
  - **并行组**：Sequential
  - **阻塞**：任务 5, 7
  - **被阻塞于**：任务 1-3

  **参考**：
  - `OverlayMap/Config/GuiStyle.cs` - 当前实现
  - `OverlayMap/Config/Strings.cs` - 配置绑定模式参考
  - `OverlayMap/Config/Global.cs:OnLanguageChanged` - 语言切换模式参考

  **验收标准**：
  - [ ] 添加 `LoadLanguageConfig` 方法
  - [ ] 实现从 `GuiStyle.{lang}.cfg` 加载配置
  - [ ] 添加配置重载事件处理
  - [ ] 代码编译通过
  - [ ] 符合现有代码风格（命名、注释）

  **QA 场景**：

  ```
  场景：验证代码编译
  工具：Bash
  步骤：
    1. 运行 dotnet build -c Debug
    2. 检查编译输出无错误
    3. 检查 OverlayMap 项目构建成功
  预期结果：编译成功，无错误和警告
  证据：.sisyphus/evidence/task-4-build-log.txt
  ```

  ```
  场景：验证代码结构
  工具：Read
  步骤：
    1. 读取 OverlayMap/Config/GuiStyle.cs
    2. 检查是否添加了 LoadLanguageConfig 方法
    3. 检查事件处理逻辑是否完整
  预期结果：代码结构符合设计要求
  证据：.sisyphus/evidence/task-4-code-review.png
  ```

  **提交**：YES
  - 消息：`feat(overlay): 添加语言配置加载支持`
  - 文件：`OverlayMap/Config/GuiStyle.cs`
  - 预检查：`dotnet build -c Debug`

---

- [ ] 5. 修改 Global.cs 的 OnLanguageChanged 触发配置重载

  **做什么**：
  - 在 `Global.OnLanguageChanged()` 方法中添加 GuiStyle 配置重载调用
  - 调用 `GuiStyle.LoadLanguageConfig(languageCode)`
  - 添加日志记录配置加载过程
  - 确保配置重载顺序正确（先语言文件，后字体配置）

  **禁止做**：
  - 不要修改语言文件加载逻辑
  - 不要破坏现有的配置重载流程

  **推荐 Agent Profile**：
  - **Category**: `unspecified-high`
  - **Skills**: `[]`
  - **理由**：需要理解配置系统和事件流

  **并行化**：
  - **可并行运行**：NO
  - **并行组**：Sequential
  - **阻塞**：任务 7-8
  - **被阻塞于**：任务 4

  **参考**：
  - `OverlayMap/Config/Global.cs:71-119` - OnLanguageChanged 当前实现
  - `OverlayMap/Config/GuiStyle.cs` - 配置加载方法（任务 4 输出）
  - `OverlayMap/Config/Strings.cs` - 配置绑定模式

  **验收标准**：
  - [ ] `OnLanguageChanged` 调用 GuiStyle 配置重载
  - [ ] 正确加载对应语言的配置文件
  - [ ] 添加适当的日志输出
  - [ ] 代码编译通过

  **QA 场景**：

  ```
  场景：验证代码编译
  工具：Bash
  步骤：
    1. 运行 dotnet build -c Debug
    2. 检查编译输出无错误
  预期结果：编译成功
  证据：.sisyphus/evidence/task-5-build-log.txt
  ```

  ```
  场景：验证 OnLanguageChanged 逻辑
  工具：Read
  步骤：
    1. 读取 OverlayMap/Config/Global.cs
    2. 检查 OnLanguageChanged 方法是否调用配置重载
    3. 检查日志输出是否完整
  预期结果：方法逻辑正确
  证据：.sisyphus/evidence/task-5-code-review.png
  ```

  **提交**：YES
  - 消息：`feat(overlay): 语言切换时触发 GuiStyle 配置重载`
  - 文件：`OverlayMap/Config/Global.cs`
  - 预检查：`dotnet build -c Debug`

---

- [ ] 6. 更新 FontManager 支持动态重载（可选）

  **做什么**：
  - 评估是否需要修改 FontManager
  - 如果需要，添加字体缓存清理方法（用于重载）
  - 确保字体加载支持动态切换
  - 添加字体资源释放逻辑

  **禁止做**：
  - 不要破坏现有的字体创建逻辑
  - 不要修改字体渲染核心算法

  **推荐 Agent Profile**：
  - **Category**: `unspecified-high`
  - **Skills**: `[]`
  - **理由**：字体管理核心代码，需要谨慎修改

  **并行化**：
  - **可并行运行**：NO
  - **并行组**：Sequential
  - **阻塞**：任务 7
  - **被阻塞于**：任务 4

  **参考**：
  - `OverlayMap/Assets/FontManager.cs` - 当前实现
  - `OverlayMap/Assets/FontData.cs` - 字体数据结构

  **验收标准**：
  - [ ] 字体支持动态重载
  - [ ] 添加字体缓存清理方法
  - [ ] 代码编译通过
  - [ ] 不破坏现有功能

  **QA 场景**：

  ```
  场景：验证代码编译
  工具：Bash
  步骤：
    1. 运行 dotnet build -c Debug
    2. 检查编译输出无错误
  预期结果：编译成功
  证据：.sisyphus/evidence/task-6-build-log.txt
  ```

  **提交**：YES（如果有修改）
  - 消息：`feat(overlay): 支持字体动态重载`
  - 文件：`OverlayMap/Assets/FontManager.cs`
  - 预检查：`dotnet build -c Debug`

---

- [ ] 7. 编译验证

  **做什么**：
  - 运行完整的构建流程
  - 验证所有项目编译通过
  - 检查无编译错误和警告
  - 验证构建产物正确生成

  **禁止做**：
  - 不要跳过任何编译错误

  **推荐 Agent Profile**：
  - **Category**: `quick`
  - **Skills**: `[]`

  **并行化**：
  - **可并行运行**：NO
  - **并行组**：Sequential
  - **阻塞**：任务 8
  - **被阻塞于**：任务 4-6

  **参考**：
  - 项目 README.md - 构建命令参考

  **验收标准**：
  - [ ] `dotnet build -c Debug` 成功
  - [ ] `dotnet build -c BIE6_Mono` 成功
  - [ ] 无编译错误
  - [ ] 无严重警告

  **QA 场景**：

  ```
  场景：Debug 构建验证
  工具：Bash
  步骤：
    1. 运行 dotnet build -c Debug
    2. 检查构建输出
    3. 验证 DLL 文件生成
  预期结果：构建成功，产物完整
  证据：.sisyphus/evidence/task-7-debug-build.txt
  ```

  ```
  场景：BIE6_Mono 构建验证
  工具：Bash
  步骤：
    1. 运行 dotnet build -c BIE6_Mono
    2. 检查构建输出
  预期结果：构建成功
  证据：.sisyphus/evidence/task-7-mono-build.txt
  ```

  **提交**：NO（与任务 8 合并）

---

- [ ] 8. 语言切换功能验证

  **做什么**：
  - 验证语言切换时 GuiStyle 配置正确重载
  - 测试三种语言（英文、俄文、中文）的切换
  - 检查字体配置正确应用到 GUI 组件
  - 记录测试结果

  **禁止做**：
  - 不要跳过任何语言的测试

  **推荐 Agent Profile**：
  - **Category**: `deep`
  - **Skills**: `[]`
  - **理由**：需要理解完整的配置加载流程

  **并行化**：
  - **可并行运行**：NO
  - **并行组**：Sequential
  - **阻塞**：F1-F2
  - **被阻塞于**：任务 7

  **参考**：
  - `OverlayMap/Config/Global.cs:OnLanguageChanged` - 语言切换逻辑
  - `OverlayMap/Gui/TopMap/TopMapStyle.cs` - 字体应用示例

  **验收标准**：
  - [ ] 切换到英文时加载英文字体配置
  - [ ] 切换到俄文时加载俄文字体配置
  - [ ] 切换到中文时加载中文字体配置
  - [ ] 字体配置正确应用到 GUI 组件

  **QA 场景**：

  ```
  场景：验证语言切换流程（代码审查）
  工具：Read
  步骤：
    1. 读取 Global.cs 的 OnLanguageChanged 方法
    2. 检查是否调用 GuiStyle 配置重载
    3. 检查日志输出路径
  预期结果：语言切换逻辑完整
  证据：.sisyphus/evidence/task-8-code-review.png
  ```

  **提交**：YES
  - 消息：`test(overlay): 添加语言切换配置验证`
  - 文件：`.sisyphus/evidence/task-8-verification.md`
  - 预检查：`dotnet build -c Debug`

---

## Final Verification Wave

> 4 个审查 Agent 并行运行。全部必须批准。拒绝 → 修复 → 重新运行。

- [ ] F1. **计划合规性审查** — `oracle`
  逐字阅读计划。对每个"Must Have"：验证实现存在（读文件、检查配置）。对每个"Must NOT Have"：搜索代码库查找禁止模式。检查配置文件存在于 OverlayMap/ConfigPrefabs/。比较交付物与计划。
  输出：`Must Have [N/N] | Must NOT Have [N/N] | Tasks [N/N] | VERDICT: APPROVE/REJECT`

- [ ] F2. **代码质量审查** — `unspecified-high`
  运行 `dotnet build -c Debug` + `dotnet build -c BIE6_Mono`。审查所有修改的文件：命名规范、注释完整性、事件处理正确性、资源清理。检查 AI slop：过度注释、硬编码路径、魔法数字。
  输出：`Build [PASS/FAIL] | Files [N modified/N clean] | VERDICT`

- [ ] F3. **配置完整性 QA** — `unspecified-high`
  从干净状态开始。验证每个配置文件：GuiStyle.en-US.cfg, GuiStyle.ru-RU.cfg, GuiStyle.zh-CN.cfg。检查语法正确性、字体路径有效性、无重复配置。保存证据到 `.sisyphus/evidence/final-qa/`。
  输出：`Configs [N/N pass] | Syntax [N/N valid] | Paths [N/N valid] | VERDICT`

- [ ] F4. **范围保真度检查** — `deep`
  对每个任务：阅读"做什么"，阅读实际变更（git diff）。验证 1:1 — 规格中的所有内容都已构建（无缺失），没有超出规格的内容（无范围蔓延）。检查"禁止做"合规性。
  输出：`Tasks [N/N compliant] | Scope Creep [CLEAN/N issues] | VERDICT`

---

## Commit Strategy

- **1**: `config(overlay): 添加英文字体配置` — GuiStyle.en-US.cfg
- **2**: `config(overlay): 添加俄文字体配置` — GuiStyle.ru-RU.cfg
- **3**: `config(overlay): 添加中文字体配置` — GuiStyle.zh-CN.cfg
- **4**: `feat(overlay): 添加语言配置加载支持` — GuiStyle.cs
- **5**: `feat(overlay): 语言切换时触发 GuiStyle 配置重载` — Global.cs
- **6**: `feat(overlay): 支持字体动态重载` — FontManager.cs（如果有修改）
- **7-8**: `chore(overlay): 编译验证和配置检查` — 合并到上述提交

**预提交检查**：`dotnet build -c Debug`

---

## Success Criteria

### 验证命令
```bash
# Debug 构建
dotnet build -c Debug  # 预期：构建成功，无错误

# Mono 发布构建
dotnet build -c BIE6_Mono  # 预期：构建成功，无错误

# IL2CPP 发布构建
dotnet build -c BIE6_IL2CPP  # 预期：构建成功，无错误
```

### 最终检查清单
- [ ] 所有"Must Have"已实现
- [ ] 所有"Must NOT Have"不存在
- [ ] 所有构建通过
- [ ] 三种语言的 GuiStyle 配置文件完整
- [ ] 语言切换触发配置重载
- [ ] 代码符合现有风格规范

---

## 字体配置推荐

### 英文 (en-US)
```ini
[TopMap]
BackgroundColor = 0,0,0,0
BackgroundImageFile = Background.png
BackgroundImageArea = 17,17,94,94
BackgroundImageBorder = 17,17,17,17

[TopMap.Count]
Font = arial.ttf
FontSize = 12
FallbackFonts = 

[TopMap.Sign]
Font = fonts/notosans-medium
FontSize = 12
FallbackFonts = arial.ttf

[TopMap.Title]
Font = fonts/notosans-medium
FontSize = 12
FallbackFonts = 

[StatsInfo]
BackgroundColor = 0,0,0,0
BackgroundImageFile = Background.png
BackgroundImageArea = 17,17,94,94
BackgroundImageBorder = 17,17,17,17

[StatsInfo.Text]
Font = fonts/notosans-medium
FontSize = 13
FallbackFonts = 

[ExtraInfo.Text]
Font = fonts/notosans-medium
FontSize = 13
FallbackFonts = 
```

### 俄文 (ru-RU)
```ini
[TopMap]
# 与英文相同

[TopMap.Count]
Font = arial.ttf
FontSize = 12
FallbackFonts = 

[TopMap.Sign]
Font = fonts/notosans-medium
FontSize = 12
FallbackFonts = arial.ttf

[TopMap.Title]
Font = fonts/notosans-medium
FontSize = 12
FallbackFonts = 

# StatsInfo 和 ExtraInfo 与英文相同
```

### 中文 (zh-CN)
```ini
[TopMap]
# 背景配置与其他语言相同

[TopMap.Count]
Font = arial.ttf
FontSize = 12
FallbackFonts = 

[TopMap.Sign]
Font = arial.ttf
FontSize = 12
FallbackFonts = seguisym.ttf

[TopMap.Title]
Font = fonts/notosanssc-medium
FontSize = 12
FallbackFonts = 

[StatsInfo.Text]
Font = fonts/notosanssc-medium
FontSize = 13
FallbackFonts = 

[ExtraInfo.Text]
Font = fonts/notosanssc-medium
FontSize = 13
FallbackFonts = 
```

---

## 架构说明

### 配置文件结构
```
OverlayMap/ConfigPrefabs/
├── GuiStyle.en-US.cfg      # 英文配置
├── GuiStyle.ru-RU.cfg      # 俄文配置
├── GuiStyle.zh-CN.cfg      # 中文配置
├── Language.en-US.cfg      # 英文语言字符串
├── Language.ru-RU.cfg      # 俄文语言字符串
├── Language.zh-CN.cfg      # 中文语言字符串
└── MarkerStyle.cfg         # 标记样式（独立于语言）
```

### 配置加载流程
```
1. 启动时：
   - 读取 Global.Language 配置（默认 "system"）
   - 根据系统语言或用户设置确定语言代码

2. OnLanguageChanged() 触发：
   - 加载 Language.{lang}.cfg（语言字符串）
   - 加载 GuiStyle.{lang}.cfg（字体和样式配置）
   - 触发 GuiStyle 配置重载事件
   - GUI 组件更新字体和样式

3. 配置重载：
   - GuiStyle.LoadLanguageConfig(lang)
   - 重新绑定所有配置项
   - 触发 OnConfigChanged 事件
   - FontManager 重新加载字体
```

### 字体优先级策略
1. **游戏内置字体**（fonts/ 目录） - 优先使用
2. **系统字体**（Windows Fonts 目录） - 作为备选
3. **外部字体** - 不推荐（增加依赖）

### 文件命名规范
- 语言字符串：`Language.{language-code}.cfg`
- GuiStyle 配置：`GuiStyle.{language-code}.cfg`
- 语言代码格式：RFC 5646（如 en-US, ru-RU, zh-CN）
