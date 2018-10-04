using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class StatPart_SurvivalTool : StatPart
    {

        public override string ExplanationPart(StatRequest req)
        {
            // The AI will cheat this system for now until tool generation gets figured out
            if (req.Thing is Pawn pawn && pawn.CanUseSurvivalTools() && pawn.Faction == Faction.OfPlayer)
            {
                if (pawn.HasSurvivalToolFor(parentStat, out SurvivalTool tool, out float statFactor))
                    return tool.LabelCapNoCount + ": x" + statFactor.ToStringPercent();
                return "NoTool".Translate() + ": x" + NoToolStatFactor.ToStringPercent();
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && pawn.CanUseSurvivalTools() && pawn.Faction == Faction.OfPlayer)
            {
                if (pawn.HasSurvivalToolFor(parentStat, out SurvivalTool tool, out float statFactor))
                    val *= statFactor;
                else
                    val *= NoToolStatFactor;
            }
        }

        public float NoToolStatFactor =>
            (SurvivalToolsSettings.hardcoreMode) ? NoToolStatFactorHardcore : noToolStatFactor;

        private float noToolStatFactor = 0.3f;

        private float noToolStatFactorHardcore = -1f;
        private float NoToolStatFactorHardcore =>
            (noToolStatFactorHardcore != -1f) ? noToolStatFactorHardcore : noToolStatFactor;

    }
}
