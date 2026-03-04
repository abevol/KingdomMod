#!/usr/bin/env bash
# generate-changelog.sh — 使用 AI 从 Git 提交历史生成结构化更新日志
#
# 用法:
#   ./scripts/generate-changelog.sh v2.5.0                  # 生成 v2.5.0 的更新日志
#   ./scripts/generate-changelog.sh v2.5.0 -o changelog.md  # 输出到文件
#   GOOGLE_API_KEY=... ./scripts/generate-changelog.sh v2.5.0
#
# 环境变量:
#   GOOGLE_API_KEY  — Google Gemini API Key（必需）
#
# 当 API 不可用时，自动回退为基于 Conventional Commits 前缀的简单分组格式。

set -euo pipefail

TAG_PREFIX="v"
CURRENT_TAG=""
OUTPUT_FILE=""

# ── 参数解析 ──────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
    case "$1" in
        -o|--output) OUTPUT_FILE="$2"; shift 2 ;;
        -h|--help)
            echo "用法: $0 <tag> [-o output-file]"
            echo ""
            echo "  <tag>          目标版本标签（如 v2.5.0）"
            echo "  -o, --output   输出到文件（默认输出到 stdout）"
            echo ""
            echo "环境变量:"
            echo "  GOOGLE_API_KEY  Google Gemini API Key"
            exit 0
            ;;
        *) CURRENT_TAG="$1"; shift ;;
    esac
done

if [ -z "$CURRENT_TAG" ]; then
    echo "错误: 请指定目标版本标签" >&2
    echo "用法: $0 <tag> [-o output-file]" >&2
    exit 1
fi

# ── 获取上一个标签 ────────────────────────────────────────────
PREVIOUS_TAG=$(git tag --list "${TAG_PREFIX}[0-9]*.[0-9]*.[0-9]*" --sort=-v:refname \
    | grep -v "^${CURRENT_TAG}$" \
    | head -1)

if [ -z "$PREVIOUS_TAG" ]; then
    echo "警告: 未找到上一个版本标签，将使用所有提交历史" >&2
    COMMIT_RANGE="$CURRENT_TAG"
    RANGE_DESC="initial release"
else
    COMMIT_RANGE="${PREVIOUS_TAG}..${CURRENT_TAG}"
    RANGE_DESC="${PREVIOUS_TAG} → ${CURRENT_TAG}"
fi

echo "提交范围: ${RANGE_DESC}" >&2

# ── 收集提交信息 ──────────────────────────────────────────────
COMMITS=$(git log "$COMMIT_RANGE" --pretty=format:"%h %s" --no-merges 2>/dev/null || true)

if [ -z "$COMMITS" ]; then
    echo "警告: 未找到提交记录" >&2
    CHANGELOG="No changes recorded."
    if [ -n "$OUTPUT_FILE" ]; then
        echo "$CHANGELOG" > "$OUTPUT_FILE"
    else
        echo "$CHANGELOG"
    fi
    exit 0
fi

COMMIT_COUNT=$(echo "$COMMITS" | wc -l | tr -d ' ')
echo "找到 ${COMMIT_COUNT} 个提交" >&2

# ── 简单回退：基于 Conventional Commits 前缀分组 ──────────────
generate_simple_changelog() {
    local commits="$1"
    local tag="$2"

    local features="" fixes="" improvements="" chores="" others=""

    while IFS= read -r line; do
        hash="${line%% *}"
        msg="${line#* }"
        entry="- ${msg} (\`${hash}\`)"

        case "$msg" in
            feat*|add*|新增*|添加*|支持*)   features="${features}${entry}"$'\n' ;;
            fix*|修复*|防止*)               fixes="${fixes}${entry}"$'\n' ;;
            refactor*|perf*|优化*|重构*)    improvements="${improvements}${entry}"$'\n' ;;
            build*|ci*|chore*|docs*|style*) chores="${chores}${entry}"$'\n' ;;
            *)                              others="${others}${entry}"$'\n' ;;
        esac
    done <<< "$commits"

    local result=""
    [ -n "$features" ]     && result="${result}## ✨ New Features"$'\n\n'"${features}"$'\n'
    [ -n "$fixes" ]        && result="${result}## 🐛 Bug Fixes"$'\n\n'"${fixes}"$'\n'
    [ -n "$improvements" ] && result="${result}## ⚡ Improvements"$'\n\n'"${improvements}"$'\n'
    [ -n "$chores" ]       && result="${result}## 🔧 Maintenance"$'\n\n'"${chores}"$'\n'
    [ -n "$others" ]       && result="${result}## 📝 Other Changes"$'\n\n'"${others}"$'\n'

    echo "$result"
}

# ── AI 生成：调用 Gemini API ──────────────────────────────────
generate_ai_changelog() {
    local commits="$1"
    local tag="$2"

    if [ -z "${GOOGLE_API_KEY:-}" ]; then
        echo "GOOGLE_API_KEY 未设置，回退到简单分组模式" >&2
        return 1
    fi

    local prompt
    prompt=$(cat <<'PROMPT_EOF'
You are a release note writer for a game modding project called "KingdomMod" (mods for the game "Kingdom Two Crowns").

Given the following git commit messages, generate a structured changelog in English.

Rules:
1. Group changes into these sections (only include sections that have entries):
   - ## ✨ New Features
   - ## 🐛 Bug Fixes
   - ## ⚡ Improvements
   - ## 🔧 Maintenance
2. Each entry should be a concise, user-friendly bullet point starting with "- ".
3. Rewrite commit messages into clear descriptions. Remove technical prefixes like "feat:", "fix:", etc.
4. Translate any non-English commit messages to English.
5. Merge related commits into a single entry when appropriate.
6. Do NOT include section headers that have no entries.
7. Output ONLY the markdown content, no extra commentary.

Commits:
PROMPT_EOF
)

    # Build JSON payload with proper escaping
    local json_payload
    json_payload=$(jq -n \
        --arg prompt "$prompt" \
        --arg commits "$commits" \
        '{
            "contents": [{
                "parts": [{"text": ($prompt + "\n" + $commits)}]
            }],
            "generationConfig": {
                "temperature": 0.3,
                "maxOutputTokens": 2048
            }
        }')

    local api_url="https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=${GOOGLE_API_KEY}"

    local response
    response=$(curl -sS --max-time 30 \
        -H "Content-Type: application/json" \
        -d "$json_payload" \
        "$api_url" 2>/dev/null) || {
        echo "API 调用失败，回退到简单分组模式" >&2
        return 1
    }

    # Extract text from Gemini response
    local text
    text=$(echo "$response" | jq -r '.candidates[0].content.parts[0].text // empty' 2>/dev/null)

    if [ -z "$text" ]; then
        local error_msg
        error_msg=$(echo "$response" | jq -r '.error.message // empty' 2>/dev/null)
        echo "API 返回无效响应${error_msg:+: $error_msg}，回退到简单分组模式" >&2
        return 1
    fi

    echo "$text"
}

# ── 主流程 ────────────────────────────────────────────────────
CHANGELOG=$(generate_ai_changelog "$COMMITS" "$CURRENT_TAG" 2>/dev/null) || \
CHANGELOG=$(generate_simple_changelog "$COMMITS" "$CURRENT_TAG")

if [ -n "$OUTPUT_FILE" ]; then
    echo "$CHANGELOG" > "$OUTPUT_FILE"
    echo "更新日志已写入: ${OUTPUT_FILE}" >&2
else
    echo "$CHANGELOG"
fi
