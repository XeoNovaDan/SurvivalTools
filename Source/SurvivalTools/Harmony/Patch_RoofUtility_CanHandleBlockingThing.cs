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

    [HarmonyPatch(typeof(RoofUtility))]
    [HarmonyPatch(nameof(RoofUtility.CanHandleBlockingThing))]
    public static class Patch_RoofUtility_CanHandleBlockingThing
    {

        public static void Postfix(ref bool __result, Thing blocker, Pawn worker)
        {
            if (blocker?.def.plant?.IsTree == true && !worker.MeetsWorkGiverStatRequirements(ST_WorkGiverDefOf.FellTrees.GetModExtension<WorkGiverExtension>().requiredStats))
                __result = false;
        }

    }

}
