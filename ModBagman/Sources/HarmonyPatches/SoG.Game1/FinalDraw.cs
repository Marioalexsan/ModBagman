using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1.FinalDraw))]
static class FinalDraw
{
    static void Prefix()
    {
        SpriteBatch spriteBatch = Globals.SpriteBatch;

        Globals.Game.GraphicsDevice.SetRenderTarget(RenderMaster.rt2dMasterTarget);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null);

        Globals.Console?.Render(spriteBatch);

        spriteBatch.End();
    }
}
