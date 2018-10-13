using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class StuffPropsTool : DefModExtension
    {

        public static readonly StuffPropsTool defaultValues = new StuffPropsTool();

        public List<StatModifier> toolStatFactors;

        public float wearFactorMultiplier = 1f;

    }
}
