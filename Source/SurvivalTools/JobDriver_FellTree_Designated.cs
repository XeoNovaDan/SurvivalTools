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
    public class JobDriver_FellTree_Designated : JobDriver_FellTree
    {
        protected override DesignationDef RequiredDesignation
        {
            get
            {
                return DesignationDefOf.CutPlant;
            }
        }
    }
}
