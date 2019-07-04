using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.Harmony
{

    [HarmonyPatch(typeof(SymbolResolver_AncientRuins))]
    [HarmonyPatch(nameof(SymbolResolver_AncientRuins.Resolve))]
    public static class Patch_SymbolResolver_AncientRuins_Resolve
    {

        private static readonly SimpleCurve StuffMarketValueRemainderToCommonalityCurve = new SimpleCurve
        {
            new CurvePoint(0f, SurvivalToolUtility.MapGenToolMaxStuffMarketValue * 0.1f),
            new CurvePoint(SurvivalToolUtility.MapGenToolMaxStuffMarketValue, SurvivalToolUtility.MapGenToolMaxStuffMarketValue)
        };

        public static void Prefix(ResolveParams rp)
        {
            if (SurvivalToolsSettings.toolMapGen)
            {
                List<Thing> things = ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.Generate();
                foreach (Thing thing in things)
                {
                    // Custom quality generator
                    if (thing.TryGetComp<CompQuality>() is CompQuality qualityComp)
                    {
                        QualityCategory newQuality = (QualityCategory)(AccessTools.Method(typeof(QualityUtility), "GenerateFromGaussian").
                            Invoke(null, new object[] { 1f, QualityCategory.Normal, QualityCategory.Poor, QualityCategory.Awful }));
                        qualityComp.SetQuality(newQuality, ArtGenerationContext.Outsider);
                    }

                    // Set stuff
                    if (thing.def.MadeFromStuff)
                    {
                        // All stuff which the tool can be made from and has a market value of less than or equal to 3, excluding small-volume
                        List<ThingDef> validStuff = DefDatabase<ThingDef>.AllDefsListForReading.Where(
                            t => !t.smallVolume && t.IsStuff &&
                            GenStuff.AllowedStuffsFor(thing.def).Contains(t) &&
                            t.BaseMarketValue <= SurvivalToolUtility.MapGenToolMaxStuffMarketValue).ToList();

                        // Set random stuff based on stuff's market value and commonality
                        thing.SetStuffDirect(validStuff.RandomElementByWeight(
                            t => StuffMarketValueRemainderToCommonalityCurve.Evaluate(SurvivalToolUtility.MapGenToolMaxStuffMarketValue - t.BaseMarketValue) *
                            t.stuffProps?.commonality ?? 1f));
                    }

                    // Set hit points
                    if (thing.def.useHitPoints)
                        thing.HitPoints = Mathf.RoundToInt(thing.MaxHitPoints * SurvivalToolUtility.MapGenToolHitPointsRange.RandomInRange);

                    rp.singleThingToSpawn = thing;
                    BaseGen.symbolStack.Push("thing", rp);
                }
            }
        }

    }

}
