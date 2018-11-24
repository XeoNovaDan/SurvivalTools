using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class SpecialThingFilterWorker_NonSmeltable : SpecialThingFilterWorker
    {

        public override bool Matches(Thing t)
        {
            return !t.Smeltable;
        }

        public override bool AlwaysMatches(ThingDef def)
        {
            return !def.smeltable && !def.MadeFromStuff;
        }

    }
}
