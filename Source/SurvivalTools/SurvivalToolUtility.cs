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
            // Add validator to ThingSetMakerDef
            ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.fixedParams.validator = (ThingDef t) =>
            t.IsWithinCategory(ST_ThingCategoryDefOf.SurvivalToolsNeolithic);

            // Remove any unnecessary RecipeDefs when Mending is active
            if (ModCompatibilityCheck.MendAndRecycle)
            {
                Log.Message("Culling redundant MendAndRecycle recipes for Survival Tools...");
                int cullCount = 0;
                bool categoryMatch = false;
                foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(r => r.defName.Contains("SurvivalTool") && r.workerClass != typeof(RecipeWorker)))
                {
                    categoryMatch = false;
                    foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.thingClass == typeof(SurvivalTool)))
                        if (recipe.IsIngredient(thing))
                        {
                            categoryMatch = true;
                            break;
                        }
                    if (!categoryMatch)
                    {
                        recipe.recipeUsers.Clear();
                        cullCount++;
                        Log.Message($"Culled recipe: {recipe.defName}");
                    }
                        
                }
                Log.Message($"Recipe culling complete. Total recipes culled: {cullCount}");
            }
            
            // Add SurvivalToolAssignmentTracker to all appropriate pawns
            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.Humanlike == true))
            {
                Log.Message($"Attaching Pawn_SurvivalToolAssignmentTracker to {tDef}");
                if (tDef.comps == null)
                    tDef.comps = new List<CompProperties>();
                tDef.comps.Add(new CompProperties(typeof(Pawn_SurvivalToolAssignmentTracker)));
            }
                
        }

        public static readonly FloatRange MapGenToolHitPointsRange = new FloatRange(0.3f, 0.7f);
        public const float MapGenToolMaxStuffMarketValue = 3f;

        public static List<StatDef> SurvivalToolStats { get; } =
            DefDatabase<StatDef>.AllDefsListForReading.Where(s => s.RequiresSurvivalTool()).ToList();

        public static List<WorkGiverDef> SurvivalToolWorkGivers { get; } =
            DefDatabase<WorkGiverDef>.AllDefsListForReading.Where(w => w.HasModExtension<WorkGiverExtension>()).ToList();

        // Fields that aren't fields :P
        public static SurvivalToolProperties survivalTool(this ThingDef def) =>
            def.GetModExtension<SurvivalToolProperties>();

        public static StuffPropsTool stuffPropsTool(this ThingDef def) =>
            def.GetModExtension<StuffPropsTool>();

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
            pawn.RaceProps.intelligence >= Intelligence.ToolUser && pawn.Faction == Faction.OfPlayer &&
            (pawn.equipment != null || pawn.inventory != null) && pawn.TraderKind == null;

        public static bool IsUnderSurvivalToolCarryLimitFor(this int count, Pawn pawn) =>
            !SurvivalToolsSettings.toolLimit || count < pawn.GetStatValue(ST_StatDefOf.SurvivalToolCarryCapacity);

        public static IEnumerable<Thing> GetHeldSurvivalTools(this Pawn pawn) =>
            pawn.inventory?.innerContainer.Where(t => t.def.IsSurvivalTool());

        public static IEnumerable<Thing> GetUsableHeldSurvivalTools(this Pawn pawn)
        {
            List<Thing> heldSurvivalTools = pawn.GetHeldSurvivalTools().ToList();
            if (heldSurvivalTools == null)
                yield break;

            int i = 0;
            while (i.IsUnderSurvivalToolCarryLimitFor(pawn) && i < heldSurvivalTools.Count)
            {
                yield return heldSurvivalTools[i];
                i++;
            }
        }

        public static IEnumerable<Thing> GetAllUsableSurvivalTools(this Pawn pawn) =>
            pawn.equipment?.GetDirectlyHeldThings().Where(t => t.def.IsSurvivalTool()).Concat(pawn.GetUsableHeldSurvivalTools());

        public static bool CanUseSurvivalTool(this Pawn pawn, ThingDef def)
        {
            SurvivalToolProperties props = def.GetModExtension<SurvivalToolProperties>();
            if (props == null)
            {
                Log.Error($"Tried to check if {def} is a usable tool but has null tool properties");
                return false;
            }
            foreach (StatModifier modifier in props.baseWorkStatFactors)
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

        public static bool HasSurvivalTool(this Pawn pawn, ThingDef tool) =>
            pawn.GetHeldSurvivalTools().Any(t => t.def == tool);

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
            if (!pawn.CanUseSurvivalTools())
                return null;

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

            if (tool != null)
                LessonAutoActivator.TeachOpportunity(ST_ConceptDefOf.UsingSurvivalTools, OpportunityType.Important);

            return tool;
        }

        public static string GetSurvivalToolOverrideReportText(SurvivalTool tool, StatDef stat)
        {
            List<StatModifier> statFactorList = tool.WorkStatFactors.ToList();
            StuffPropsTool stuffPropsTool = tool.Stuff?.stuffPropsTool();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(stat.description);

            builder.AppendLine();
            builder.AppendLine(tool.def.LabelCap + ": " + tool.def.survivalTool().baseWorkStatFactors.GetStatFactorFromList(stat).ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Factor));

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

        public static void TryDegradeTool(Pawn pawn, StatDef stat)
        {
            SurvivalTool tool = pawn.GetBestSurvivalTool(stat);

            if (tool != null && tool.def.useHitPoints && SurvivalToolsSettings.toolDegradation)
            {
                LessonAutoActivator.TeachOpportunity(ST_ConceptDefOf.SurvivalToolDegradation, OpportunityType.GoodToKnow);
                tool.workTicksDone++;
                if (tool.workTicksDone >= tool.WorkTicksToDegrade)
                {
                    tool.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1));
                    tool.workTicksDone = 0;
                }
            }
        }
            
        public static IEnumerable<Thing> GetHeldSurvivalTools(this ThingOwner container) =>
            container?.Where(t => t.def.IsSurvivalTool());

        public static int HeldSurvivalToolCount(this Pawn pawn) =>
            pawn.inventory?.innerContainer?.GetHeldSurvivalTools()?.Count() ?? 0;

        public static bool CanCarryAnyMoreSurvivalTools(this Pawn pawn) =>
            (pawn.RaceProps.Humanlike && pawn.HeldSurvivalToolCount().IsUnderSurvivalToolCarryLimitFor(pawn)) || pawn.IsFormingCaravan() || pawn.IsCaravanMember();

        public static bool MeetsWorkGiverStatRequirements(this Pawn pawn, List<StatDef> requiredStats)
        {
            foreach (StatDef stat in requiredStats)
                if (pawn.GetStatValue(stat) <= 0f)
                    return false;
            return true;
        }

        public static IEnumerable<WorkGiver> AssignedToolRelevantWorkGivers(this Pawn pawn)
        {
            Pawn_WorkSettings workSettings = pawn.workSettings;
            if (workSettings == null)
            {
                Log.ErrorOnce($"Tried to get tool-relevant work givers for {pawn} but has null workSettings", 11227);
                yield break;
            }


            foreach (WorkGiver giver in pawn.workSettings.WorkGiversInOrderNormal)
            {
                WorkGiverExtension extension = giver.def.GetModExtension<WorkGiverExtension>();
                if (extension != null)
                    foreach (StatDef stat in extension.requiredStats)
                        if (stat.RequiresSurvivalTool())
                        {
                            yield return giver;
                            break;
                        }
            }
        }

        public static List<StatDef> AssignedToolRelevantWorkGiversStatDefs(this Pawn pawn)
        {
            List<StatDef> resultList = new List<StatDef>();
            foreach (WorkGiver giver in pawn.AssignedToolRelevantWorkGivers())
                foreach (StatDef stat in giver.def.GetModExtension<WorkGiverExtension>().requiredStats)
                    if (!resultList.Contains(stat))
                        resultList.Add(stat);
            return resultList;
        }

    }
}
