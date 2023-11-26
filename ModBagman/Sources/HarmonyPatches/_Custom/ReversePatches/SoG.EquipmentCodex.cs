namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SoG.CardCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_EquipmentCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetAccessoryInfo))]
    public static EquipmentInfo GetAccessoryInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetArmorInfo))]
    public static EquipmentInfo GetArmorInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShieldInfo))]
    public static EquipmentInfo GetShieldInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EquipmentCodex), nameof(EquipmentCodex.GetShoesInfo))]
    public static EquipmentInfo GetShoesInfo(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }
}
