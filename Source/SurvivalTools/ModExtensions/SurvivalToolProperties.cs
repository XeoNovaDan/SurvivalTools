using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class SurvivalToolProperties : DefModExtension
    {

        public static readonly SurvivalToolProperties defaultValues = new SurvivalToolProperties();

        public List<StatModifier> baseWorkStatFactors;

        public float toolWearFactor = 1f;

    }
}
