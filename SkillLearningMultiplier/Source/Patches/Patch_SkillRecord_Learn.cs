using HarmonyLib;
using RimWorld;

namespace SkillLearningMultiplier
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
    public static class Patch_SkillRecord_Learn
    {
        public static void Prefix(SkillRecord __instance, ref float xp, bool direct, bool ignoreLearnRate)
        {
            if (xp > 0f && __instance != null && __instance.def != null && SkillMultiplierMod.Settings != null)
            {
                if (!SkillMultiplierMod.Settings.ShouldApplyTo(__instance.Pawn))
                {
                    return;
                }

                float multiplier = SkillMultiplierMod.Settings.GetMultiplierForSkill(__instance.def);
                xp *= multiplier;
            }
        }
    }
}
