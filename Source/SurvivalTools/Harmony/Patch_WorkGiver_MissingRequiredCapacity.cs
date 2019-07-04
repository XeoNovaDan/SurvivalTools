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

    [HarmonyPatch(typeof(WorkGiver))]
    [HarmonyPatch(nameof(WorkGiver.MissingRequiredCapacity))]
    public static class Patch_WorkGiver_MissingRequiredCapacity
    {

        public static void Postfix(WorkGiver __instance, ref PawnCapacityDef __result, Pawn pawn)
        {
            // Bit of a weird hack for now, but I hope to make this a bit more elegant in the future
            if (__result == null && __instance.def.GetModExtension<WorkGiverExtension>() is WorkGiverExtension extension && !pawn.MeetsWorkGiverStatRequirements(extension.requiredStats))
                __result = PawnCapacityDefOf.Manipulation;
        }

    }

}
