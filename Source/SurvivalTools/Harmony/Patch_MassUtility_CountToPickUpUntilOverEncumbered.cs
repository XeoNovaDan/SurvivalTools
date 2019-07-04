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

    [HarmonyPatch(typeof(MassUtility))]
    [HarmonyPatch(nameof(MassUtility.CountToPickUpUntilOverEncumbered))]
    public static class Patch_MassUtility_CountToPickUpUntilOverEncumbered
    {

        public static void Postfix(ref int __result, Pawn pawn, Thing thing)
        {
            if (__result > 0 && pawn.RaceProps.Humanlike && thing as SurvivalTool != null && !pawn.CanCarryAnyMoreSurvivalTools())
                __result = 0;
        }

    }

}
