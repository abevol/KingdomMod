using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using static KingdomMod.OverlayMap.OverlayMapHolder;

#if IL2CPP
using UnityEngine.Bindings;
using Il2CppInterop.Runtime;
#endif

namespace KingdomMod.OverlayMap.Assets
{
    public class FontManager
    {
        private static string GetSystemFontFile(string fontName)
        {
            string fontFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontName);
            if (!File.Exists(fontFilePath))
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                fontFilePath = Path.Combine(userProfile, "AppData", "Local", "Microsoft", "Windows", "Fonts", fontName);
            }

            return fontFilePath;
        }

        public static FontData CreateMainFont(string fontName, FontData lastFont)
        {
            var newFont = CreateMainFont(fontName);
            if (newFont == null)
            {
                LogError($"Failed to create font: {fontName}");
                return null;
            }

            // AddChineseCharactersToFont(newFont);

            if (lastFont != null)
                newFont.TryAddCharacters(new string(lastFont.Chars.ToArray()));

            return newFont;
        }

        private static FontData CreateMainFont(string fontName)
        {
            var sourceFont = CreateSourceFont(fontName);
            if (sourceFont == null)
            {
                LogError($"Failed to create source font: {fontName}");
                return null;
            }

            var font = CreateFontAsset(sourceFont);
            if (font == null)
            {
                LogError($"Failed to create font: {fontName}");
                return null;
            }

            return new FontData {FontName = fontName, Font = font, SourceFont = sourceFont, Chars = new HashSet<char>(), MissingChars = new HashSet<char>() };
        }

        /// <summary>
        /// 向字体添加中文字符
        /// </summary>
        private static void AddChineseCharactersToFont(FontData fontData)
        {
            if (fontData == null) return;

            // 添加信息显示中可能用到的中文字符
            var infoChars = "城堡列表字体调试面板热键切换显示数字键切换字体方向键调整参数当前字体大小颜色对齐样式富文本换行行间距字符间距白色红色绿色蓝色黄色青色洋红左中右左上上右上左中中右中左下下右下普通粗体斜体粗斜体开关未知自定义";
            fontData.TryAddCharacters(infoChars);

            // 添加一些常用的中文字符
            var commonChineseChars = "的一是了我不人在他有这个上们来到时大地为子中你说生国年着就那和要她出也得里后自以会家可下而过天去能对小多然于心学么之都好看起发当没成只如事把还用第样道想作种开美总从无情面最女但现前些所同日手又行意动方期它头经长儿回位分爱老因很给名法间斯知世什两次使身者被高已亲其进此话常与活正感";
            fontData.TryAddCharacters(commonChineseChars);

            LogTrace($"Added Chinese characters to font: {fontData.FontName}");
        }

        public static Font CreateSourceFont(string fontName)
        {
            Font font;

            font = Resources.Load<Font>(fontName);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>(fontName);

            if (font == null)
            {
                LogTrace($"Failed to find font in Resources: {fontName}.");

                var fontFilePath = Path.Combine(AssetsDir, "Fonts", fontName);
                if (!File.Exists(fontFilePath))
                {
                    fontFilePath = GetSystemFontFile(fontName);
                }
                if (File.Exists(fontFilePath))
                {
                    LogTrace($"fontFilePath: {fontFilePath}");

#if IL2CPP
                    font = new Font();
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(fontFilePath)))
                        Internal_CreateFont(font, fontFilePath);
                    else
                        Internal_CreateFontFromPath(font, fontFilePath);
#else
                    font = new Font(fontFilePath);
#endif
                }
                else
                {
                    LogError($"Failed to find font file: {fontFilePath}.");
                }
            }

            if (font == null)
            {
                LogError($"Failed to load font: {fontName}");
                return null;
            }

            UnityEngine.Object.DontDestroyOnLoad(font);
            font.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return font;
        }

        public static TMP_FontAsset CreateFontAsset(Font font)
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(font, 48, 3, GlyphRenderMode.SDFAA, 1024, 1024);
            if (fontAsset == null)
            {
                LogError($"Failed to create font.");
                return null;
            }

            UnityEngine.Object.DontDestroyOnLoad(fontAsset);
            fontAsset.sourceFontFile.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            fontAsset.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

            LogTrace($"Font Created: {fontAsset.faceInfo.familyName}, '{fontAsset.sourceFontFile == null}' '{!fontAsset.sourceFontFile}', sourceFontFile: {fontAsset.sourceFontFile.name}");
            return fontAsset;
        }

#if IL2CPP
        private static unsafe void Internal_CreateFont(Font self, string name)
        {
            try
            {
                ManagedSpanWrapper managedSpanWrapper;
                if (string.IsNullOrEmpty(name))
                {
                    managedSpanWrapper = default;
                    Font.Internal_CreateFont_Injected(self, ref managedSpanWrapper);
                }
                else
                {
                    fixed (char* ptr = name)
                    {
                        managedSpanWrapper = new ManagedSpanWrapper((void*)ptr, name.Length);
                        Font.Internal_CreateFont_Injected(self, ref managedSpanWrapper);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Internal_CreateFont failed: {ex}");
            }
        }

        private static unsafe void Internal_CreateFontFromPath(Font self, string fontPath)
        {
            try
            {
                ManagedSpanWrapper managedSpanWrapper;
                if (string.IsNullOrEmpty(fontPath))
                {
                    managedSpanWrapper = default;
                    Font.Internal_CreateFontFromPath_Injected(self, ref managedSpanWrapper);
                }
                else
                {
                    fixed (char* ptr = fontPath)
                    {
                        managedSpanWrapper = new ManagedSpanWrapper((void*)ptr, fontPath.Length);
                        Font.Internal_CreateFontFromPath_Injected(self, ref managedSpanWrapper);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Internal_CreateFontFromPath failed: {ex}");
            }
        }
#endif
    }
}
