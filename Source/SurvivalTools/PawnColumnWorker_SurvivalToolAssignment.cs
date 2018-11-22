﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public sealed class PawnColumnWorker_SurvivalToolAssignment : PawnColumnWorker
    {

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Pawn_SurvivalToolAssignmentTracker toolAssignmentTracker = pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>();
            if (toolAssignmentTracker == null)
                return;
            int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
            int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
            float num3 = rect.x;
            Rect rect2 = new Rect(num3, rect.y + 2f, (float)num, rect.height - 4f);
            Rect rect3 = rect2;
            Pawn pawn2 = pawn;
            Func<Pawn, SurvivalToolAssignment> getPayload = (Pawn p) => p.TryGetComp<Pawn_SurvivalToolAssignmentTracker>().CurrentSurvivalToolAssignment;
            Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<SurvivalToolAssignment>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<SurvivalToolAssignment>>>(Button_GenerateMenu);
            string buttonLabel = toolAssignmentTracker.CurrentSurvivalToolAssignment.label.Truncate(rect2.width, null);
            string label = toolAssignmentTracker.CurrentSurvivalToolAssignment.label;
            Widgets.Dropdown(rect3, pawn2, getPayload, menuGenerator, buttonLabel, null, label, null, null, true);
            num3 += rect2.width;
            num3 += 4f;
            Rect rect5 = new Rect(num3, rect.y + 2f, (float)num2, rect.height - 4f);
            if (Widgets.ButtonText(rect5, "AssignTabEdit".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ManageSurvivalToolAssignments(toolAssignmentTracker.CurrentSurvivalToolAssignment));
            }
            num3 += (float)num2;
        }

        private IEnumerable<Widgets.DropdownMenuElement<SurvivalToolAssignment>> Button_GenerateMenu(Pawn pawn)
        {
            using (List<SurvivalToolAssignment>.Enumerator enumerator = Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().AllSurvivalToolAssignments.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    SurvivalToolAssignment survivalToolAssignment = enumerator.Current;
                    yield return new Widgets.DropdownMenuElement<SurvivalToolAssignment>
                    {
                        option = new FloatMenuOption(survivalToolAssignment.label,
                        () => pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>().CurrentSurvivalToolAssignment = survivalToolAssignment),
                        payload = survivalToolAssignment
                    };
                }
            }
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));
        }

        public override int GetMinHeaderHeight(PawnTable table)
        {
            return Mathf.Max(base.GetMinHeaderHeight(table), PawnColumnWorker_Outfit.TopAreaHeight);
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private int GetValueToCompare(Pawn pawn)
        {
            return (pawn.TryGetComp<Pawn_SurvivalToolAssignmentTracker>() is Pawn_SurvivalToolAssignmentTracker toolAssignmentTracker && toolAssignmentTracker.CurrentSurvivalToolAssignment != null) ?
                toolAssignmentTracker.CurrentSurvivalToolAssignment.uniqueId : int.MinValue;
        }

    }
}