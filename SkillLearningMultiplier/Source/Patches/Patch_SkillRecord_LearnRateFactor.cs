using HarmonyLib;
using RimWorld;

namespace SkillLearningMultiplier
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.LearnRateFactor))]
    public static class Patch_SkillRecord_LearnRateFactor
    {
        public static void Postfix(SkillRecord __instance, bool direct, ref float __result)
        {
            if (SkillMultiplierMod.Settings != null && SkillMultiplierMod.Settings.disableDailyCap && !direct)
            {
                if (__instance != null && SkillMultiplierMod.Settings.ShouldApplyTo(__instance.Pawn) && __instance.LearningSaturatedToday)
                {
                    // Vanilla applies a 0.2f factor when LearningSaturatedToday (>4000 daily xp) is true.
                    // Dividing by 0.2f removes this penalty.
                    __result /= 0.2f;
                }
            }
        }
    }
}
