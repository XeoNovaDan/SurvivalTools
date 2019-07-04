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

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch(nameof(Pawn_InventoryTracker.Notify_ItemRemoved))]
    public static class Patch_Pawn_InventoryTracker_Notify_ItemRemoved
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
