namespace ModBagman.HarmonyPatches;

/// <summary>
/// Reverse patches for <see cref="LevelBlueprint"/>.
/// </summary>
[HarmonyPatch]
public static class Original_LevelBlueprint
{
    /// <!-- nodoc -->
    [HarmonyPatch(typeof(LevelBlueprint), nameof(LevelBlueprint.GetBlueprint))]
    public static LevelBlueprint GetBlueprint(Level.ZoneEnum enZoneToGet)
    {
        throw new NotImplementedException("Stub method.");
    }
}
