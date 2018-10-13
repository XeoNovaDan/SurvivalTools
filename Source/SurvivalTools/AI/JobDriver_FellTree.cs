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
    public class JobDriver_FellTree : JobDriver_PlantWork
    {
        protected override void Init()
        {
            if (base.Plant.def.plant.harvestedThingDef != null && base.Plant.CanYieldNow())
            {
                this.xpPerTick = 0.085f;
            }
            else
            {
                this.xpPerTick = 0f;
            }
        }

        protected override Toil PlantWorkDoneToil()
        {
            return DestroyThing(TargetIndex.A);
        }

        // Toils_Interact is internal so I copypasted this too
        public static Toil DestroyThing(TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                Pawn actor = toil.actor;
                Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
                if (!thing.Destroyed)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            };
            return toil;
        }
    }
}
