# 计划：版本号管理 + GitHub Actions CI/CD 自动发布

## 元信息
- **计划创建**：2026-02-25
- **目标仓库**：KingdomMapModDev（Kingdom Two Crowns BepInEx 模组 monorepo）
- **执行者**：Sisyphus

---

## 背景与约束

### 项目现状
- **5 个项目**：SharedLib（基础库）+ OverlayMap、StaminaBar、DevTools、BetterPayableUpgrade（4 个模组）
- **版本统一**：`ProjectSettings.shared.props` → `<Version>2.4.3</Version>`，所有模组共用
- **构建配置**：`BIE6_IL2CPP`（net6.0）和 `BIE6_Mono`（netstandard2.1）
- **`_libs/` DLL**：已提交 git，CI 直接可用
- **`ProjectSettings.custom.props`**：被 gitignore，记录本地游戏路径，**CI 必须在 `dotnet restore` 之前动态生成**
- **CopyOutput MSBuild Target**：构建后复制 DLL 到 `BepInExPluginsPath`，若目录不存在则失败
- **`AppendTargetFrameworkToOutputPath=false`**：产物在 `bin/$CONFIG/`，**无** `net6.0/` 子目录

### 关键依赖（Metis 发现）
- **BLOCKER**：`custom.props` 是无条件 `<Import>`，必须在 MSBuild 解析阶段之前存在
- **BLOCKER**：IL2CPP `with-BepInEx` 整合包需要 3 个 ZIP：BepInEx + Cpp2IL.Patch + Il2CppInterop.Patch
- **BLOCKER**：matrix 并发构建存在竞态条件，Release 创建必须单独 job

### 用户决策
| 决策项 | 选择 |
|--------|------|
| 版本策略 | 统一版本号（`ProjectSettings.shared.props`） |
| 版本注入 | `dotnet build -p:Version=$VER`（不修改文件） |
| 触发方式 | `v*.*.*` git tag 推送自动触发 |
| Runner | ubuntu-latest |
| 发布产物 | All 合并包 + with-BepInEx 整合包（IL2CPP + Mono 各一，共 4 个 ZIP） |
| PR 检查 | 必须构建通过才能合并 |

### 发布产物清单（精确文件名）
```
KingdomMod.All-BIE6_IL2CPP-v{version}.zip
KingdomMod.All-BIE6_Mono-v{version}.zip
KingdomMod.All-BIE6_IL2CPP-v{version}-with-BepInEx.zip
KingdomMod.All-BIE6_Mono-v{version}-with-BepInEx.zip
```

### ZIP 内部结构（必须精确还原）
**All ZIP**（来自 `handle_all.bat` 逻辑）：
```
KingdomMod.BetterPayableUpgrade/
  KingdomMod.BetterPayableUpgrade.dll
  KingdomMod.SharedLib.dll
KingdomMod.DevTools/
  KingdomMod.DevTools.dll
  KingdomMod.SharedLib.dll
KingdomMod.OverlayMap/
  KingdomMod.OverlayMap.dll
  KingdomMod.SharedLib.dll
KingdomMod.StaminaBar/
  KingdomMod.StaminaBar.dll
  KingdomMod.SharedLib.dll
```

**with-BepInEx ZIP**（来自 `merge_package.bat` 逻辑，README 记载路径）：
```
winhttp.dll
BepInEx/
  core/
  ...
  plugins/
    KingdomMod.BetterPayableUpgrade/
      KingdomMod.BetterPayableUpgrade.dll
      KingdomMod.SharedLib.dll
    KingdomMod.DevTools/
    KingdomMod.OverlayMap/
    KingdomMod.StaminaBar/
```

### 外部依赖 URL（硬编码，勿改）
```bash
# IL2CPP BepInEx（注意 URL 中 + 需编码为 %2B）
BEPINEX_IL2CPP_URL="https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip"
# Mono BepInEx
BEPINEX_MONO_URL="https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip"
# IL2CPP 补丁（来自本项目历史 Release，版本号固定）
CPP2IL_PATCH_URL="https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip"
IL2CPP_INTEROP_PATCH_URL="https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip"
```

---

## 文件变更清单

### 新建文件
```
.github/
  workflows/
    ci.yml           # PR 构建验证工作流
    release.yml      # tag 触发发布工作流
```

### 不修改文件
- 所有 `.csproj` 文件（版本通过 `-p:Version=` 注入）
- `ProjectSettings.shared.props`（版本号手动维护，tag 为发布信号）
- `_bin/` 目录下所有 `.bat` 文件（保留用于本地 Windows 打包）

---

## 工作流架构

```
ci.yml:
  on: [pull_request, push to main]
  job: build-check
    → 生成 custom.props
    → dotnet restore + dotnet build (BIE6_IL2CPP)
    → dotnet build (BIE6_Mono)

release.yml:
  on: push tag v*.*.*
  job: validate
    → 校验 tag 格式
  job: build (matrix: BIE6_IL2CPP, BIE6_Mono) [needs: validate]
    → 生成 custom.props
    → dotnet restore + dotnet build -p:Version=$VER
    → 打包 All ZIP
    → upload-artifact
  job: package-with-bepinex [needs: build]
    → download-artifact (两个 All ZIP)
    → 下载 BepInEx ZIP（含补丁）
    → 解压合并，重打包 with-BepInEx ZIP（两个）
    → upload-artifact
  job: release [needs: package-with-bepinex]
    → download-artifact（4 个 ZIP）
    → gh release create + upload assets
```

---

## 任务列表

<!-- TASKS_START -->

### TASK-01：创建 `.github/workflows/` 目录结构
**文件**：`$REPO_ROOT/.github/workflows/`（目录，需创建）
**操作**：`mkdir -p .github/workflows`
**验收**：目录存在

---

### TASK-02：创建 CI 构建验证工作流 `ci.yml`
**文件**：`.github/workflows/ci.yml`（新建）
**触发条件**：
- `push` → 分支 `main`（以及 `master`，兼容两种命名）
- `pull_request` → 目标分支 `main`/`master`

**权限**：`contents: read`（最小权限原则）

**并发控制**：
```yaml
concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true  # PR 推送新 commit 取消旧 CI
```

**job: build-check**，runs-on: ubuntu-latest

**步骤顺序（严格按序）**：

**步骤 1 - Checkout**：
```yaml
- uses: actions/checkout@v4
```

**步骤 2 - Setup .NET**（注意：IL2CPP 用 net6.0，Mono 用 netstandard2.1，但 SDK 6 可同时支持）：
```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '6.0.x'
```

**步骤 3 - 生成 CI 专用 `ProjectSettings.custom.props`**（BLOCKER，必须在 restore 之前）：
```yaml
- name: Generate CI custom props
  run: |
    CI_OUT=${{ github.workspace }}/ci-bepinex
    mkdir -p "$CI_OUT/plugins" "$CI_OUT/config"
    cat > ProjectSettings.custom.props << 'EOF'
    <Project>
      <PropertyGroup>
        <BepInExPath>${{ github.workspace }}/ci-bepinex/</BepInExPath>
        <BepInExConfigPath>${{ github.workspace }}/ci-bepinex/config/</BepInExConfigPath>
        <BepInExPluginsPath>${{ github.workspace }}/ci-bepinex/plugins/</BepInExPluginsPath>
      </PropertyGroup>
    </Project>
    EOF
```
注意：此处 HEREDOC 内路径使用字面量 `${{ github.workspace }}`，shell 会展开 GitHub Actions 上下文变量。

**步骤 4 - NuGet 缓存**：
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj') }}
    restore-keys: nuget-${{ runner.os }}-
```

**步骤 5 - dotnet restore**：
```yaml
- name: Restore dependencies
  run: dotnet restore KingdomMod.sln
```

**步骤 6 - Build IL2CPP**：
```yaml
- name: Build BIE6_IL2CPP
  run: dotnet build KingdomMod.sln -c BIE6_IL2CPP --no-restore
```

**步骤 7 - Build Mono**：
```yaml
- name: Build BIE6_Mono
  run: dotnet build KingdomMod.sln -c BIE6_Mono --no-restore
```

**验收**：
- workflow 在 PR 时自动运行
- 两个配置均构建成功（exit code 0）
- CopyOutput Target 复制到 `ci-bepinex/plugins/` 目录下不报错

---

### TASK-03：创建发布工作流 `release.yml`
**文件**：`.github/workflows/release.yml`（新建）
**触发条件**：`push` → tags 匹配 `v[0-9]+.[0-9]+.[0-9]+`

**权限（顶层）**：
```yaml
permissions:
  contents: write  # 创建 Release + 上传资产
```

**并发控制**：
```yaml
concurrency:
  group: release-${{ github.ref }}
  cancel-in-progress: false  # 发布不允许取消
```

完整 job 架构见下方 TASK-04 至 TASK-08。

---

### TASK-04：release.yml → job `validate`
**purpose**：校验 tag 格式，提取版本号，供后续 job 通过 output 使用

```yaml
jobs:
  validate:
    runs-on: ubuntu-latest
    outputs:
      version: ${{ steps.extract.outputs.version }}
    steps:
      - name: Validate and extract version
        id: extract
        run: |
          TAG="${{ github.ref_name }}"
          if [[ ! "$TAG" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
            echo "ERROR: Tag '$TAG' does not match v*.*.* format"
            exit 1
          fi
          VERSION="${TAG#v}"
          echo "version=$VERSION" >> $GITHUB_OUTPUT
          echo "Releasing version: $VERSION"
```

**验收**：
- tag `v2.4.3` → output `version=2.4.3` ✅
- tag `vbeta` → job 失败并输出错误信息 ✅
- tag `v2.4` → job 失败 ✅

---

### TASK-05：release.yml → job `build`（matrix 构建 + 打包 All ZIP）
**needs**：`validate`
**strategy**：`matrix: config: [BIE6_IL2CPP, BIE6_Mono]`
**runs-on**：ubuntu-latest

**版本变量**：`${{ needs.validate.outputs.version }}`

**步骤顺序**：

**步骤 1 - Checkout**：`actions/checkout@v4`

**步骤 2 - Setup .NET 6**：`actions/setup-dotnet@v4` with `dotnet-version: '6.0.x'`

**步骤 3 - 生成 CI custom.props**（与 TASK-02 步骤 3 完全相同）：
- 创建 `$GITHUB_WORKSPACE/ci-bepinex/{plugins,config}` 目录
- 写入 `ProjectSettings.custom.props` XML

**步骤 4 - NuGet 缓存**（与 TASK-02 步骤 4 相同）

**步骤 5 - dotnet restore**：`dotnet restore KingdomMod.sln`

**步骤 6 - dotnet build with version injection**：
```bash
dotnet build KingdomMod.sln \
  -c ${{ matrix.config }} \
  --no-restore \
  -p:Version=${{ needs.validate.outputs.version }}
```

**步骤 7 - 创建模组子目录并收集产物**：
```bash
# 变量
CONFIG=${{ matrix.config }}
VERSION=${{ needs.validate.outputs.version }}
STAGING=$GITHUB_WORKSPACE/_staging

# 对每个模组创建命名子目录，复制 DLL
# 注意：bin/$CONFIG/ 无 TargetFramework 子目录（AppendTargetFrameworkToOutputPath=false）
# 注意：SharedLib 的 DLL 也需要复制到每个模组子目录（还原 bat 脚本行为）
MODS=(BetterPayableUpgrade DevTools OverlayMap StaminaBar)
for MOD in "${MODS[@]}"; do
  MOD_DIR="$STAGING/KingdomMod.$MOD"
  mkdir -p "$MOD_DIR"
  cp "$MOD/bin/$CONFIG/KingdomMod.$MOD.dll" "$MOD_DIR/"
  cp "SharedLib/bin/$CONFIG/KingdomMod.SharedLib.dll" "$MOD_DIR/"
done
```

**步骤 8 - 打包 All ZIP**：
```bash
VERSION=${{ needs.validate.outputs.version }}
CONFIG=${{ matrix.config }}
STAGING=$GITHUB_WORKSPACE/_staging
ZIP_NAME="KingdomMod.All-${CONFIG}-v${VERSION}.zip"

cd "$STAGING"
zip -r "$GITHUB_WORKSPACE/$ZIP_NAME" .
cd "$GITHUB_WORKSPACE"
```

**步骤 9 - upload-artifact**：
```yaml
- uses: actions/upload-artifact@v4
  with:
    name: all-zip-${{ matrix.config }}
    path: KingdomMod.All-${{ matrix.config }}-v${{ needs.validate.outputs.version }}.zip
    retention-days: 1
```

**验收**：
- `OverlayMap/bin/BIE6_IL2CPP/KingdomMod.OverlayMap.dll` 存在（路径无 net6.0 子目录）
- `SharedLib/bin/BIE6_IL2CPP/KingdomMod.SharedLib.dll` 存在
- All ZIP 内部有 4 个命名子目录，每个含 2 个 DLL
- `unzip -l *.zip | grep 'KingdomMod.OverlayMap/KingdomMod.OverlayMap.dll'` 有输出

---

### TASK-06：release.yml → job `package-with-bepinex`（下载 BepInEx + 合并打包）
**needs**：`build`（等待 matrix 两个 job 均完成）
**runs-on**：ubuntu-latest

**步骤 1 - download-artifact（IL2CPP All ZIP）**：
```yaml
- uses: actions/download-artifact@v4
  with:
    name: all-zip-BIE6_IL2CPP
    path: ./zips
```

**步骤 2 - download-artifact（Mono All ZIP）**：
```yaml
- uses: actions/download-artifact@v4
  with:
    name: all-zip-BIE6_Mono
    path: ./zips
```

**步骤 3 - 下载 IL2CPP BepInEx + 补丁**（注意 URL 中 + 编码为 %2B）：
```bash
mkdir -p _bepinex_zips

# BepInEx IL2CPP
curl -fsSL -o _bepinex_zips/BepInEx-IL2CPP.zip \
  'https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753%2B0d275a4.zip'

# BepInEx Mono
curl -fsSL -o _bepinex_zips/BepInEx-Mono.zip \
  'https://builds.bepinex.dev/projects/bepinex_be/753/BepInEx-Unity.Mono-win-x64-6.0.0-be.753%2B0d275a4.zip'

# IL2CPP 补丁（本项目历史 Release，版本号固定）
curl -fsSL -L -o _bepinex_zips/Cpp2IL.Patch.zip \
  'https://github.com/abevol/KingdomMod/releases/download/2.4.0/Cpp2IL.Patch.zip'

curl -fsSL -L -o _bepinex_zips/Il2CppInterop.Patch.zip \
  'https://github.com/abevol/KingdomMod/releases/download/2.4.3/Il2CppInterop.Patch.zip'
```

**步骤 4 - 合并打包 IL2CPP with-BepInEx ZIP**（还原 `merge_package.bat` 逻辑）：
```bash
VERSION=${{ needs.validate.outputs.version }}
TMPDIR_IL2CPP=$RUNNER_TEMP/merge_il2cpp
mkdir -p "$TMPDIR_IL2CPP"

# 解压 BepInEx 框架（含 winhttp.dll 等根目录文件）
unzip -q _bepinex_zips/BepInEx-IL2CPP.zip -d "$TMPDIR_IL2CPP"

# 解压 IL2CPP 补丁（覆盖同名文件）
unzip -q -o _bepinex_zips/Cpp2IL.Patch.zip -d "$TMPDIR_IL2CPP"
unzip -q -o _bepinex_zips/Il2CppInterop.Patch.zip -d "$TMPDIR_IL2CPP"

# 解压模组包到 BepInEx/plugins/ 目录
mkdir -p "$TMPDIR_IL2CPP/BepInEx/plugins"
unzip -q zips/KingdomMod.All-BIE6_IL2CPP-v${VERSION}.zip \
  -d "$TMPDIR_IL2CPP/BepInEx/plugins"

# 重新打包为最终 ZIP
cd "$TMPDIR_IL2CPP"
zip -r "$GITHUB_WORKSPACE/KingdomMod.All-BIE6_IL2CPP-v${VERSION}-with-BepInEx.zip" .
cd "$GITHUB_WORKSPACE"
rm -rf "$TMPDIR_IL2CPP"
```

**步骤 5 - 合并打包 Mono with-BepInEx ZIP**（同上，Mono 无 Cpp2IL/Il2CppInterop 补丁）：
```bash
VERSION=${{ needs.validate.outputs.version }}
TMPDIR_MONO=$RUNNER_TEMP/merge_mono
mkdir -p "$TMPDIR_MONO"

# 解压 BepInEx Mono 框架
unzip -q _bepinex_zips/BepInEx-Mono.zip -d "$TMPDIR_MONO"

# 解压模组包到 BepInEx/plugins/
mkdir -p "$TMPDIR_MONO/BepInEx/plugins"
unzip -q zips/KingdomMod.All-BIE6_Mono-v${VERSION}.zip \
  -d "$TMPDIR_MONO/BepInEx/plugins"

# 重新打包
cd "$TMPDIR_MONO"
zip -r "$GITHUB_WORKSPACE/KingdomMod.All-BIE6_Mono-v${VERSION}-with-BepInEx.zip" .
cd "$GITHUB_WORKSPACE"
rm -rf "$TMPDIR_MONO"
```

**步骤 6 - upload-artifact（4 个 ZIP 统一上传）**：
```yaml
- uses: actions/upload-artifact@v4
  with:
    name: release-zips
    path: |
      KingdomMod.All-BIE6_IL2CPP-v*.zip
      KingdomMod.All-BIE6_Mono-v*.zip
      KingdomMod.All-BIE6_IL2CPP-v*-with-BepInEx.zip
      KingdomMod.All-BIE6_Mono-v*-with-BepInEx.zip
    retention-days: 1
```

**验收**：
- `unzip -l *-with-BepInEx.zip | grep winhttp.dll` 有输出
- `unzip -l *IL2CPP*-with-BepInEx.zip | grep 'BepInEx/plugins/KingdomMod.OverlayMap/'` 有输出
- IL2CPP with-BepInEx ZIP 比 Mono with-BepInEx ZIP 更大（含额外补丁）

---

### TASK-07：release.yml → job `release`（创建 GitHub Release）
**needs**：`package-with-bepinex`（等待打包完成）
**runs-on**：ubuntu-latest

**步骤 1 - download-artifact（4 个 ZIP）**：
```yaml
- uses: actions/download-artifact@v4
  with:
    name: release-zips
    path: ./release-assets
```

**步骤 2 - 获取版本号**（从 tag name 重新提取，因为跨 job 传递用 needs）：
```bash
VERSION="${GITHUB_REF_NAME#v}"
echo "VERSION=$VERSION" >> $GITHUB_ENV
```

**步骤 3 - 创建 GitHub Release 并上传资产**：
```yaml
- name: Create Release and upload assets
  run: |
    gh release create "${{ github.ref_name }}" \
      --title "KingdomMod v${{ needs.validate.outputs.version }}" \
      --generate-notes \
      --draft=false \
      release-assets/*.zip
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

说明：
- `--generate-notes`：自动从 commit 历史生成 Release 说明（可后续手动编辑）
- `GITHUB_TOKEN` 使用仓库内置 token，无需额外 Secret

**验收**：
- GitHub Release 页面出现版本对应的 Release 条目
- `gh release view v2.4.3 --json assets --jq '.assets | length'` 输出 `4`
- 4 个 ZIP 均可正常下载

---

### TASK-08：在 `ci.yml` 中添加跳过 CopyOutput 失败的容错（可选加固）
**背景**：CI 环境 `BepInExPluginsPath` 设置为临时目录，若 `dotnet build` 过程中 CopyOutput Target 失败会中止构建。
**已通过 TASK-02/05 预先创建目录解决**，但若目录权限问题仍报错，可在 build 命令加 `--no-dependencies` 跳过。

**实际操作**：在 TASK-02 和 TASK-05 的步骤 3（生成 custom.props）之后，确认已执行：
```bash
mkdir -p "$GITHUB_WORKSPACE/ci-bepinex/plugins"
mkdir -p "$GITHUB_WORKSPACE/ci-bepinex/config"
```
这两行是防止 MSBuild Copy Task 失败的**硬性保障**，不可省略。

---

### TASK-09：在仓库 GitHub 设置中配置分支保护规则（人工操作）
**此步骤无法通过 YAML 自动化，需仓库 Owner 手动完成。**

**操作路径**：GitHub → 仓库 Settings → Branches → Add branch protection rule

**规则配置**（针对 `main` 分支）：
- ✅ Require status checks to pass before merging
- ✅ Status check 选择：`build-check`（来自 ci.yml 的 job 名称）
- ✅ Require branches to be up to date before merging
- ✅ Do not allow bypassing the above settings

**记录**：此步骤完成后，PR 必须通过 CI 才能合并。

---

### TASK-10：验证 `_libs/` 路径分隔符在 Linux 上的兼容性
**问题**：所有 `.csproj` 的 HintPath 使用 Windows 反斜杠（`_libs\BIE6_IL2CPP\core\0Harmony.dll`）。
**结论**：.NET 6 MSBuild 在 Linux 上会自动规范化路径分隔符，**无需修改**。
**验证方式**：TASK-05 的构建步骤若成功则此问题已解决。若失败，则需在此任务中将所有 `.csproj` 中的 `\` 替换为 `/`（5 个文件，批量操作）。

**验收**：Build IL2CPP 和 Mono 均成功，无 "File not found" 相关 HintPath 错误。

---

### TASK-11：更新 `.gitignore`（可选）
**目的**：确保 `ci-bepinex/` 临时目录不被意外提交。
**操作**：在 `.gitignore` 末尾追加一行：
```
/ci-bepinex/
```
**注意**：此目录只在 CI 环境（`$GITHUB_WORKSPACE`）创建，本地不会出现，但作为防护措施添加无害。

---

### TASK-12：文档更新 README 发布说明（可选，低优先级）
**文件**：`Readme.md` 和 `Readme.zh-CN.md`
**内容**：在 Install 章节说明新版 Release 的文件命名规则（文件名格式已统一，无需修改安装步骤）。
**操作**：仅确认现有 README 中描述的文件名格式与新生成的 ZIP 文件名一致，若一致则跳过。

<!-- TASKS_END -->

---

## Final Verification Wave

实现完成后，执行以下验证：

```bash
# 1. 验证 YAML 语法
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/ci.yml'))"
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/release.yml'))"

# 2. 验证 workflow 文件存在
test -f .github/workflows/ci.yml && echo "ci.yml OK"
test -f .github/workflows/release.yml && echo "release.yml OK"

# 3. 本地模拟 custom.props 生成（验证 XML 格式正确）
bash -c '
  CI_OUT=/tmp/ci-test-out
  mkdir -p "$CI_OUT/plugins" "$CI_OUT/config"
  cat > /tmp/test-custom.props <<EOF
<Project>
  <PropertyGroup>
    <BepInExPath>$CI_OUT/</BepInExPath>
    <BepInExConfigPath>$CI_OUT/config/</BepInExConfigPath>
    <BepInExPluginsPath>$CI_OUT/plugins/</BepInExPluginsPath>
  </PropertyGroup>
</Project>
EOF
  echo "custom.props OK"
'

# 4. 验证 act（本地 GitHub Actions 运行）可加载 workflow（若已安装）
# act --list -W .github/workflows/ci.yml 2>/dev/null || echo "act not installed, skip"
```
