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
    [HarmonyPatch(nameof(Pawn_InventoryTracker.FirstUnloadableThing), MethodType.Getter)]
    public static class Patch_Pawn_InventoryTracker_FirstUnloadableThing
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

}
