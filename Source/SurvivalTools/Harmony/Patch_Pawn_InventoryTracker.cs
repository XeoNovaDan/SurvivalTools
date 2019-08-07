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

    public static class Patch_Pawn_InventoryTracker
    {

        [HarmonyPatch(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.FirstUnloadableThing), MethodType.Getter)]
        public static class FirstUnloadableThing
        {

            public static void Postfix(Pawn_InventoryTracker __instance, ref ThingCount __result)
            {
                if (__result.Thing is SurvivalTool tool && tool.InUse)
                {
                    bool foundNewThing = false;
                    // Had to iterate through because a lambda expression in this case isn't possible
                    for (int i = 0; i < __instance.innerContainer.Count; i++)
                    {
                        Thing newThing = __instance.innerContainer[i];
                        if (newThing as SurvivalTool == null || !((SurvivalTool)newThing).InUse)
                        {
                            __result = new ThingCount(newThing, newThing.stackCount);
                            foundNewThing = true;
                            break;
                        }
                    }
                    if (!foundNewThing)
                        __result = default;
                }
            }

        }

        [HarmonyPatch(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.InventoryTrackerTickRare))]
        public static class InventoryTrackerTickRare
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

        [HarmonyPatch(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.Notify_ItemRemoved))]
        public static class Notify_ItemRemoved
        {

            public static void Postfix(Pawn_InventoryTracker __instance, Thing item)
            {
                if (item is SurvivalTool && __instance.pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>() is Pawn_SurvivalToolAssignmentTracker assignmentTracker)
                {
                    assignmentTracker.forcedHandler.SetForced(item, false);
                }

            }

        }

    }

}
