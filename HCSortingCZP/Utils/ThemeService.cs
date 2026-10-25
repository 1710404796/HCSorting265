using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HCSortingCZP.Utils
{
    public static class ThemeService
    {
        public const string LightBlue = "LightBlue";
        public const string BlueTech = "BlueTech";

        private static readonly string[] OrderedThemes =
        [
            LightBlue,
            BlueTech
        ];

        private static readonly IReadOnlyDictionary<string, string> ThemeDisplayNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [LightBlue] = "浅蓝色",
                [BlueTech] = "深蓝科技"
            };

        public static string CurrentTheme { get; private set; } = LightBlue;

        public static void Apply(string? themeName)
        {
            var normalizedTheme = IsKnownTheme(themeName) ? themeName! : LightBlue;
            var application = Application.Current;
            if (application == null)
            {
                CurrentTheme = normalizedTheme;
                return;
            }

            var oldPalettes = application.Resources.MergedDictionaries
                .Where(dictionary =>
                    dictionary.Source != null &&
                    dictionary.Source.OriginalString.Contains(
                        "ThemePalette.",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var oldPalette in oldPalettes)
            {
                application.Resources.MergedDictionaries.Remove(oldPalette);
            }

            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        $"Theme/ThemePalette.{normalizedTheme}.xaml",
                        UriKind.Relative)
                });

            CurrentTheme = normalizedTheme;
        }

        public static string GetNextTheme()
        {
            var currentIndex = System.Array.IndexOf(OrderedThemes, CurrentTheme);
            var nextIndex = currentIndex < 0
                ? 0
                : (currentIndex + 1) % OrderedThemes.Length;

            return OrderedThemes[nextIndex];
        }

        public static string GetDisplayName(string? themeName)
        {
            if (themeName != null &&
                ThemeDisplayNames.TryGetValue(themeName, out var displayName))
            {
                return displayName;
            }

            return themeName ?? LightBlue;
        }

        private static bool IsKnownTheme(string? themeName)
        {
            return themeName != null &&
                   OrderedThemes.Contains(themeName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
