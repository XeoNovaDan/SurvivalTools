using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class StatWorker_EstimatedLifespan : StatWorker
    {

        public static float BaseWearInterval =>
            GenDate.TicksPerHour * ((SurvivalToolsSettings.hardcoreMode) ? 0.333f : 0.5f); // Once per 30 mins of continuous work, or 20 mins with hardcore

        public override bool ShouldShowFor(StatRequest req) =>
            req.Def.IsSurvivalTool();

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            SurvivalTool tool = req.Thing as SurvivalTool;
            return GetBaseEstimatedLifespan(tool, req.Def);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            SurvivalTool tool = req.Thing as SurvivalTool;
            return $"{"StatsReport_BaseValue".Translate()}: {GetBaseEstimatedLifespan(tool, req.Def).ToString("F1")}";
        }

        private float GetBaseEstimatedLifespan(SurvivalTool tool, BuildableDef def)
        {
            SurvivalToolProperties props = def.GetModExtension<SurvivalToolProperties>();

            // For def
            if (tool == null)
                return GenDate.TicksToDays(Mathf.RoundToInt((BaseWearInterval * def.GetStatValueAbstract(StatDefOf.MaxHitPoints)) / props.toolWearFactor));

            // For thing
            float wearFactor = tool.ToolProps.toolWearFactor * (tool.StuffProps?.wearFactorMultiplier ?? 1f);
            return GenDate.TicksToDays(Mathf.RoundToInt((BaseWearInterval * tool.GetStatValue(StatDefOf.MaxHitPoints)) / wearFactor));
        }

    }
}
