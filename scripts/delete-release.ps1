<#
.SYNOPSIS
scripts/delete-release.ps1 — 删除指定或最新的 Git 标签和对应的 GitHub Release

.DESCRIPTION
使用 PowerShell 和 GitHub CLI (gh) 结合 Git 命令删除本地和远程的版本标签及其关联的 Release。

.EXAMPLE
./scripts/delete-release.ps1
删除最新的 v* 标签和 Release

.EXAMPLE
./scripts/delete-release.ps1 v2.5.0
删除指定的 v2.5.0 标签和 Release
#>

param (
    [string]$TargetTag = ""
)

$ErrorActionPreference = "Stop"

# 检查依赖
if (!(Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Error "错误: 未找到 'gh' 命令。请先安装 GitHub CLI: https://cli.github.com/"
    exit 1
}

if (!(Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Error "错误: 未找到 'git' 命令。"
    exit 1
}

# 如果没有提供标签，则获取最新的 v* 标签
if ([string]::IsNullOrWhiteSpace($TargetTag)) {
    Write-Host "正在查找最新的版本标签..."
    git fetch --tags -q

    # Windows 下通过 Git for Windows 自带的环境排序
    $TargetTag = (git tag --list "v[0-9]*.[0-9]*.[0-9]*" | Sort-Object -Descending | Select-Object -First 1)

    if ([string]::IsNullOrWhiteSpace($TargetTag)) {
        Write-Error "未找到任何形如 v*.*.* 的标签。"
        exit 1
    }
    Write-Host "找到最新标签: $TargetTag"
}

# 确认提示
$confirm = Read-Host "⚠️ 警告: 确定要从本地和远程删除标签和 Release '$TargetTag' 吗？(y/N)"
if ($confirm -notmatch '^[Yy]$') {
    Write-Host "操作已取消。"
    exit 0
}

Write-Host "==> 1. 删除 GitHub Release..."
try {
    # 尝试查找 Release
    gh release view $TargetTag 2>$null
    if ($LASTEXITCODE -eq 0) {
        gh release delete $TargetTag -y
        Write-Host "✅ GitHub Release '$TargetTag' 已删除。"
    } else {
        Write-Host "ℹ️ GitHub Release '$TargetTag' 不存在或已被删除。"
    }
} catch {
    Write-Host "ℹ️ 无法删除 GitHub Release。请手动检查。"
}

Write-Host "==> 2. 删除远程标签..."
try {
    # 检查远程是否包含该标签
    $remoteTags = git ls-remote --tags origin
    if ($remoteTags -match "refs/tags/$TargetTag") {
        git push origin --delete $TargetTag
        Write-Host "✅ 远程标签 '$TargetTag' 已删除。"
    } else {
        Write-Host "ℹ️ 远程标签 '$TargetTag' 不存在或已被删除。"
    }
} catch {
    Write-Host "ℹ️ 无法删除远程标签。请手动检查。"
}

Write-Host "==> 3. 删除本地标签..."
try {
    # 检查本地是否包含该标签
    $localTag = git rev-parse -q --verify "refs/tags/$TargetTag" 2>$null
    if ($LASTEXITCODE -eq 0) {
        git tag -d $TargetTag
        Write-Host "✅ 本地标签 '$TargetTag' 已删除。"
    } else {
        Write-Host "ℹ️ 本地标签 '$TargetTag' 不存在或已被删除。"
    }
} catch {
    Write-Host "ℹ️ 无法删除本地标签。请手动检查。"
}

Write-Host "🎉 清理完成！"
