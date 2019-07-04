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

    [HarmonyPatch(typeof(Toils_Haul))]
    [HarmonyPatch(nameof(Toils_Haul.TakeToInventory))]
    [HarmonyPatch(new Type[] { typeof(TargetIndex), typeof(Func<int>) })]
    public static class Patch_Toils_Haul_TakeToInventory
    {

        public static void Postfix(Toil __result, TargetIndex ind)
        {
            Action initAction = __result.initAction;
            __result.initAction = () =>
            {
                initAction();
                Pawn actor = __result.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                if (thing is SurvivalTool && actor.CanUseSurvivalTools() && actor.inventory.Contains(thing))
                    if (actor.CurJob.playerForced)
                        actor.GetComp<Pawn_SurvivalToolAssignmentTracker>().forcedHandler.SetForced(thing, true);
            };
        }

    }

}
