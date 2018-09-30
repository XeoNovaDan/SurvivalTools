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
    public class WorkGiver_FellTrees : WorkGiver_Scanner
    {

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Designation> desList = pawn.Map.designationManager.allDesignations;
            for (int i = 0; i < desList.Count; i++)
            {
                Designation des = desList[i];
                if (des.def == DesignationDefOf.CutPlant || des.def == DesignationDefOf.HarvestPlant)
                {
                    yield return des.target.Thing;
                }
            }
            yield break;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.def.category != ThingCategory.Plant)
            {
                return null;
            }
            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return null;
            }
            if (t.IsForbidden(pawn))
            {
                return null;
            }
            if (t.IsBurning())
            {
                return null;
            }
            if (t.def.plant?.IsTree != true)
                return null;
            foreach (Designation designation in pawn.Map.designationManager.AllDesignationsOn(t))
            {
                if (designation.def == DesignationDefOf.HarvestPlant)
                {
                    if (!((Plant)t).HarvestableNow)
                    {
                        return null;
                    }
                    return new Job(ST_JobDefOf.HarvestTreeDesignated, t);
                }
                if (designation.def == DesignationDefOf.CutPlant)
                {
                    return new Job(ST_JobDefOf.FellTreeDesignated, t);
                }
            }
            return null;
        }

    }
}
