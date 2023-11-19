using Microsoft.Xna.Framework.Audio;
using System.Reflection.Emit;
using System.Reflection;

namespace ModBagman.HarmonyPatches;

[HarmonyPatch(typeof(SoundSystem), nameof(SoundSystem.PlayInterfaceCue))]
static class PlayInterfaceCue
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code, ILGenerator gen)
        => PlayCue.Transpiler(code, gen);
}
