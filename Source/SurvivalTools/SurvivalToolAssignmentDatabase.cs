using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{

    public class SurvivalToolAssignmentDatabase : GameComponent
    {

        public SurvivalToolAssignmentDatabase(Game game)
        {
        }

        public override void FinalizeInit()
        {
            if (!initialized)
            {
                GenerateStartingSurvivalToolAssignments();
                initialized = true;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref initialized, "initialized", false);
            Scribe_Collections.Look(ref survivalToolAssignments, "survivalToolAssignments", LookMode.Deep, new object[0]);
        }

        public List<SurvivalToolAssignment> AllSurvivalToolAssignments =>
            survivalToolAssignments;

        public SurvivalToolAssignment DefaultSurvivalToolAssignment() =>
            survivalToolAssignments.Count == 0 ? MakeNewSurvivalToolAssignment() : survivalToolAssignments[0];

        public AcceptanceReport TryDelete(SurvivalToolAssignment toolAssignment)
        {

            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                if (pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>()?.CurrentSurvivalToolAssignment == toolAssignment)
                    return new AcceptanceReport("SurvivalToolAssignmentInUse".Translate(pawn));
            foreach (Pawn pawn2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                if (pawn2.TryGetComp<Pawn_SurvivalToolAssignmentTracker>() is Pawn_SurvivalToolAssignmentTracker toolAssignmentTracker &&
                    toolAssignmentTracker.CurrentSurvivalToolAssignment == toolAssignment)
                    toolAssignmentTracker.CurrentSurvivalToolAssignment = null;
            survivalToolAssignments.Remove(toolAssignment);
            return AcceptanceReport.WasAccepted;
        }

        public SurvivalToolAssignment MakeNewSurvivalToolAssignment()
        {
            int uniqueId = survivalToolAssignments.Any() ? survivalToolAssignments.Max(a => a.uniqueId) + 1 : 1;
            SurvivalToolAssignment toolAssignment = new SurvivalToolAssignment(uniqueId , $"{"SurvivalToolAssignment".Translate()} {uniqueId}");
            toolAssignment.filter.SetAllow(ST_ThingCategoryDefOf.SurvivalTools, true);
            survivalToolAssignments.Add(toolAssignment);
            return toolAssignment;
        }

        private void GenerateStartingSurvivalToolAssignments()
        {
            SurvivalToolAssignment staAnything = MakeNewSurvivalToolAssignment();
            staAnything.label = "OutfitAnything".Translate();

            SurvivalToolAssignment staConstructor = MakeNewSurvivalToolAssignment();
            staConstructor.label = "SurvivalToolAssignmentConstructor".Translate();
            staConstructor.filter.SetDisallowAll(null, null);

            SurvivalToolAssignment staMiner = MakeNewSurvivalToolAssignment();
            staMiner.label = "SurvivalToolAssignmentMiner".Translate();
            staMiner.filter.SetDisallowAll(null, null);

            SurvivalToolAssignment staPlantWorker = MakeNewSurvivalToolAssignment();
            staPlantWorker.label = "SurvivalToolAssignmentPlantWorker".Translate();
            staPlantWorker.filter.SetDisallowAll(null, null);

            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs)
            {
                SurvivalToolProperties toolProps = tDef.survivalTool();
                if (toolProps == null)
                    continue;
                else
                {
                    if (toolProps.defaultSurvivalToolAssignmentTags.Contains("Constructor"))
                        staConstructor.filter.SetAllow(tDef, true);
                    if (toolProps.defaultSurvivalToolAssignmentTags.Contains("Miner"))
                        staMiner.filter.SetAllow(tDef, true);
                    if (toolProps.defaultSurvivalToolAssignmentTags.Contains("PlantWorker"))
                        staPlantWorker.filter.SetAllow(tDef, true);
                }
            }

            SurvivalToolAssignment staNothing = MakeNewSurvivalToolAssignment();
            staNothing.label = "FoodRestrictionNothing".Translate();
            staNothing.filter.SetDisallowAll(null, null);
        }

        private bool initialized = false;
        private List<SurvivalToolAssignment> survivalToolAssignments = new List<SurvivalToolAssignment>();

    }

}
