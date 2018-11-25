using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class SurvivalToolForcedHandler : IExposable
    {

        private List<Thing> forcedTools = new List<Thing>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref forcedTools, "forcedTools", LookMode.Reference);
        }

        public bool IsForced(Thing tool)
        {
            if (tool.Destroyed)
            {
                Log.Error($"SurvivalTool was forced while Destroyed: {tool}");
                if (forcedTools.Contains(tool))
                    forcedTools.Remove(tool);
                return false;
            }
            return forcedTools.Contains(tool);
        }

        public void SetForced(Thing tool, bool forced)
        {
            if (forced && !forcedTools.Contains(tool))
                forcedTools.Add(tool);
            else if (!forced && forcedTools.Contains(tool))
                forcedTools.Remove(tool);
                
        }

        public bool AllowedToAutomaticallyDrop(Thing tool) => !IsForced(tool);

        public void Reset() => forcedTools.Clear();

        public List<Thing> ForcedTools => forcedTools;

        public bool SomethingForced => !forcedTools.NullOrEmpty();

    }
}
