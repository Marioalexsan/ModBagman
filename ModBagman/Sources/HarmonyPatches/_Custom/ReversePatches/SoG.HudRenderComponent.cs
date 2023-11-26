using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="HudRenderComponent"/>.
/// </summary>
[HarmonyPatch]
public static class Original_HudRenderComponent
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.GetBuffTexture))]
    public static Texture2D GetBuffTexture(HudRenderComponent __instance, BaseStats.StatusEffectSource en)
    {
        throw new NotImplementedException("Stub method.");
    }
}
