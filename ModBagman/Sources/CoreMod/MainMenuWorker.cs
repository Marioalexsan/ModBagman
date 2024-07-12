using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static System.Net.Mime.MediaTypeNames;

namespace ModBagman;

/// <summary>
/// Handles callbacks for Main Menu patches.
/// This includes save mod compatibility information, and the mod menu.
/// </summary>
internal static class MainMenuWorker
{
    private static class ModMenu
    {
        public static int Selection
        {
            get => _selection;
            set => _selection = Math.Min(Math.Max(value, MinSelection), MaxSelection);
        }
        private static int _selection = 0;

        public const int MinSelection = 0;
        public const int MaxSelection = 1;
    }

    private static class ModListMenu
    {
        private static int SanitizeSelection(int value) => Math.Min(Math.Max(value, 0), ModManager.Mods.Where(x => x.Name != VanillaMod.ModName).Count() - 1);
        private static int RestrictScroll(int value) => Math.Min(Math.Max(value, Selection - MaxItems + 1), Selection);

        public static int ScrollStart
        {
            get => _scrollStart = RestrictScroll(_scrollStart);
            set => _scrollStart = RestrictScroll(value);
        }
        private static int _scrollStart = 0;

        public static int Selection
        {
            get => _selection = SanitizeSelection(_selection);
            set => _selection = SanitizeSelection(value);
        }
        private static int _selection = 0;

        public const int MaxItems = 8;
    }

    public static readonly GlobalData.MainMenu.MenuLevel ReservedModMenuID = (GlobalData.MainMenu.MenuLevel)300;

    private static Dictionary<int, SaveCompatibility> _storySaves = new();

    private static SaveCompatibility _arcadeSave;

    private static int _previousTopMenuSelection;

    private static int CurrentSubMenu = 0;

    public static void UpdateStorySaveCompatibility()
    {
        for (int index = 0; index < Globals.Game.xGlobalData.lxCharacterSaves.Count; index++)
        {
            var save = Globals.Game.xGlobalData.lxCharacterSaves[index];

            if (save.bIncompatibleSave || save.sCharacterName == "")
            {
                continue;
            }

            string path = $"{Globals.Game.sAppData}Characters/{index}.cha{ModSaving.SaveFileExtension}";

            if (File.Exists(path))
            {
                using (FileStream stream = new(path, FileMode.Open, FileAccess.Read))
                {
                    _storySaves[index] = ModSaving.CheckCompatibility(stream);
                }
            }
        }
    }

    public static void UpdateArcadeSaveCompatibility()
    {
        if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile)
        {
            _arcadeSave = null;
            return;
        }

        string appData = Globals.Game.sAppData;

        string path = appData + $"arcademode.sav{ModSaving.SaveFileExtension}";

        if (File.Exists(path))
        {
            using (FileStream stream = new(path, FileMode.Open, FileAccess.Read))
            {
                _arcadeSave = ModSaving.CheckCompatibility(stream);
            }
        }
    }

    public static void CheckStorySaveCompatibility()
    {
        var menuData = Globals.Game.xGlobalData.xMainMenuData;

        int slot = menuData.iSelectedChar + menuData.iCurrentCharSelectPage * 3;
        //float alpha = menuData.fCurrentMenuAlpha;

        if (slot < 0 || slot > 8)
            return;

        if (!_storySaves.ContainsKey(slot))
        {
            RenderMessage("No data available (vanilla save?).", 444, 90 + 65);
        }
        else
        {
            RenderMessage(_storySaves[slot].GetCompatibilityText(), 444, 90 + 65);
        }
    }

    public static void CheckArcadeSaveCompatiblity()
    {
        if (Globals.Game.xGlobalData.xMainMenuData.iTopMenuSelection != 1)
            return;

        if (_arcadeSave == null)
        {
            RenderMessage("No data available (vanilla save?).", 422, 243);
        }
        else
        {
            RenderMessage(_arcadeSave.GetCompatibilityText(), 422, 243);
        }
    }

    public static void RenderMessage(string message, int x, int y)
    {
        float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;

        Vector2 measure = FontManager.GetFont(FontManager.FontType.Reg7).MeasureString(message);

        Globals.Game._Menu_RenderNotice(Globals.SpriteBatch, 1f, new Rectangle(x - 4, y - (int)measure.Y / 2 - 4, (int)measure.X + 8, (int)measure.Y + 8), false);

        Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), message, new Vector2(x, y - (int)measure.Y / 2), Color.White * alpha);
    }

    public static void PostTopMenuInterface()
    {
        var menuData = Globals.Game.xGlobalData.xMainMenuData;
        var inputData = Globals.Game.xInput_Menu;
        var audio = Globals.Game.xSoundSystem;

        if (inputData.Left.bPressed && menuData.iTopMenuSelection != 4)
        {
            _previousTopMenuSelection = menuData.iTopMenuSelection;
            menuData.iTopMenuSelection = 4;
            audio.PlayInterfaceCue("Menu_Move");
        }
        else if (inputData.Right.bPressed && menuData.iTopMenuSelection == 4)
        {
            menuData.iTopMenuSelection = _previousTopMenuSelection;
            audio.PlayInterfaceCue("Menu_Move");
        }

        if (inputData.Action.bPressed && menuData.iTopMenuSelection == 4)
        {
            audio.PlayInterfaceCue("Menu_Changed");

            // TODO: Lol
            Globals.Game.xGlobalData.xMainMenuData.Transition(ReservedModMenuID);
        }
    }

    public static void RenderModMenuButton()
    {
        var menuData = Globals.Game.xGlobalData.xMainMenuData;
        var spriteBatch = Globals.SpriteBatch;
        float alpha = menuData.fCurrentMenuAlpha;

        Globals.Game._Menu_RenderContentBox(spriteBatch, alpha, new Rectangle(100, 219, 121, 56));

        Color selected = Color.White;
        Color notSelected = Color.Gray * 0.8f;

        var texture = ModBagmanResources.ModMenu;
        Vector2 center = new(texture.Width / 2, texture.Height / 2);

        Color colorToUse = menuData.iTopMenuSelection == 4 ? selected : notSelected;

        spriteBatch.Draw(texture, new Vector2(160 - center.X, 245 - center.Y), null, colorToUse * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public static void MenuInterface()
    {
        if (Globals.Game.xGlobalData.xMainMenuData.enTargetMenuLevel != GlobalData.MainMenu.MenuLevel.Null)
        {
            return;
        }

        if (Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == ReservedModMenuID)
        {
            if (CurrentSubMenu == 0)
                ModMenuInterface();

            else if (CurrentSubMenu == 1)
                ModListInterface();
        }
    }

    public static void MenuRender()
    {
        if (Globals.Game.xGlobalData.xMainMenuData.enTargetMenuLevel != GlobalData.MainMenu.MenuLevel.Null)
        {
            return;
        }

        if (Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == ReservedModMenuID)
        {
            if (CurrentSubMenu == 0)
                ModMenuRender();

            else if (CurrentSubMenu == 1)
                ModListRender();
        }
    }

    private static void ModMenuInterface()
    {
        var input = Globals.Game.xInput_Menu;

        var previousSelection = ModMenu.Selection;

        if (input.Down.bPressed)
        {
            ModMenu.Selection++;
        }
        else if (input.Up.bPressed)
        {
            ModMenu.Selection--;
        }

        if (previousSelection != ModMenu.Selection)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Move");
        }

        if (input.Action.bPressed)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Change");

            switch (ModMenu.Selection)
            {
                case 0:
                    ModManager.Reload();
                    break;
                case 1:
                    CurrentSubMenu = 1;
                    break;
            }

        }
        else if (input.MenuBack.bPressed)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Cancel");

            Globals.Game.xGlobalData.xMainMenuData.Transition(GlobalData.MainMenu.MenuLevel.TopMenu);
        }
    }

    private static void ModMenuRender()
    {
        float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;
        Color selected = Color.White;
        Color notSelected = Color.Gray * 0.8f;

        SpriteBatch spriteBatch = Globals.SpriteBatch;

        Globals.Game._Menu_RenderContentBox(spriteBatch, alpha, new Rectangle(235, 189, 173, 138));

        Texture2D reloadModsTex = ModBagmanResources.ReloadMods;
        Vector2 reloadModsCenter = new(reloadModsTex.Width / 2, reloadModsTex.Height / 2);
        Color reloadModsColor = ModMenu.Selection == 0 ? selected : notSelected;

        spriteBatch.Draw(reloadModsTex, new Vector2(320, 225), null, reloadModsColor, 0f, reloadModsCenter, 1f, SpriteEffects.None, 0f);

        Texture2D modListTex = ModBagmanResources.ModList;
        Vector2 modListCenter = new(modListTex.Width / 2, modListTex.Height / 2);
        Color modListColor = ModMenu.Selection == 1 ? selected : notSelected;

        spriteBatch.Draw(modListTex, new Vector2(320, 251), null, modListColor, 0f, modListCenter, 1f, SpriteEffects.None, 0f);
    }

    private static void ModListInterface()
    {
        var input = Globals.Game.xInput_Menu;

        if (input.Down.bPressed)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Move");
            ModListMenu.Selection++;
        }
        else if (input.Up.bPressed)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Move");
            ModListMenu.Selection--;
        }

        if (input.MenuBack.bPressed)
        {
            Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Cancel");

            CurrentSubMenu = 0;
        }
    }

    private static void ModListRender()
    {
        int x = 320;
        int y = 120;

        var mods = ModManager.Mods.Where(x => x.Name != VanillaMod.ModName).ToList();
        var modView = mods.Skip(ModListMenu.ScrollStart).Take(ModListMenu.MaxItems).ToList();

        float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;

        Vector2 size = new(200, 50);

        foreach (var mod in modView)
        {
            Vector2 measure = FontManager.GetFont(FontManager.FontType.Reg7).MeasureString(mod.Name + " v." + mod.Version.ToString());

            size.X = Math.Max(size.X, measure.X);
            size.Y += measure.Y;
        }

        Globals.Game._Menu_RenderNotice(Globals.SpriteBatch, 1f, new Rectangle(x - 4 - (int)(size.X / 2), y - 4, (int)size.X + 8, (int)size.Y + 8), false);

        int yOffset = 0;

        Vector2 measureTitle = FontManager.GetFont(FontManager.FontType.Bold7Spacing1).MeasureString("Mods loaded:");

        Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Bold7Spacing1), "Mods loaded:", new Vector2(x - (int)(measureTitle.X / 2), y + yOffset), Color.White * alpha * 0.75f);

        yOffset += (int)measureTitle.Y + 10;

        if (ModListMenu.ScrollStart > 0)
        {
            Vector2 measureArrow = FontManager.GetFont(FontManager.FontType.Bold7Spacing1).MeasureString("/\\");

            Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), "/\\", new Vector2(x - (int)(measureArrow.X / 2), y + yOffset), Color.White * alpha * 0.75f);
        }

        yOffset += 10;

        foreach (var mod in modView)
        {
            var text = mod.Name + " v." + mod.Version.ToString();

            Vector2 measure = FontManager.GetFont(FontManager.FontType.Reg7).MeasureString(text);

            Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), text, new Vector2(x - (int)(measure.X / 2), y + yOffset), Color.White * alpha * (mod == mods[ModListMenu.Selection] ? 1f : 0.75f));

            yOffset += (int)measure.Y;
        }

        if (ModListMenu.ScrollStart + ModListMenu.MaxItems < mods.Count)
        {
            Vector2 measureArrow = FontManager.GetFont(FontManager.FontType.Bold7Spacing1).MeasureString("\\/");

            Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), "\\/", new Vector2(x - (int)(measureArrow.X / 2), y + yOffset), Color.White * alpha * 0.75f);
        }
    }
}
