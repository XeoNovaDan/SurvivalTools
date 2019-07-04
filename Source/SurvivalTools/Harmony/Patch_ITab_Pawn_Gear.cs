using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.Harmony
{

    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("DrawThingRow")]
    public static class Patch_ITab_Pawn_Gear
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();

            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];

                yield return instruction;

                // If equipment is a tool, adjust its label in a similar fashion to how apparel labels are adjusted (though using a helper method)
                if (!done && instruction.opcode == OpCodes.Stloc_S && instruction.operand.ToString() == "5")
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5); // text
                    yield return new CodeInstruction(OpCodes.Ldarg_3); // thing
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear").GetGetMethod(true)); // this.SelPawnForGear
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ITab_Pawn_Gear), "AdjustDisplayedLabel")); // AdjustDisplayedLabel(ref text, thing, this.SelPawnForGear)
                    done = true;
                }
            }
        }

        public static void AdjustDisplayedLabel(ref string originalLabel, Thing thing, Pawn pawn)
        {
            if (thing is SurvivalTool tool)
            {
                // Forced
                if (pawn.GetComp<Pawn_SurvivalToolAssignmentTracker>() is Pawn_SurvivalToolAssignmentTracker toolAssignmentTracker && toolAssignmentTracker.forcedHandler.IsForced(tool))
                    originalLabel += $", {"ApparelForcedLower".Translate()}";

                // In use
                if (tool.InUse)
                    originalLabel += $", {"ToolInUse".Translate()}";
            }
        }
    }

}
