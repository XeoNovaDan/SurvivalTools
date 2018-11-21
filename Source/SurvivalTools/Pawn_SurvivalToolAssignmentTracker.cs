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
            Scribe_References.Look(ref curSurvivalToolAssignment, "curSurvivalToolAssignment");
        }

        public int nextSurvivalToolOptimizeTick = -99999;
        private SurvivalToolAssignment curSurvivalToolAssignment;

    }
}
