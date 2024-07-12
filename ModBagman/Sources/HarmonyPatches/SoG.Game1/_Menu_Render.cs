using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._Menu_Render))]
static class _Menu_Render
{
    static void Postfix()
    {
        SpriteBatch spriteBatch = Globals.SpriteBatch;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, null);

        if (Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == MainMenuWorker.ReservedModMenuID)
            MainMenuWorker.MenuRender();

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null);
    }

}
