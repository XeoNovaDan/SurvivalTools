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

        public int workTicksDone = 0;

        #region Properties
        public Pawn HoldingPawn
        {
            get
            {
                if (ParentHolder is Pawn_EquipmentTracker eq)
                    return eq.pawn;
                if (ParentHolder is Pawn_InventoryTracker inv)
                    return inv.pawn;
                return null;
            }
        }

        public bool InUse =>
            HoldingPawn != null && HoldingPawn.CanUseSurvivalTools() && HoldingPawn.CanUseSurvivalTool(def) &&
            SurvivalToolUtility.BestSurvivalToolsFor(HoldingPawn).Contains(this);

        public int WorkTicksToDegrade =>
            Mathf.FloorToInt((this.GetStatValue(ST_StatDefOf.ToolEstimatedLifespan) * GenDate.TicksPerDay) / MaxHitPoints);
        
        public IEnumerable<StatModifier> WorkStatFactors
        {
            get
            {
                foreach (StatModifier modifier in def.GetModExtension<SurvivalToolProperties>().baseWorkStatFactors)
                {
                    float newFactor = modifier.value * this.GetStatValue(ST_StatDefOf.ToolEffectivenessFactor);

                    if (Stuff?.GetModExtension<StuffPropsTool>()?.toolStatFactors.NullOrEmpty() == false)
                        foreach (StatModifier modifier2 in Stuff?.GetModExtension<StuffPropsTool>()?.toolStatFactors)
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

        //public override string LabelNoCount
        //{
        //    get
        //    {
        //        string label = base.LabelNoCount;
        //        if (HoldingPawn != null && HoldingPawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>() is Pawn_SurvivalToolAssignmentTracker tracker &&
        //            tracker.forcedHandler.IsForced(this))
        //            label += $", {"ApparelForcedLower".Translate()}";
        //        if (InUse)
        //            label += $", {"ToolInUse".Translate()}";
        //        return label;
        //    }
        //}
        #endregion

        #region Methods
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (StatModifier modifier in WorkStatFactors)
                yield return new StatDrawEntry(ST_StatCategoryDefOf.SurvivalTool,
                    modifier.stat.LabelCap,
                    modifier.value.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor),
                    overrideReportText: SurvivalToolUtility.GetSurvivalToolOverrideReportText(this, modifier.stat));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workTicksDone, "workTicksDone", 0);
        }
        #endregion

    }
}
