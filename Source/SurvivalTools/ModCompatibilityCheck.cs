﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{

    [StaticConstructorOnStartup]
    public static class ModCompatibilityCheck
    {

        public static bool Quarry = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Quarry 1.0");
        public static bool FluffyBreakdowns = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Fluffy Breakdowns");
        public static bool TurretExtensions = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[XND] Turret Extensions");
        public static bool PrisonLabor = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Prison Labor");
        public static bool CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended");
        public static bool PickUpAndHaul = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "PickUpAndHaul");
        public static bool MendAndRecycle = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "MendAndRecycle");
        public static bool OtherInventoryModsActive = CombatExtended || PickUpAndHaul;
        public static bool DubsBadHygiene = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dubs Bad Hygiene");

    }
}
