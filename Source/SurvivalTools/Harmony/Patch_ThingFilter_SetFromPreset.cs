using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;

namespace SurvivalTools.Harmony
{

    [HarmonyPatch(typeof(ThingFilter))]
    [HarmonyPatch(nameof(ThingFilter.SetFromPreset))]
    public static class Patch_ThingFilter_SetFromPreset
    {

        public static void Postfix(ThingFilter __instance, StorageSettingsPreset preset)
        {
            if (preset == StorageSettingsPreset.DefaultStockpile)
                __instance.SetAllow(ST_ThingCategoryDefOf.SurvivalTools, true);
        }

    }

}
