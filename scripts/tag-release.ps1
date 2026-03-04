# tag-release.ps1 — 从 Git 标签自动推导下一版本号，打标签并推送
#
# 用法:
#   .\scripts\tag-release.ps1              # 自动递增 patch（默认）
#   .\scripts\tag-release.ps1 patch        # 2.4.5 → 2.4.6
#   .\scripts\tag-release.ps1 minor        # 2.4.5 → 2.5.0
#   .\scripts\tag-release.ps1 major        # 2.4.5 → 3.0.0
#   .\scripts\tag-release.ps1 2.6.0        # 使用指定版本号
#   .\scripts\tag-release.ps1 -DryRun      # 仅显示将要创建的标签，不执行

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$BumpOrVersion = "patch",

    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$TagPrefix = "v"

# ── 检查环境 ──────────────────────────────────────────────────
$null = git rev-parse --is-inside-work-tree 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 当前目录不是 Git 仓库" -ForegroundColor Red
    exit 1
}

$status = git status --porcelain
if ($status) {
    Write-Host "警告: 工作区有未提交的更改" -ForegroundColor Yellow
    git status --short
    Write-Host ""
    $confirm = Read-Host "是否继续？(y/N)"
    if ($confirm -notmatch '^[Yy]$') {
        Write-Host "已取消"
        exit 0
    }
}

# ── 获取最新标签 ──────────────────────────────────────────────
$allTags = git tag --list "${TagPrefix}[0-9]*.[0-9]*.[0-9]*" --sort=-v:refname 2>$null
$latestTag = if ($allTags) { ($allTags -split "`n")[0].Trim() } else { "" }

if (-not $latestTag) {
    Write-Host "未找到现有版本标签，使用初始版本 0.0.0" -ForegroundColor Yellow
    $currentVersion = "0.0.0"
} else {
    $currentVersion = $latestTag -replace "^$TagPrefix", ""
}

Write-Host "当前版本: ${TagPrefix}${currentVersion}" -ForegroundColor Cyan

# ── 解析参数 ──────────────────────────────────────────────────
$bump = ""
$explicitVersion = ""

if ($BumpOrVersion -match '^\d+\.\d+\.\d+$') {
    $explicitVersion = $BumpOrVersion
} elseif ($BumpOrVersion -in @("major", "minor", "patch")) {
    $bump = $BumpOrVersion
} elseif ($BumpOrVersion -in @("-h", "--help")) {
    Write-Host "用法: .\scripts\tag-release.ps1 [patch|minor|major|<version>] [-DryRun]"
    Write-Host ""
    Write-Host "  patch        递增补丁版本号（默认）  2.4.5 → 2.4.6"
    Write-Host "  minor        递增次版本号            2.4.5 → 2.5.0"
    Write-Host "  major        递增主版本号            2.4.5 → 3.0.0"
    Write-Host "  <version>    使用指定版本号          如 2.6.0"
    Write-Host "  -DryRun      仅显示，不执行"
    exit 0
} else {
    Write-Host "错误: 未知参数 '$BumpOrVersion'" -ForegroundColor Red
    Write-Host "运行 '.\scripts\tag-release.ps1 --help' 查看用法"
    exit 1
}

# ── 计算下一版本 ──────────────────────────────────────────────
if ($explicitVersion) {
    $nextVersion = $explicitVersion
} else {
    $parts = $currentVersion -split '\.'
    [int]$major = $parts[0]
    [int]$minor = $parts[1]
    [int]$patch = $parts[2]

    switch ($bump) {
        "major" { $nextVersion = "$($major + 1).0.0" }
        "minor" { $nextVersion = "${major}.$($minor + 1).0" }
        "patch" { $nextVersion = "${major}.${minor}.$($patch + 1)" }
    }
}

$nextTag = "${TagPrefix}${nextVersion}"

# ── 检查标签是否已存在 ────────────────────────────────────────
$existing = git tag --list $nextTag 2>$null
if ($existing) {
    Write-Host "错误: 标签 '${nextTag}' 已存在" -ForegroundColor Red
    exit 1
}

# ── 显示摘要 ──────────────────────────────────────────────────
$commitShort = git rev-parse --short HEAD
$commitMsg = git log -1 --format='%s'

Write-Host ""
Write-Host "  ${TagPrefix}${currentVersion} → ${nextTag}" -ForegroundColor Green
Write-Host "  提交: ${commitShort} ${commitMsg}" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "[dry-run] 将执行: git tag ${nextTag} && git push origin ${nextTag}" -ForegroundColor Yellow
    exit 0
}

# ── 确认并执行 ────────────────────────────────────────────────
$confirm = Read-Host "确认创建标签并推送？(y/N)"
if ($confirm -notmatch '^[Yy]$') {
    Write-Host "已取消"
    exit 0
}

git tag $nextTag
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 创建标签失败" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 已创建标签: ${nextTag}" -ForegroundColor Green

git push origin $nextTag
if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 推送标签失败" -ForegroundColor Red
    exit 1
}
Write-Host "✓ 已推送标签: ${nextTag}" -ForegroundColor Green
Write-Host ""
Write-Host "发布工作流已触发，可在 GitHub Actions 查看进度。" -ForegroundColor Green
