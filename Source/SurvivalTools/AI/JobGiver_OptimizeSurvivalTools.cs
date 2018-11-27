using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace SurvivalTools
{
    public class JobGiver_OptimizeSurvivalTools : ThinkNode_JobGiver
    {

        private void SetNextOptimizeTick(Pawn pawn)
        {
            pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>().nextSurvivalToolOptimizeTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
        }

        // This is a janky mess and a half, but works!
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (SurvivalToolsSettings.toolOptimization)
            {
 
                Pawn_SurvivalToolAssignmentTracker assignmentTracker = pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>();

                // Pawn can't use tools, lacks a tool assignment tracker or it isn't yet time to re-optimise tools
                if (!pawn.CanUseSurvivalTools() || assignmentTracker == null || Find.TickManager.TicksGame < assignmentTracker.nextSurvivalToolOptimizeTick)
                    return null;

                // Check if current tool assignment allows for each tool, auto-removing those that aren't allowed.
                SurvivalToolAssignment curAssignment = assignmentTracker.CurrentSurvivalToolAssignment;
                List<Thing> heldTools = pawn.GetHeldSurvivalTools().ToList();
                foreach (Thing tool in heldTools)
                    if ((!curAssignment.filter.Allows(tool) || !pawn.NeedsSurvivalTool((SurvivalTool)tool)) && assignmentTracker.forcedHandler.AllowedToAutomaticallyDrop(tool))
                        return pawn.DequipAndTryStoreSurvivalTool(tool);

                // Look for better alternative tools to what the colonist currently has, based on what stats are relevant to the work types the colonist is assigned to
                List<Thing> heldUsableTools = heldTools.Where(t => heldTools.IndexOf(t).IsUnderSurvivalToolCarryLimitFor(pawn)).ToList();
                List<Thing> mapTools = pawn.MapHeld.listerThings.AllThings.Where(t => t is SurvivalTool).ToList();
                List<StatDef> workRelevantStats = pawn.AssignedToolRelevantWorkGiversStatDefs();
                Thing curTool = null;
                Thing newTool = null;
                float optimality = 0f;
                foreach (StatDef stat in workRelevantStats)
                {
                    curTool = pawn.GetBestSurvivalTool(stat);
                    optimality = SurvivalToolScore(curTool, workRelevantStats);
                    foreach (Thing potentialToolThing in mapTools)
                    {
                        SurvivalTool potentialTool = (SurvivalTool)potentialToolThing;
                        if (StatUtility.StatListContains(potentialTool.WorkStatFactors.ToList(), stat) && curAssignment.filter.Allows(potentialTool) && potentialTool.BetterThanWorkingToollessFor(stat) &&
                            pawn.CanUseSurvivalTool(potentialTool.def) && potentialTool.IsInAnyStorage() && !potentialTool.IsForbidden(pawn) && !potentialTool.IsBurning())
                        {
                            float potentialOptimality = SurvivalToolScore(potentialTool, workRelevantStats);
                            float delta = potentialOptimality - optimality;
                            if (delta > 0f && pawn.CanReserveAndReach(potentialTool, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                            {
                                newTool = potentialTool;
                                optimality = potentialOptimality;
                            }
                        }
                    }
                    if (newTool != null)
                        break;
                }

                // Return a job based on whether or not a better tool was located

                // Failure
                if (newTool == null)
                {
                    SetNextOptimizeTick(pawn);
                    return null;
                }

                // Success
                int heldToolOffset = 0;
                if (curTool != null && assignmentTracker.forcedHandler.AllowedToAutomaticallyDrop(curTool))
                {
                    pawn.jobs.jobQueue.EnqueueFirst(pawn.DequipAndTryStoreSurvivalTool(curTool, false));
                    heldToolOffset = -1;
                }
                if (pawn.CanCarryAnyMoreSurvivalTools(heldToolOffset))
                {
                    Job pickupJob = new Job(JobDefOf.TakeInventory, newTool)
                    {
                        count = 1
                    };
                    return pickupJob;
                }

                // Final failure state
                SetNextOptimizeTick(pawn);
            }

            return null;

        }

        private static float SurvivalToolScore(Thing toolThing, List<StatDef> workRelevantStats)
        {
            SurvivalTool tool = toolThing as SurvivalTool;
            if (tool == null)
                return 0f;
            
            float optimality = 0f;
            foreach (StatDef stat in workRelevantStats)
                optimality += StatUtility.GetStatValueFromList(tool.WorkStatFactors.ToList(), stat, 0f);

            if (tool.def.useHitPoints)
            {
                float lifespanRemaining = tool.GetStatValue(ST_StatDefOf.ToolEstimatedLifespan) * ((float)tool.HitPoints * tool.MaxHitPoints);
                optimality *= LifespanDaysToOptimalityMultiplierCurve.Evaluate(lifespanRemaining);
            }
            return optimality;
        }

        private static readonly SimpleCurve LifespanDaysToOptimalityMultiplierCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.04f),
            new CurvePoint(0.5f, 0.2f),
            new CurvePoint(1f, 0.5f),
            new CurvePoint(2f, 1f),
            new CurvePoint(4f, 1f),
            new CurvePoint(999f, 10f)
        };

    }
}
