using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class SurvivalTool : ThingWithComps
    {

        #region Properties
        //public bool InUse =>
        //    SurvivalToolUtility.BestSurvivalToolsFor(holdingOwner.Owner).Contains(this);

        public float WearChancePerTick =>
            // Yo dawg, i heard you like GenDate.TicksPerDay
            (GenDate.TicksPerDay / (this.GetStatValue(ST_StatDefOf.ToolEstimatedLifespan) * GenDate.TicksPerDay)) / StatWorker_EstimatedLifespan.WearInterval;

        public SurvivalToolProperties ToolProps =>
            def.GetModExtension<SurvivalToolProperties>();

        public StuffPropsTool StuffProps =>
            Stuff?.GetModExtension<StuffPropsTool>();
        #endregion

        public IEnumerable<StatModifier> WorkStatFactors
        {
            get
            {
                foreach (StatModifier modifier in ToolProps.baseWorkStatFactors)
                {
                    float newFactor = modifier.value * this.GetStatValue(ST_StatDefOf.ToolEffectivenessFactor);

                    if (StuffProps?.toolStatFactors.NullOrEmpty() == false)
                        foreach (StatModifier modifier2 in StuffProps.toolStatFactors)
                            if (modifier2.stat == modifier.stat)
                                newFactor *= modifier2.value;

                    yield return new StatModifier
                    {
                        stat = modifier.stat,
                        value = newFactor
                    };
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (StatModifier modifier in WorkStatFactors)
                yield return new StatDrawEntry(ST_StatCategoryDefOf.SurvivalTool,
                    modifier.stat.LabelCap,
                    modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                    overrideReportText: SurvivalToolUtility.GetSurvivalToolOverrideReportText(this, modifier.stat));
        }

        //public override string LabelNoCount
        //{
        //    get
        //    {
        //        string label = base.LabelNoCount;
        //        if (InUse)
        //            label += $", {"ToolInUse".Translate()}";
        //        return label;
        //    }
        //}

    }
}
