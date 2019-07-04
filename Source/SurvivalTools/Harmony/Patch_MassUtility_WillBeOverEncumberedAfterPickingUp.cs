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
    [HarmonyPatch(nameof(MassUtility.WillBeOverEncumberedAfterPickingUp))]
    public static class Patch_MassUtility_WillBeOverEncumberedAfterPickingUp
    {

        // Another janky hack
        public static void Postfix(ref bool __result, Pawn pawn, Thing thing)
        {
            if (pawn.RaceProps.Humanlike && thing as SurvivalTool != null && !pawn.CanCarryAnyMoreSurvivalTools())
                __result = true;
        }

    }

}
