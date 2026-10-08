using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SkillLearningMultiplier
{
    public static class SettingsWindowDrawer
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private static readonly Dictionary<string, string> textBuffers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string globalBuffer = null;

        public static void ClearBuffers()
        {
            textBuffers.Clear();
            globalBuffer = null;
        }

        public static void DrawSettings(Rect inRect, SkillMultiplierSettings settings)
        {
            var skills = DefDatabase<SkillDef>.AllDefsListForReading;
            int skillCount = skills != null ? skills.Count : 0;
            float totalHeight = 380f + (skillCount * 38f);

            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 24f, totalHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            // General & QoL Section
            Text.Font = GameFont.Medium;
            listing.Label("SLM_GeneralSettings".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled(
                "SLM_OnlyColonists".Translate(),
                ref settings.onlyColonists,
                "SLM_OnlyColonists_Desc".Translate()
            );

            listing.CheckboxLabeled(
                "SLM_DisableDailyCap".Translate(),
                ref settings.disableDailyCap,
                "SLM_DisableDailyCap_Desc".Translate()
            );

            listing.CheckboxLabeled(
                "SLM_DisableSkillDecay".Translate(),
                ref settings.disableSkillDecay,
                "SLM_DisableSkillDecay_Desc".Translate()
            );

            listing.CheckboxLabeled(
                "SLM_UseGlobalMultiplier".Translate(),
                ref settings.useGlobalMultiplier,
                "SLM_UseGlobalMultiplier_Desc".Translate()
            );

            listing.Gap(10f);

            // Preset Buttons Section
            Text.Font = GameFont.Medium;
            listing.Label("SLM_Presets".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            Rect presetRowRect = listing.GetRect(30f);
            float[] presetValues = { 1f, 2f, 5f, 10f, 25f, 50f, 100f };
            string[] presetLabels = { "1x", "2x", "5x", "10x", "25x", "50x", "100x" };
            float buttonSpacing = 6f;
            float buttonWidth = (presetRowRect.width - (buttonSpacing * 7)) / 8f;

            for (int i = 0; i < presetValues.Length; i++)
            {
                Rect btnRect = new Rect(presetRowRect.x + i * (buttonWidth + buttonSpacing), presetRowRect.y, buttonWidth, 30f);
                if (Widgets.ButtonText(btnRect, presetLabels[i]))
                {
                    settings.SetAllMultipliers(presetValues[i]);
                    ClearBuffers();
                }
            }

            Rect resetBtnRect = new Rect(presetRowRect.x + presetValues.Length * (buttonWidth + buttonSpacing), presetRowRect.y, buttonWidth, 30f);
            if (Widgets.ButtonText(resetBtnRect, "SLM_ResetDefaults".Translate()))
            {
                settings.ResetToDefaults();
                ClearBuffers();
            }

            listing.Gap(14f);

            // Global Multiplier Row
            Text.Font = GameFont.Medium;
            listing.Label("SLM_GlobalMultiplier".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            Rect globalRowRect = listing.GetRect(32f);
            DrawMultiplierRow(globalRowRect, "SLM_GlobalMultiplier".Translate(), settings.globalMultiplier, ref globalBuffer, newVal =>
            {
                settings.globalMultiplier = newVal;
            });

            listing.Gap(14f);

            // Individual Multipliers Section
            Text.Font = GameFont.Medium;
            listing.Label("SLM_IndividualMultipliers".Translate());
            Text.Font = GameFont.Small;

            if (settings.useGlobalMultiplier)
            {
                GUI.color = Color.yellow;
                listing.Label("SLM_IndividualNotice".Translate());
                GUI.color = Color.white;
            }

            listing.Gap(6f);

            if (skills != null)
            {
                var sortedSkills = skills.OrderBy(s => s.listOrder).ToList();
                foreach (SkillDef skill in sortedSkills)
                {
                    if (skill == null) continue;

                    string key = skill.defName;
                    float currentVal = settings.GetSkillMultiplier(key);
                    string buffer = textBuffers.TryGetValue(key, out string b) ? b : null;

                    Rect rowRect = listing.GetRect(32f);
                    string label = !string.IsNullOrEmpty(skill.label) ? skill.LabelCap.Resolve() : skill.defName;

                    DrawMultiplierRow(rowRect, label, currentVal, ref buffer, newVal =>
                    {
                        settings.SetSkillMultiplier(key, newVal);
                    });

                    textBuffers[key] = buffer;
                    listing.Gap(4f);
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawMultiplierRow(Rect rowRect, string label, float currentValue, ref string buffer, Action<float> onChanged)
        {
            float labelWidth = 180f;
            float textWidth = 65f;
            float sliderWidth = rowRect.width - labelWidth - textWidth - 20f;

            // Label
            Rect labelRect = new Rect(rowRect.x, rowRect.y, labelWidth, rowRect.height);
            Widgets.Label(labelRect, label);

            // Slider
            Rect sliderRect = new Rect(rowRect.x + labelWidth + 5f, rowRect.y + 4f, sliderWidth, rowRect.height - 8f);
            float newSliderValue = Widgets.HorizontalSlider(sliderRect, currentValue, 0f, 100f, false, null, "0x", "100x", 0.05f);

            if (Math.Abs(newSliderValue - currentValue) > 0.001f)
            {
                float rounded = Mathf.Round(newSliderValue * 100f) / 100f;
                buffer = rounded.ToString("0.##", CultureInfo.InvariantCulture);
                onChanged(rounded);
            }

            // Text Input Field
            Rect textRect = new Rect(rowRect.x + labelWidth + sliderWidth + 15f, rowRect.y + 2f, textWidth, 26f);
            if (buffer == null)
            {
                buffer = currentValue.ToString("0.##", CultureInfo.InvariantCulture);
            }

            string newText = Widgets.TextField(textRect, buffer);
            if (newText != buffer)
            {
                buffer = newText;
                if (float.TryParse(newText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ||
                    float.TryParse(newText, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
                {
                    float clamped = Mathf.Clamp(parsed, 0f, 100f);
                    onChanged(clamped);
                }
            }
        }
    }
}
