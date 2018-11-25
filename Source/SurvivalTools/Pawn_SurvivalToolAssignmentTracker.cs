using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class Pawn_SurvivalToolAssignmentTracker : ThingComp
    {

        Pawn Pawn => (Pawn)parent;

        public SurvivalToolAssignment CurrentSurvivalToolAssignment
        {
            get
            {
                if (curSurvivalToolAssignment == null)
                    curSurvivalToolAssignment = Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().DefaultSurvivalToolAssignment();
                return curSurvivalToolAssignment;
            }
            set
            {
                curSurvivalToolAssignment = value;
                nextSurvivalToolOptimizeTick = Find.TickManager.TicksGame;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextSurvivalToolOptimizeTick, "nextSurvivalToolOptimizeTick", -99999);
            Scribe_Deep.Look(ref forcedHandler, "forcedHandler");
            Scribe_References.Look(ref curSurvivalToolAssignment, "curSurvivalToolAssignment");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (forcedHandler == null)
                    forcedHandler = new SurvivalToolForcedHandler();
            }
        }

        public int nextSurvivalToolOptimizeTick = -99999;
        public SurvivalToolForcedHandler forcedHandler;
        private SurvivalToolAssignment curSurvivalToolAssignment;

    }
}
