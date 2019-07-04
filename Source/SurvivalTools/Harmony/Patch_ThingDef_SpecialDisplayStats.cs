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

    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
    public static class Patch_ThingDef_SpecialDisplayStats
    {

        public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            // Tool def
            if (req.Thing == null && __instance.IsSurvivalTool(out SurvivalToolProperties tProps))
            {
                foreach (StatModifier modifier in tProps.baseWorkStatFactors)
                    __result = __result.Add(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalTool,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        overrideReportText: modifier.stat.description));
            }

            // Stuff
            if (__instance.IsStuff && __instance.GetModExtension<StuffPropsTool>() is StuffPropsTool sPropsTool)
            {
                foreach (StatModifier modifier in sPropsTool.toolStatFactors)
                    __result = __result.Add(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalToolMaterial,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        overrideReportText: modifier.stat.description));
            }
        }

    }

}
