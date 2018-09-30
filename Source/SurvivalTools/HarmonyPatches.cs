using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;

namespace SurvivalTools
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {

            HarmonyInstance h = HarmonyInstance.Create("XeoNovaDan.SurvivalTools");

            // HarmonyInstance.DEBUG = true;

            h.Patch(AccessTools.Method(typeof(SymbolResolver_AncientRuins), nameof(SymbolResolver_AncientRuins.Resolve)),
                new HarmonyMethod(patchType, nameof(Prefix_Resolve)));

            h.Patch(AccessTools.Method(typeof(WorkGiver), nameof(WorkGiver.MissingRequiredCapacity)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_MissingRequiredCapacity)));

            h.Patch(AccessTools.Method(typeof(MassUtility), nameof(MassUtility.WillBeOverEncumberedAfterPickingUp)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_WillBeOverEncumberedAfterPickingUp)));

            h.Patch(AccessTools.Method(typeof(MassUtility), nameof(MassUtility.CountToPickUpUntilOverEncumbered)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_CountToPickUpUntilOverEncumbered)));

            h.Patch(AccessTools.Method(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_SpecialDisplayStats)));

            h.Patch(AccessTools.Method(typeof(WorkGiver_PlantsCut), nameof(WorkGiver_PlantsCut.JobOnThing)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_JobOnThing)));

            h.Patch(AccessTools.Method(typeof(JobDriver_Mine), "ResetTicksToPickHit"),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_ResetTicksToPickHit)));

            // erdelf never fails to impress :)
            #region JobDriver Boilerplate
            h.Patch(typeof(RimWorld.JobDriver_PlantWork).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_PlantWork_MakeNewToils)));

            h.Patch(typeof(JobDriver_Mine).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Mine_MakeNewToils)));

            h.Patch(typeof(JobDriver_ConstructFinishFrame).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_ConstructFinishFrame_MakeNewToils)));

            h.Patch(typeof(JobDriver_Repair).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_Repair_MakeNewToils)));
            #endregion

            // Add validator
            ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.fixedParams.validator = (ThingDef t) =>
            t.techLevel == TechLevel.Neolithic;

        }

        #region Prefix_Resolve
        public static void Prefix_Resolve(ResolveParams rp)
        {
            List<Thing> things = ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.Generate();
            foreach (Thing thing in things)
            {
                // Custom quality generator
                if (thing.TryGetComp<CompQuality>() is CompQuality qualityComp)
                {
                    QualityCategory newQuality = (QualityCategory)(AccessTools.Method(typeof(QualityUtility), "GenerateFromGaussian").
                        Invoke(null, new object[] { 1f, QualityCategory.Normal, QualityCategory.Poor, QualityCategory.Awful }));
                    qualityComp.SetQuality(newQuality, ArtGenerationContext.Outsider);
                }

                // Set hit points
                if (thing.def.useHitPoints)
                    thing.HitPoints = Mathf.RoundToInt(thing.MaxHitPoints * SurvivalToolUtility.AncientToolHitPointsRange.RandomInRange);

                rp.singleThingToSpawn = thing;
                BaseGen.symbolStack.Push("thing", rp);
            }
        }
        #endregion

        #region Postfix_MissingRequiredCapacity
        public static void Postfix_MissingRequiredCapacity(WorkGiver __instance, ref PawnCapacityDef __result, Pawn pawn)
        {
            // Bit of a weird hack for now, but I hope to make this a bit more elegant in the future
            if (__result == null && __instance.def.GetModExtension<WorkGiverExtension>() is WorkGiverExtension extension && !pawn.MeetsWorkGiverStatRequirement(extension.requiredStat))
                __result = PawnCapacityDefOf.Manipulation;
        }
        #endregion

        #region Postfix_WillBeOverEncumberedAfterPickingUp
        // Another janky hack
        public static void Postfix_WillBeOverEncumberedAfterPickingUp(ref bool __result, Pawn pawn, Thing thing)
        {
            if (pawn.RaceProps.Humanlike && thing as SurvivalTool != null // If the pawn is humanlike (can use tools) and the thing is a tool
                && pawn.inventory.innerContainer.Where(t => t.def.IsSurvivalTool()).Count() >= SurvivalToolUtility.MaxToolsCarriedAtOnce) // and if the pawn is carrying 3 or more tools
                __result = true;
        }
        #endregion

        #region Postfix_CountToPickUpUntilOverEncumbered
        public static void Postfix_CountToPickUpUntilOverEncumbered(ref int __result, Pawn pawn, Thing thing)
        {
            if (__result > 0 && pawn.RaceProps.Humanlike && thing as SurvivalTool != null && !pawn.CanCarryAnyMoreSurvivalTools())
                __result = 0;
        }
        #endregion

        #region Postfix_SpecialDisplayStats
        public static void Postfix_SpecialDisplayStats(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result, StatRequest req)
        {
            // Tool def
            if (req.Thing == null && __instance.IsSurvivalTool(out SurvivalToolProperties tProps))
            {
                foreach (StatModifier modifier in tProps.baseWorkStatFactors)
                    __result = __result.Add(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalTool,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        overrideReportText: modifier.stat.description));
            }

            // Stuff
            if (__instance.IsStuff && __instance.GetModExtension<StuffPropsTool>() is StuffPropsTool sPropsTool)
            {
                foreach (StatModifier modifier in sPropsTool.toolStatFactors)
                    __result = __result.Add(new StatDrawEntry(ST_StatCategoryDefOf.SurvivalToolMaterial,
                        modifier.stat.LabelCap,
                        modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                        overrideReportText: modifier.stat.description));
            }
        }
        #endregion

        #region Postfix_JobOnThing
        public static void Postfix_JobOnThing(ref Job __result, Thing t, Pawn pawn)
        {
            if (t.def.plant?.IsTree == true)
                __result = null;
        }
        #endregion

        #region Transpile_ResetTicksToPickHit
        public static IEnumerable<CodeInstruction> Transpile_ResetTicksToPickHit(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.MiningSpeed)))
                {
                    instruction.operand = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed));
                }

                yield return instruction;
            }
        }
        #endregion

        #region JobDriver Boilerplate

        // Using the transpiler-friendly overload
        private static MethodInfo TryApplyToolWear =>
            AccessTools.Method(typeof(SurvivalToolUtility), nameof(SurvivalToolUtility.TryApplyToolWear), new[] { typeof(Pawn), typeof(StatDef) });

        #region Transpile_JobDriver_PlantWork_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_PlantWork_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo plantHarvestingSpeed = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.PlantHarvestingSpeed));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, plantHarvestingSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryApplyToolWear);
                }

                if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.PlantWorkSpeed)))
                {
                    instruction.operand = plantHarvestingSpeed;
                }

                yield return instruction;
            }
        }
        #endregion
        #region Transpile_JobDriver_Mine_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Mine_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo diggingSpeed = AccessTools.Field(typeof(ST_StatDefOf), nameof(ST_StatDefOf.DiggingSpeed));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, diggingSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryApplyToolWear);
                }

                yield return instruction;
            }
        }
        #endregion

        #region Construction JobDrivers

        private static FieldInfo ConstructionSpeed =>
            AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.ConstructionSpeed));

        #region Transpile_JobDriver_ConstructFinishFrame_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_ConstructFinishFrame_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryApplyToolWear);
                }

                yield return instruction;
            }
        }
        #endregion
        #region Transpile_JobDriver_Repair_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_Repair_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    instruction = new CodeInstruction(OpCodes.Call, TryApplyToolWear);
                }

                yield return instruction;
            }
        }
        #endregion

        #endregion

        #endregion

    }
}
