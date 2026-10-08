using HarmonyLib;
using UnityEngine;
using Verse;

namespace SkillLearningMultiplier
{
    public class SkillMultiplierMod : Mod
    {
        public static SkillMultiplierSettings Settings { get; private set; }

        public SkillMultiplierMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SkillMultiplierSettings>();
            var harmony = new Harmony("ramogay.skilllearningmultiplier");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "SLM_ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SettingsWindowDrawer.DrawSettings(inRect, Settings);
        }
    }
}
