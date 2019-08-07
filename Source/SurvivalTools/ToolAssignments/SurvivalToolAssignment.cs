using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class SurvivalToolAssignment : IExposable, ILoadReferenceable
    {

        public SurvivalToolAssignment()
        {
        }

        public SurvivalToolAssignment(int uniqueId, string label)
        {
            this.uniqueId = uniqueId;
            this.label = label;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId");
            Scribe_Values.Look(ref label, "label");
            Scribe_Deep.Look(ref filter, "filter", new object[0]);
        }

        public string GetUniqueLoadID()
        {
            return "SurvivalToolAssignment_" + label + uniqueId.ToString();
        }

        public int uniqueId;
        public string label;
        public ThingFilter filter = new ThingFilter();

    }
}
