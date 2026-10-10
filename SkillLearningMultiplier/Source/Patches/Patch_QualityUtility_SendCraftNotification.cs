using HarmonyLib;
using RimWorld;
using Verse;

namespace SkillLearningMultiplier.Patches
{
    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.SendCraftNotification))]
    public static class Patch_QualityUtility_SendCraftNotification
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (SkillMultiplierMod.Settings != null && SkillMultiplierMod.Settings.muteMasterworkLegendaryNotifications)
            {
                // Silences the loud letter sound effect and blocks the masterwork/legendary popup letter.
                return false;
            }
            return true;
        }
    }
}
