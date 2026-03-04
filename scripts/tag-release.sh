#!/usr/bin/env bash
# tag-release.sh — 从 Git 标签自动推导下一版本号，打标签并推送
#
# 用法:
#   ./scripts/tag-release.sh              # 自动递增 patch（默认）
#   ./scripts/tag-release.sh patch        # 2.4.5 → 2.4.6
#   ./scripts/tag-release.sh minor        # 2.4.5 → 2.5.0
#   ./scripts/tag-release.sh major        # 2.4.5 → 3.0.0
#   ./scripts/tag-release.sh 2.6.0        # 使用指定版本号
#   ./scripts/tag-release.sh --dry-run    # 仅显示将要创建的标签，不执行

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

TAG_PREFIX="v"
DRY_RUN=false
BUMP="patch"
EXPLICIT_VERSION=""

# ── 参数解析 ──────────────────────────────────────────────────
for arg in "$@"; do
    case "$arg" in
        --dry-run) DRY_RUN=true ;;
        major|minor|patch) BUMP="$arg" ;;
        [0-9]*.[0-9]*.[0-9]*) EXPLICIT_VERSION="$arg" ;;
        -h|--help)
            echo "用法: $0 [patch|minor|major|<version>] [--dry-run]"
            echo ""
            echo "  patch        递增补丁版本号（默认）  2.4.5 → 2.4.6"
            echo "  minor        递增次版本号            2.4.5 → 2.5.0"
            echo "  major        递增主版本号            2.4.5 → 3.0.0"
            echo "  <version>    使用指定版本号          如 2.6.0"
            echo "  --dry-run    仅显示，不执行"
            exit 0
            ;;
        *)
            echo -e "${RED}错误: 未知参数 '$arg'${NC}"
            echo "运行 '$0 --help' 查看用法"
            exit 1
            ;;
    esac
done

# ── 检查环境 ──────────────────────────────────────────────────
if ! git rev-parse --is-inside-work-tree &>/dev/null; then
    echo -e "${RED}错误: 当前目录不是 Git 仓库${NC}"
    exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
    echo -e "${YELLOW}警告: 工作区有未提交的更改${NC}"
    git status --short
    echo ""
    read -rp "是否继续？(y/N) " confirm
    if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
        echo "已取消"
        exit 0
    fi
fi

# ── 获取最新标签 ──────────────────────────────────────────────
LATEST_TAG=$(git tag --list "${TAG_PREFIX}[0-9]*.[0-9]*.[0-9]*" --sort=-v:refname | head -1)

if [ -z "$LATEST_TAG" ]; then
    echo -e "${YELLOW}未找到现有版本标签，使用初始版本 0.0.0${NC}"
    CURRENT_VERSION="0.0.0"
else
    CURRENT_VERSION="${LATEST_TAG#$TAG_PREFIX}"
fi

echo -e "${CYAN}当前版本: ${TAG_PREFIX}${CURRENT_VERSION}${NC}"

# ── 计算下一版本 ──────────────────────────────────────────────
if [ -n "$EXPLICIT_VERSION" ]; then
    NEXT_VERSION="$EXPLICIT_VERSION"
else
    IFS='.' read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"
    case "$BUMP" in
        major) NEXT_VERSION="$((MAJOR + 1)).0.0" ;;
        minor) NEXT_VERSION="${MAJOR}.$((MINOR + 1)).0" ;;
        patch) NEXT_VERSION="${MAJOR}.${MINOR}.$((PATCH + 1))" ;;
    esac
fi

NEXT_TAG="${TAG_PREFIX}${NEXT_VERSION}"

# ── 检查标签是否已存在 ────────────────────────────────────────
if git tag --list "$NEXT_TAG" | grep -q "^${NEXT_TAG}$"; then
    echo -e "${RED}错误: 标签 '${NEXT_TAG}' 已存在${NC}"
    exit 1
fi

# ── 显示摘要 ──────────────────────────────────────────────────
COMMIT_SHORT=$(git rev-parse --short HEAD)
COMMIT_MSG=$(git log -1 --format='%s')

echo ""
echo -e "  ${GREEN}${TAG_PREFIX}${CURRENT_VERSION}${NC} → ${GREEN}${NEXT_TAG}${NC}"
echo -e "  提交: ${CYAN}${COMMIT_SHORT}${NC} ${COMMIT_MSG}"
echo ""

if [ "$DRY_RUN" = true ]; then
    echo -e "${YELLOW}[dry-run] 将执行: git tag ${NEXT_TAG} && git push origin ${NEXT_TAG}${NC}"
    exit 0
fi

# ── 确认并执行 ────────────────────────────────────────────────
read -rp "确认创建标签并推送？(y/N) " confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
    echo "已取消"
    exit 0
fi

git tag "$NEXT_TAG"
echo -e "${GREEN}✓ 已创建标签: ${NEXT_TAG}${NC}"

git push origin "$NEXT_TAG"
echo -e "${GREEN}✓ 已推送标签: ${NEXT_TAG}${NC}"
echo ""
echo -e "${GREEN}发布工作流已触发，可在 GitHub Actions 查看进度。${NC}"
