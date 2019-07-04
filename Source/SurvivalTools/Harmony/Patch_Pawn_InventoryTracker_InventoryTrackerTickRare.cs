using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.Harmony
{

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch(nameof(Pawn_InventoryTracker.InventoryTrackerTickRare))]
    public static class Patch_Pawn_InventoryTracker_InventoryTrackerTickRare
    {

        public static void Postfix(Pawn_InventoryTracker __instance)
        {
            if (SurvivalToolsSettings.toolLimit)
            {
                Pawn pawn = __instance.pawn;
                if (pawn.CanUseSurvivalTools() && pawn.GetHeldSurvivalTools().Count() > pawn.GetStatValue(ST_StatDefOf.SurvivalToolCarryCapacity) && pawn.CanRemoveExcessSurvivalTools())
                {
                    Thing tool = pawn.GetHeldSurvivalTools().Last();
                    Job job = pawn.DequipAndTryStoreSurvivalTool(tool);
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced, cancelBusyStances: false);
                }
            }
        }

    }

}
