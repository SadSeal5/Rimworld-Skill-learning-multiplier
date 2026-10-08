using HarmonyLib;
using RimWorld;

namespace SkillLearningMultiplier
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Interval))]
    public static class Patch_SkillRecord_Interval
    {
        public static bool Prefix(SkillRecord __instance)
        {
            if (SkillMultiplierMod.Settings != null && SkillMultiplierMod.Settings.disableSkillDecay)
            {
                if (__instance != null && SkillMultiplierMod.Settings.ShouldApplyTo(__instance.Pawn))
                {
                    // When disableSkillDecay is true and pawn qualifies, skip the interval decay.
                    return false;
                }
            }
            return true;
        }
    }
}
