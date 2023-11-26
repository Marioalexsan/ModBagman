namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="WeaponCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_WeaponCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(WeaponCodex), nameof(WeaponCodex.GetWeaponInfo))]
    public static WeaponInfo GetWeaponInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
