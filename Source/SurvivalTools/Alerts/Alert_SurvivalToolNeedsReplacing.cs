using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class Alert_SurvivalToolNeedsReplacing : Alert
    {

        private const float DamagedToolRemainingLifespanThreshold = 0.5f;

        public Alert_SurvivalToolNeedsReplacing()
        {
            defaultPriority = AlertPriority.Medium;
        }

        private IEnumerable<Pawn> WorkersDamagedTools
        {
            get
            {
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                    if (HasDamagedTools(pawn))
                        yield return pawn;
            }
        }

        private static bool HasDamagedTools(Pawn pawn)
        {
            foreach (SurvivalTool tool in pawn.GetAllUsableSurvivalTools().Where(t => ((SurvivalTool)t).InUse))
            {
                float toolLifespan = tool.GetStatValue(ST_StatDefOf.ToolEstimatedLifespan);
                float hitPointsPercentage = (float)tool.HitPoints / tool.MaxHitPoints;
                if (toolLifespan * hitPointsPercentage <= DamagedToolRemainingLifespanThreshold)
                    return true;
            }
            return false;
        }

        public override string GetExplanation()
        {
            string result = "SurvivalToolNeedsReplacingDesc".Translate() + ":\n";
            foreach (Pawn pawn in WorkersDamagedTools)
                result += ("\n    " + pawn.LabelShort);
            return result;
        }

        public override string GetLabel() =>
            "SurvivalToolsNeedReplacing".Translate();

        public override AlertReport GetReport() =>
            (SurvivalToolsSettings.ToolDegradation) ? AlertReport.CulpritsAre(WorkersDamagedTools) : false;
    }
}
