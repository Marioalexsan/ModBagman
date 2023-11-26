using Microsoft.Xna.Framework.Graphics;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(Game1), nameof(Game1._RogueLike_GetPerkTexture))]
static class _RogueLike_GetPerkTexture
{
    static bool Prefix(RogueLikeMode.Perks enPerk, ref Texture2D __result)
    {
        var entry = Entries.Perks.Get(enPerk);

        if (entry == null)
        {
            __result = ModBagmanResources.NullTexture;
            return false;
        }

        if (entry.TexturePath == null && entry.IsVanilla)
            return true;

        __result = Globals.Game.Content.TryLoadWithModSupport<Texture2D>(entry.TexturePath);
        return false;
    }
}
