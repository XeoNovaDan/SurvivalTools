using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace SurvivalTools
{
    [StaticConstructorOnStartup]
    public static class SurvivalToolUtility
    {

        static SurvivalToolUtility()
        {
            // Add validator
            ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.fixedParams.validator = (ThingDef t) =>
            t.techLevel == TechLevel.Neolithic;
        }

        public static readonly FloatRange MapGenToolHitPointsRange = new FloatRange(0.3f, 0.7f);
        public const float MapGenToolMaxStuffMarketValue = 3f;
        public const int SurvivalToolCarryLimit = 3;

        public static List<StatDef> SurvivalToolStats { get; } =
            DefDatabase<StatDef>.AllDefsListForReading.Where(s => s.RequiresSurvivalTool()).ToList();

        public static List<WorkGiverDef> SurvivalToolWorkGivers { get; } =
            DefDatabase<WorkGiverDef>.AllDefsListForReading.Where(w => w.HasModExtension<WorkGiverExtension>()).ToList();

        public static bool RequiresSurvivalTool(this StatDef stat)
        {
            if (!stat.parts.NullOrEmpty())
                foreach (StatPart part in stat.parts)
                    if (part?.GetType() == typeof(StatPart_SurvivalTool))
                        return true;
            return false;
        }

        public static bool IsSurvivalTool(this BuildableDef def, out SurvivalToolProperties toolProps)
        {
            toolProps = def.GetModExtension<SurvivalToolProperties>();
            return def.IsSurvivalTool();
        }

        public static bool IsSurvivalTool(this BuildableDef def) =>
            def is ThingDef tDef && tDef.thingClass == typeof(SurvivalTool) && tDef.HasModExtension<SurvivalToolProperties>();

        public static bool CanUseSurvivalTools(this Pawn pawn) =>
            pawn.RaceProps.intelligence >= Intelligence.ToolUser && (pawn.equipment != null || pawn.inventory != null) && pawn.TraderKind == null;

        public static bool IsUnderSurvivalToolCarryLimit(this int count) =>
            !SurvivalToolsSettings.toolLimit || count < SurvivalToolCarryLimit;

        public static IEnumerable<Thing> GetHeldSurvivalTools(this Pawn pawn) =>
            pawn.inventory?.innerContainer.Where(t => t.def.IsSurvivalTool());

        public static IEnumerable<Thing> GetUsableHeldSurvivalTools(this Pawn pawn)
        {
            List<Thing> heldSurvivalTools = pawn.GetHeldSurvivalTools().ToList();
            if (heldSurvivalTools == null)
                yield break;

            int i = 0;
            while (i.IsUnderSurvivalToolCarryLimit() && i < heldSurvivalTools.Count)
            {
                yield return heldSurvivalTools[i];
                i++;
            }
        }

        public static IEnumerable<Thing> GetAllUsableSurvivalTools(this Pawn pawn) =>
            pawn.equipment?.GetDirectlyHeldThings().Where(t => t.def.IsSurvivalTool()).Concat(pawn.GetUsableHeldSurvivalTools());

        public static Pawn GetPawnFromThingHolder(IThingHolder holder) =>
            (holder is Pawn_EquipmentTracker eq) ? eq.pawn : ((holder is Pawn_InventoryTracker inv) ? inv.pawn : null);

        public static bool CanUseSurvivalTool(this Pawn pawn, SurvivalTool tool)
        {
            foreach (StatModifier modifier in tool.WorkStatFactors)
                if (modifier.stat?.Worker?.IsDisabledFor(pawn) == false)
                    return true;
            return false;
        }

        public static IEnumerable<SurvivalTool> BestSurvivalToolsFor(Pawn pawn)
        {
            foreach (StatDef stat in SurvivalToolStats)
            {
                SurvivalTool tool = pawn.GetBestSurvivalTool(stat);
                if (tool != null)
                {
                    yield return tool;
                }
            }
        }

        public static bool HasSurvivalToolFor(this Pawn pawn, StatDef stat) =>
            pawn.GetBestSurvivalTool(stat) != null;

        public static bool HasSurvivalToolFor(this Pawn pawn, StatDef stat, out SurvivalTool tool, out float statFactor)
        {
            tool = pawn.GetBestSurvivalTool(stat);
            statFactor = tool?.WorkStatFactors.ToList().GetStatFactorFromList(stat) ?? -1f;

            return tool != null;
        }

        public static SurvivalTool GetBestSurvivalTool(this Pawn pawn, StatDef stat)
        {
            SurvivalTool tool = null;
            float statFactor = stat.GetStatPart<StatPart_SurvivalTool>().NoToolStatFactor;

            List<Thing> usableTools = pawn.GetAllUsableSurvivalTools().ToList();
            foreach (SurvivalTool curTool in usableTools)
                foreach (StatModifier modifier in curTool.WorkStatFactors)
                    if (modifier.stat == stat && modifier.value > statFactor)
                    {
                        tool = curTool;
                        statFactor = modifier.value;
                    }

            return tool;
        }

        public static string GetSurvivalToolOverrideReportText(SurvivalTool tool, StatDef stat)
        {
            List<StatModifier> statFactorList = tool.WorkStatFactors.ToList();
            StuffPropsTool stuffPropsTool = tool.StuffProps;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(stat.description);

            builder.AppendLine();
            builder.AppendLine(tool.def.LabelCap + ": " + tool.ToolProps.baseWorkStatFactors.GetStatFactorFromList(stat).ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Factor));

            builder.AppendLine();
            builder.AppendLine(ST_StatDefOf.ToolEffectivenessFactor.LabelCap + ": " +
                tool.GetStatValue(ST_StatDefOf.ToolEffectivenessFactor).ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Factor));

            if (stuffPropsTool != null && stuffPropsTool.toolStatFactors.GetStatFactorFromList(stat) != 1f)
            {
                builder.AppendLine();
                builder.AppendLine("StatsReport_Material".Translate() + " (" + tool.Stuff.LabelCap + "): " +
                    stuffPropsTool.toolStatFactors.GetStatFactorFromList(stat).ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Factor));
            }

            builder.AppendLine();
            builder.AppendLine("StatsReport_FinalValue".Translate() + ": " + statFactorList.GetStatFactorFromList(stat).ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Factor));
            return builder.ToString();
        }
        public static void TryApplyToolWear(SurvivalTool tool)
        {
            if (tool != null && Rand.Chance(tool.WearChancePerTick))
                tool.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
        }

        // A transpiler-friendly version of the above overload
        public static void TryApplyToolWear(Pawn pawn, StatDef stat) =>
            TryApplyToolWear(pawn.GetBestSurvivalTool(stat));
            

        public static IEnumerable<Thing> GetHeldSurvivalTools(this ThingOwner container) =>
            container?.Where(t => t.def.IsSurvivalTool());

        public static int HeldSurvivalToolCount(this Pawn pawn) =>
            pawn.inventory?.innerContainer?.GetHeldSurvivalTools()?.Count() ?? 0;

        public static bool CanCarryAnyMoreSurvivalTools(this Pawn pawn) =>
            (pawn.RaceProps.Humanlike && pawn.HeldSurvivalToolCount().IsUnderSurvivalToolCarryLimit()) || pawn.IsFormingCaravan() || pawn.IsCaravanMember();

        public static bool MeetsWorkGiverStatRequirements(this Pawn pawn, List<StatDef> requiredStats)
        {
            foreach (StatDef stat in requiredStats)
                if (pawn.GetStatValue(stat) <= 0f)
                    return false;
            return true;
        }

    }
}
