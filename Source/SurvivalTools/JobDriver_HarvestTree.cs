using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace SurvivalTools
{

    // More copypasta
    public class JobDriver_HarvestTree : JobDriver_PlantWork
    {
        protected override void Init()
        {
            this.xpPerTick = 0.085f;
        }

        protected override Toil PlantWorkDoneToil()
        {
            return Toils_General.RemoveDesignationsOnThing(TargetIndex.A, DesignationDefOf.HarvestPlant);
        }
    }
}
