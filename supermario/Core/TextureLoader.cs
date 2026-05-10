using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace supermario
{
    internal static class TextureLoader
    {
        private static readonly Dictionary<string, string> SheetFiles = new Dictionary<string, string>
        {
            { "player", "player_sheet.png" },
            { "enemies", "enemies_sheet.png" },
            { "items", "items_sheet.png" },
            { "blocks", "blocks_sheet.png" },
            { "bg", "world_bg.png" },
        };

        public static readonly Dictionary<string, Image> Sheets = new Dictionary<string, Image>();

        private static bool _loaded;

        public static void LoadAll()
        {
            if (_loaded) return;

            string sheetDirectory = FindSheetDirectory();
            foreach (var sheet in SheetFiles)
            {
                Sheets[sheet.Key] = Image.FromFile(Path.Combine(sheetDirectory, sheet.Value));
            }

            _loaded = true;
        }

        public static bool TryGetSheet(string key, out Image sheet)
        {
            return Sheets.TryGetValue(key, out sheet);
        }

        private static string FindSheetDirectory()
        {
            const string assetRoot = "assets";
            const string textureRoot = "textures";
            const string sheetRoot = "sprite_sheets";

            foreach (string root in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(root);
                while (directory != null)
                {
                    string candidate = Path.Combine(directory.FullName, assetRoot, textureRoot, sheetRoot);
                    if (Directory.Exists(candidate)) return candidate;

                    directory = directory.Parent;
                }
            }

            throw new DirectoryNotFoundException("Could not find assets/textures/sprite_sheets.");
        }
    }
}
