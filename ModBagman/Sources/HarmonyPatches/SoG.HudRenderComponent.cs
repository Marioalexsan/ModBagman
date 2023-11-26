using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(HudRenderComponent))]
static class SoG_HudRenderComponent
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudRenderComponent.RenderTopGUI))]
    static void RenderTopGUI_Postfix(SpriteBatch spriteBatch)
    {
        Mod.PostRenderTopGUIData data = new()
        {
            SpriteBatch = spriteBatch
        };

        foreach (Mod mod in ModManager.Mods)
            mod.PostRenderTopGUI(data);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HudRenderComponent.GetBuffTexture))]
    static bool GetBuffTexture_Prefix(ref Texture2D __result, BaseStats.StatusEffectSource en)
    {
        var entry = Entries.StatusEffects.GetRequired(en);

        if (entry.TextureLoader != null)
        {
            __result = entry.TextureLoader.Invoke() ?? RenderMaster.txNullTex;
        }
        else
        {
            __result = Globals.Game.Content.TryLoadWithModSupport<Texture2D>(entry.TexturePath, true);
        }

        return false;
    }
}
