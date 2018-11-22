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

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn_SurvivalToolAssignmentTracker toolAssignmentTracker = pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>();
            if (!pawn.CanUseSurvivalTools() || toolAssignmentTracker == null || Find.TickManager.TicksGame < toolAssignmentTracker.nextSurvivalToolOptimizeTick)
            {
                return null;
            }

            SurvivalToolAssignment curAssignment = toolAssignmentTracker.CurrentSurvivalToolAssignment;
            List<Thing> heldTools = pawn.GetHeldSurvivalTools().ToList();
            foreach (Thing tool in heldTools)
                if (!curAssignment.filter.Allows(tool))
                    return pawn.DequipAndTryStoreSurvivalTool(tool);

            List<Thing> mapTools = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon).Where(t => t is SurvivalTool).ToList();
            if (mapTools.Count == 0)
            {
                SetNextOptimizeTick(pawn);
                return null;
            }

            Thing newTool = null;
            float delta = 0f;
            List<StatDef> workRelevantStats = pawn.AssignedToolRelevantWorkGiversStatDefs();

            foreach (Thing potentialTool in mapTools)
                if (curAssignment.filter.Allows(potentialTool))
                    if (pawn.CanUseSurvivalTool(potentialTool.def))
                        if (!pawn.HasSurvivalTool(potentialTool.def))
                            if (potentialTool.IsInAnyStorage())
                                if (!potentialTool.IsForbidden(pawn))
                                    if (!potentialTool.IsBurning())
                                    {
                                        float newDelta = SurvivalToolScoreGain(pawn, (SurvivalTool)potentialTool, workRelevantStats);
                                        if (newDelta > 0.05f && newDelta > delta)
                                            if (pawn.CanReserveAndReach(potentialTool, PathEndMode.OnCell, pawn.NormalMaxDanger()))
                                            {
                                                newTool = potentialTool;
                                                delta = newDelta;
                                            }
                                    }

            if (newTool == null || !pawn.CanCarryAnyMoreSurvivalTools())
            {
                SetNextOptimizeTick(pawn);
                return null;
            }

            Job pickupJob = new Job(JobDefOf.TakeInventory, newTool)
            {
                count = 1
            };
            return pickupJob; 
            
        }

        private static float SurvivalToolScoreGain(Pawn pawn, SurvivalTool tool, List<StatDef> workRelevantStats)
        {
            float delta = SurvivalToolScoreRaw(pawn, tool, workRelevantStats);
            foreach (SurvivalTool heldTool in pawn.GetAllUsableSurvivalTools())
                if (heldTool.InUse)
                    foreach (StatModifier statMod in heldTool.WorkStatFactors)
                        if (tool.WorkStatFactors.Contains(statMod))
                        {
                            delta -= SurvivalToolScoreRaw(pawn, heldTool, workRelevantStats);
                            goto Finished;
                        }
            Finished:
                return delta;
                
        }

        private static float SurvivalToolScoreRaw(Pawn pawn, SurvivalTool tool, List<StatDef> workRelevantStats)
        {
            float optimality = 1f;
            foreach (StatDef relStat in workRelevantStats)
                optimality *= StatUtility.GetStatFactorFromList(tool.WorkStatFactors.ToList(), relStat);
            if (tool.def.useHitPoints)
                optimality *= tool.GetStatValue(ST_StatDefOf.ToolEstimatedLifespan) * ((float)tool.HitPoints / tool.MaxHitPoints);
            return optimality;
        }

    }
}
