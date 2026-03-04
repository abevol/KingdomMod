#!/usr/bin/env bash
# scripts/delete-release.sh — 删除指定或最新的 Git 标签和对应的 GitHub Release
#
# 用法:
#   ./scripts/delete-release.sh         # 删除最新的 v* 标签和 Release
#   ./scripts/delete-release.sh v2.5.0  # 删除指定的 v2.5.0 标签和 Release

set -euo pipefail

# 检查依赖
if ! command -v gh &> /dev/null; then
    echo "错误: 未找到 'gh' 命令。请先安装 GitHub CLI: https://cli.github.com/" >&2
    exit 1
fi

if ! command -v git &> /dev/null; then
    echo "错误: 未找到 'git' 命令。" >&2
    exit 1
fi

TARGET_TAG="${1:-}"

# 如果没有提供标签，则尝试获取最新的 v* 标签
if [ -z "$TARGET_TAG" ]; then
    echo "正在查找最新的版本标签..."
    # 确保我们有最新的远程标签
    git fetch --tags -q
    TARGET_TAG=$(git tag --list "v[0-9]*.[0-9]*.[0-9]*" --sort=-v:refname | head -1)
    
    if [ -z "$TARGET_TAG" ]; then
        echo "错误: 未找到任何形如 v*.*.* 的标签。" >&2
        exit 1
    fi
    echo "找到最新标签: $TARGET_TAG"
fi

echo -n "⚠️ 警告: 确定要从本地和远程删除标签和 Release '$TARGET_TAG' 吗？(y/N) "
read -r CONFIRM
if [[ ! "$CONFIRM" =~ ^[Yy]$ ]]; then
    echo "操作已取消。"
    exit 0
fi

echo "==> 1. 删除 GitHub Release..."
# gh release delete 不加 --cleanup-tag，因为我们要手动处理标签，以便更好地控制输出和错误
if gh release view "$TARGET_TAG" &>/dev/null; then
    gh release delete "$TARGET_TAG" -y
    echo "✅ GitHub Release '$TARGET_TAG' 已删除。"
else
    echo "ℹ️ GitHub Release '$TARGET_TAG' 不存在或已被删除。"
fi

echo "==> 2. 删除远程标签..."
if git ls-remote --tags origin | grep -q "refs/tags/$TARGET_TAG"; then
    git push origin --delete "$TARGET_TAG"
    echo "✅ 远程标签 '$TARGET_TAG' 已删除。"
else
    echo "ℹ️ 远程标签 '$TARGET_TAG' 不存在或已被删除。"
fi

echo "==> 3. 删除本地标签..."
if git rev-parse -q --verify "refs/tags/$TARGET_TAG" >/dev/null; then
    git tag -d "$TARGET_TAG"
    echo "✅ 本地标签 '$TARGET_TAG' 已删除。"
else
    echo "ℹ️ 本地标签 '$TARGET_TAG' 不存在或已被删除。"
fi

echo "🎉 清理完成！"
