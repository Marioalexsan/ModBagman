namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="SoG.ItemCodex"/>.
/// </summary>
[HarmonyPatch]
public static class Original_ItemCodex
{
    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription_PostSpecial))]
    public static ItemDescription GetItemDescription_PostSpecial(ItemCodex.ItemTypes enType)
    {
        throw new NotImplementedException("Stub method.");
    }

    /// <!-- nodoc -->
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription))]
    public static ItemDescription GetItemDescription(ItemCodex.ItemTypes enType)
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original = AccessTools.Method(typeof(ItemCodex), nameof(ItemCodex.GetItemDescription_PostSpecial));
            var replacement = AccessTools.Method(typeof(Original_ItemCodex), nameof(GetItemDescription_PostSpecial));

            return instructions.MethodReplacer(original, replacement);
        }

        _ = Transpiler(null);
        throw new NotImplementedException("Stub method.");
    }
}
