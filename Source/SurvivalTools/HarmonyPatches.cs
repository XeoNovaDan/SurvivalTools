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

            //HarmonyInstance.DEBUG = true;

            h.Patch(AccessTools.Method(typeof(SymbolResolver_AncientRuins), nameof(SymbolResolver_AncientRuins.Resolve)),
                new HarmonyMethod(patchType, nameof(Prefix_Resolve)));

            h.Patch(AccessTools.Method(typeof(WorkGiver), nameof(WorkGiver.MissingRequiredCapacity)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_MissingRequiredCapacity)));

            h.Patch(AccessTools.Method(typeof(MassUtility), nameof(MassUtility.WillBeOverEncumberedAfterPickingUp)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_WillBeOverEncumberedAfterPickingUp)));

            h.Patch(AccessTools.Method(typeof(MassUtility), nameof(MassUtility.CountToPickUpUntilOverEncumbered)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_CountToPickUpUntilOverEncumbered)));

            h.Patch(AccessTools.Property(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.FirstUnloadableThing)).GetGetMethod(),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_FirstUnloadableThing)));

            h.Patch(AccessTools.Method(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_SpecialDisplayStats)));

            h.Patch(AccessTools.Method(typeof(WorkGiver_PlantsCut), nameof(WorkGiver_PlantsCut.JobOnThing)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_JobOnThing)));

            h.Patch(AccessTools.Method(typeof(WorkGiver_GrowerSow), nameof(WorkGiver_GrowerSow.JobOnCell)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_JobOnCell)));

            h.Patch(AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.HandleBlockingThingJob)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_HandleBlockingThingJob)));

            h.Patch(AccessTools.Method(typeof(RoofUtility), nameof(RoofUtility.HandleBlockingThingJob)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_HandleBlockingThingJob)));

            h.Patch(AccessTools.Method(typeof(RoofUtility), nameof(RoofUtility.CanHandleBlockingThing)),
                postfix: new HarmonyMethod(patchType, nameof(Postfix_CanHandleBlockingThing)));

            h.Patch(AccessTools.Method(typeof(JobDriver_Mine), "ResetTicksToPickHit"),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_ResetTicksToPickHit)));

            // Doing the same stuff as the above patch, therefore the same transpiler can be used
            h.Patch(AccessTools.Method(typeof(Mineable), nameof(Mineable.Notify_TookMiningDamage)),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_ResetTicksToPickHit)));

            // Thanks Mehni!
            h.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Transpile_FloatMenuMakerMad_AddHumanlikeOrders)));

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

            h.Patch(AccessTools.Method(typeof(JobDriver_Deconstruct), "TickAction"),
                new HarmonyMethod(patchType, nameof(Prefix_JobDriver_Deconstruct_TickAction)));

            h.Patch(typeof(JobDriver_AffectRoof).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().
                GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).
                MaxBy(mi => mi.GetMethodBody()?.GetILAsByteArray().Length ?? -1),
                transpiler: new HarmonyMethod(patchType, nameof(Transpile_JobDriver_AffectRoof_MakeNewToils)));
            #endregion

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

                // Set stuff
                if (thing.def.MadeFromStuff)
                {
                    // All stuff which the tool can be made from and has a market value of less than or equal to 3, excluding small-volume
                    List<ThingDef> validStuff = DefDatabase<ThingDef>.AllDefsListForReading.Where(
                        t => !t.smallVolume && t.IsStuff && 
                        GenStuff.AllowedStuffsFor(thing.def).Contains(t) &&
                        t.BaseMarketValue <= SurvivalToolUtility.MapGenToolMaxStuffMarketValue).ToList();

                    // Set random stuff based on stuff's market value and commonality
                    thing.SetStuffDirect(validStuff.RandomElementByWeight(
                        t => StuffMarketValueRemainderToCommonalityCurve.Evaluate(SurvivalToolUtility.MapGenToolMaxStuffMarketValue - t.BaseMarketValue) *
                        t.stuffProps?.commonality ?? 1f));
                }

                // Set hit points
                if (thing.def.useHitPoints)
                    thing.HitPoints = Mathf.RoundToInt(thing.MaxHitPoints * SurvivalToolUtility.MapGenToolHitPointsRange.RandomInRange);

                rp.singleThingToSpawn = thing;
                BaseGen.symbolStack.Push("thing", rp);
            }
        }

        private static SimpleCurve StuffMarketValueRemainderToCommonalityCurve = new SimpleCurve
        {
            new CurvePoint(0f, SurvivalToolUtility.MapGenToolMaxStuffMarketValue * 0.1f),
            new CurvePoint(SurvivalToolUtility.MapGenToolMaxStuffMarketValue, SurvivalToolUtility.MapGenToolMaxStuffMarketValue)
        };
        #endregion

        #region Postfix_MissingRequiredCapacity
        public static void Postfix_MissingRequiredCapacity(WorkGiver __instance, ref PawnCapacityDef __result, Pawn pawn)
        {
            // Bit of a weird hack for now, but I hope to make this a bit more elegant in the future
            if (__result == null && __instance.def.GetModExtension<WorkGiverExtension>() is WorkGiverExtension extension && !pawn.MeetsWorkGiverStatRequirements(extension.requiredStats))
                __result = PawnCapacityDefOf.Manipulation;
        }
        #endregion

        #region Postfix_WillBeOverEncumberedAfterPickingUp
        // Another janky hack
        public static void Postfix_WillBeOverEncumberedAfterPickingUp(ref bool __result, Pawn pawn, Thing thing)
        {
            if (pawn.RaceProps.Humanlike && thing as SurvivalTool != null && !pawn.CanCarryAnyMoreSurvivalTools())
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

        #region Postfix_FirstUnloadableThing
        public static void Postfix_FirstUnloadableThing(Pawn_InventoryTracker __instance, ref ThingCount __result)
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
                    __result = default(ThingCount);
            }
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

        #region Postfix_JobOnCell
        public static void Postfix_JobOnCell(ref Job __result, Pawn pawn)
        {
            if (__result?.def == JobDefOf.CutPlant && __result.targetA.Thing.def.plant.IsTree)
            {
                if (pawn.MeetsWorkGiverStatRequirements(ST_WorkGiverDefOf.FellTrees.GetModExtension<WorkGiverExtension>().requiredStats))
                    __result = new Job(ST_JobDefOf.FellTree, __result.targetA);
                else
                    __result = null;
            }
        }
        #endregion

        #region Postfix_HandleBlockingThingJob
        public static void Postfix_HandleBlockingThingJob(ref Job __result, Pawn worker)
        {
            if (__result?.def == JobDefOf.CutPlant && __result.targetA.Thing.def.plant.IsTree)
            {
                if (worker.MeetsWorkGiverStatRequirements(ST_WorkGiverDefOf.FellTrees.GetModExtension<WorkGiverExtension>().requiredStats))
                    __result = new Job(ST_JobDefOf.FellTree, __result.targetA);
                else
                    __result = null;
            }
        }
        #endregion

        #region Postfix_CanHandleBlockingThing
        public static void Postfix_CanHandleBlockingThing(ref bool __result, Thing blocker, Pawn worker)
        {
            if (blocker?.def.plant?.IsTree == true && !worker.MeetsWorkGiverStatRequirements(ST_WorkGiverDefOf.FellTrees.GetModExtension<WorkGiverExtension>().requiredStats))
                __result = false;
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

        // Credit to goes Mehni for letting me use this. Thanks!
        #region Transpile_FloatMenuMakerMad_AddHumanlikeOrders
        public static IEnumerable<CodeInstruction> Transpile_FloatMenuMakerMad_AddHumanlikeOrders(IEnumerable<CodeInstruction> instructions)
        {

            MethodInfo playerHome = AccessTools.Property(typeof(Map), nameof(Map.IsPlayerHome)).GetGetMethod();
            List<CodeInstruction> instructionList = instructions.ToList();

            //instructionList.RemoveRange(instructions.FirstIndexOf(ci => ci.operand == playerHome) - 3, 5);
            //return instructionList;

            bool patched = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (!patched && instruction.operand == playerHome && !ModCompatibilityCheck.OtherInventoryModsActive) // CE, Pick Up And Haul etc.
                //if (instructionList[i + 3].opcode == OpCodes.Callvirt && instruction.operand == playerHome)
                //if (instructionList[i + 3].operand == playerHome)
                {
                    {
                        instruction.opcode = OpCodes.Ldc_I4_0;
                        instruction.operand = null;
                        yield return instruction;
                        patched = true;
                    }
                    //    //{ instructionList[i + 5].labels = instruction.labels;}
                    //    instructionList.RemoveRange(i, 5);
                    //    patched = true;
                }
                yield return instruction;
            }
        }
        #endregion

        #region JobDriver Boilerplate

        // Using the transpiler-friendly overload
        private static MethodInfo TryDegradeTool =>
            AccessTools.Method(typeof(SurvivalToolUtility), nameof(SurvivalToolUtility.TryDegradeTool), new[] { typeof(Pawn), typeof(StatDef) });

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
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
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
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
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
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
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
                    instruction = new CodeInstruction(OpCodes.Call, TryDegradeTool);
                }

                yield return instruction;
            }
        }
        #endregion
        #region Prefix_JobDriver_Deconstruct_TickAction
        public static void Prefix_JobDriver_Deconstruct_TickAction(JobDriver_Deconstruct __instance)
        {
            SurvivalToolUtility.TryDegradeTool(__instance.pawn, StatDefOf.ConstructionSpeed);
        }
        #endregion
        #region Transpile_JobDriver_AffectRoof_MakeNewToils
        public static IEnumerable<CodeInstruction> Transpile_JobDriver_AffectRoof_MakeNewToils(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool done = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if (!done)
                {
                    yield return instruction;
                    yield return new CodeInstruction(instructionList[(i + 1)].opcode, instructionList[(i + 1)].operand);
                    yield return new CodeInstruction(instructionList[(i + 2)].opcode, instructionList[(i + 2)].operand);
                    yield return new CodeInstruction(OpCodes.Ldsfld, ConstructionSpeed);
                    yield return new CodeInstruction(OpCodes.Call, TryDegradeTool);
                    done = true;
                }

                yield return instruction;
            }
        }
        #endregion

        #endregion

        #endregion

    }
}
