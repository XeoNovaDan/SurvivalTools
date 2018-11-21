using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class Alert_ColonistNeedsSurvivalTool : Alert
    {

        public Alert_ColonistNeedsSurvivalTool()
        {
            defaultPriority = AlertPriority.High;
        }

        private IEnumerable<Pawn> ToollessWorkers
        {
            get
            {
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                    if (WorkingToolless(pawn))
                        yield return pawn;
            }
        }

        private static bool WorkingToolless(Pawn pawn)
        {
            foreach (StatDef stat in pawn.AssignedToolRelevantWorkGiversStatDefs())
                if (!pawn.HasSurvivalToolFor(stat))
                    return true;
            return false;
        }

        private static string ToollessWorkTypesString(Pawn pawn)
        {
            List<string> types = new List<string>();
            foreach (WorkGiver giver in pawn.AssignedToolRelevantWorkGivers())
                foreach (StatDef stat in giver.def.GetModExtension<WorkGiverExtension>().requiredStats)
                {
                    string gerundLabel = giver.def.workType.gerundLabel;
                    if (!pawn.HasSurvivalToolFor(stat) && !types.Contains(gerundLabel))
                        types.Add(gerundLabel);
                }
            return GenText.ToCommaList(types).CapitalizeFirst();
        }

        public override string GetExplanation()
        {
            string result = "ColonistNeedsSurvivalToolDesc".Translate() + ":\n";
            foreach (Pawn pawn in ToollessWorkers)
                result += ("\n    " + pawn.LabelShort + " (" + ToollessWorkTypesString(pawn) + ")");
            return result;
        }

        public override string GetLabel() =>
            ((ToollessWorkers.Count() <= 1) ? "ColonistNeedsSurvivalTool" : "ColonistsNeedSurvivalTool").Translate();

        public override AlertReport GetReport() =>
            AlertReport.CulpritsAre(ToollessWorkers);
    }
}
