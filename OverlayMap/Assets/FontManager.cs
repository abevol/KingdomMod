using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Assets
{
    public class FontManager : MonoBehaviour
    {
        private readonly Dictionary<string, TMP_FontAsset> _fonts = new();

        public TMP_FontAsset GetFont(string fontName, TMP_FontAsset oldFontAsset)
        {
            var font = GetFont(fontName);
            if (font != null && oldFontAsset != null)
                TransferCharacters(oldFontAsset, font);

            return font;
        }

        public TMP_FontAsset GetFont(string fontName)
        {
            if (_fonts.TryGetValue(fontName, out var fontExisted))
            {
                return fontExisted;
            }

            var font = CreateFont(fontName);
            if (font != null)
                _fonts.Add(fontName, font);


            return font;
        }

        public void AddFallbackFonts(TMP_FontAsset mainFont, string[] fontNames, bool replace)
        {
            if (mainFont == null)
            {
                LogError("mainFont is null.");
                return;
            }

            if (replace)
                mainFont.fallbackFontAssetTable.Clear();

            foreach (var fontName in fontNames)
            {
                AddFallbackFont(mainFont, fontName);
            }
        }

        public void AddFallbackFont(TMP_FontAsset mainFont, string fontName)
        {
            if (mainFont == null)
            {
                LogError("mainFont is null.");
                return;
            }

            var fallbackFont = GetFont(fontName);
            if (fallbackFont == null)
            {
                LogError($"Failed to get fallback font: {fontName}");
                return;
            }

            AddFallbackFont(mainFont, fallbackFont);
        }

        public static void AddFallbackFont(TMP_FontAsset mainFont, TMP_FontAsset fontAsset)
        {
            if (!mainFont.fallbackFontAssetTable.Contains(fontAsset))
            {
                mainFont.fallbackFontAssetTable.Add(fontAsset);
                LogMessage($"Added fallback font: {fontAsset.name}");
            }
            else
            {
                LogWarning($"Fallback font: {fontAsset.name} already exists.");
            }
        }

        private string GetSystemFontFile(string fontName)
        {
            string fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontName);
            if (!File.Exists(fontFilePath))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                fontFilePath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "Fonts", fontName);
            }

            return fontFilePath;
        }

        private TMP_FontAsset CreateFont(string fontName)
        {
            var fontFilePath = Path.Combine(AssetsDir, "Fonts", fontName);
            if (!File.Exists(fontFilePath))
            {
                // 获取系统字体文件路径
                fontFilePath = GetSystemFontFile(fontName);
            }

            Font font;
            if (File.Exists(fontFilePath))
            {
                LogMessage($"fontFilePath: {fontFilePath}");

                // 加载字体
                font = new Font(fontFilePath);
            }
            else
            {
                LogError($"Failed to find font file: {fontFilePath}, try to load font '{fontName}' from game resources.");

                font = Resources.Load<Font>(fontName);
                if (font == null)
                    font = Resources.GetBuiltinResource<Font>(fontName);
            }

            // Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            // Font font = Resources.Load<Font>("Fonts/Zpix");
            
            if (font == null)
            {
                LogError($"Failed to load font: {fontName}");
                return null;
            }

            // 创建 TMP 字体资产
            var fontAsset = TMP_FontAsset.CreateFontAsset(font, 48, 3, GlyphRenderMode.SDFAA, 1024, 1024);
            if (fontAsset == null)
                LogError($"Failed to create font.");

            return fontAsset;
        }

        public void TryAddCharacters(string fontName, string characters)
        {
            var font = GetFont(fontName);
            if (font == null)
            {
                LogError($"Failed to get font: {fontName}");
                return;
            }

            TryAddCharacters(font, characters);
        }

        public void TryAddCharacters(TMP_FontAsset fontAsset, string characters)
        {
            LogMessage($"TryAddCharacters: {characters}, ThreadId: {Thread.CurrentThread.ManagedThreadId}, {Thread.CurrentThread.Name}");
            if (fontAsset == null)
            {
                LogError("fontAsset is null.");
                return;
            }

            if (string.IsNullOrEmpty(characters))
                return;

            bool result = fontAsset.TryAddCharacters(characters, out var missingCharacters, false);
            if (result)
            {
                LogMessage($"successfully added characters: {characters}");
            }
            else
            {
                LogError($"failed to add characters: {characters}, missing characters: {missingCharacters}");
            }
        }

        private static void TransferCharacters(TMP_FontAsset source, TMP_FontAsset target)
        {
            LogMessage($"TransferCharacters, ThreadId: {Thread.CurrentThread.ManagedThreadId}, {Thread.CurrentThread.Name}");
            // 确保源和目标字体资产都有效
            if (source == null || target == null)
            {
                LogError("Source or Target font asset is null.");
                return;
            }

            LogMessage($"TransferCharacters 1, ThreadId: {Thread.CurrentThread.ManagedThreadId}, {Thread.CurrentThread.Name}");

            // 创建一个 Unicode 列表来存储源字体的字符
            uint[] unicodes = new uint[source.characterTable.Count];

            // 遍历源字体的字符表并获取每个字符的 Unicode 值
            for (int i = 0; i < source.characterTable.Count; i++)
            {
                var unicode = source.characterTable[i].unicode;
                unicodes[i] = unicode;
            }

            var stringBuilder = new StringBuilder();

            foreach (var unicode in unicodes)
            {
                var character = char.ConvertFromUtf32((int)unicode);
                stringBuilder.Append(unicode.ToString("X4"));
                stringBuilder.Append(':');
                stringBuilder.Append(character);
                stringBuilder.Append(", ");
            }

            string result = stringBuilder.ToString();
            LogWarning($"TryAddCharacters: {result}");

            // 尝试将字符添加到目标字体资产
            if (target.TryAddCharacters(unicodes, out uint[] missingUnicodes))
            {
                LogMessage($"Characters added successfully to target font asset. Count: {unicodes.Length}");
            }
            else
            {
                stringBuilder = new StringBuilder();

                foreach (var unicode in missingUnicodes)
                {
                    var character = char.ConvertFromUtf32((int)unicode);
                    stringBuilder.Append(unicode.ToString("X4"));
                    stringBuilder.Append(':');
                    stringBuilder.Append(character);
                    stringBuilder.Append(", ");
                }

                result = stringBuilder.ToString();
                LogWarning($"Some characters could not be added: {result}");
            }
        }
    }
}