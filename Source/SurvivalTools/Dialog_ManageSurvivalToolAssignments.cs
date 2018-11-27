using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{
    public class Dialog_ManageSurvivalToolAssignments : Window
    {

        private Vector2 scrollPosition;
        private static ThingFilter survivalToolGlobalFilter;
        private SurvivalToolAssignment selSurvivalToolAssignmentInt;

        public const float TopAreaHeight = 40f;
        public const float TopButtonHeight = 35f;
        public const float TopButtonWidth = 150f;

        public Dialog_ManageSurvivalToolAssignments(SurvivalToolAssignment selectedToolAssignment)
        {
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            if (survivalToolGlobalFilter == null)
            {
                survivalToolGlobalFilter = new ThingFilter();
                survivalToolGlobalFilter.SetAllow(ST_ThingCategoryDefOf.SurvivalTools, true);
            }
            SelectedSurvivalToolAssignment = selectedToolAssignment;
        }

        private SurvivalToolAssignment SelectedSurvivalToolAssignment
        {
            get => selSurvivalToolAssignmentInt;
            set
            {
                CheckSelectedSurvivalToolAssignmentHasName();
                selSurvivalToolAssignmentInt = value;
            }
        }

        private void CheckSelectedSurvivalToolAssignmentHasName()
        {
            if (SelectedSurvivalToolAssignment?.label.NullOrEmpty() == true)
                SelectedSurvivalToolAssignment.label = "Unnamed";
        }

        public override Vector2 InitialSize => new Vector2(700f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            float num = 0f;
            Rect rect = new Rect(0f, 0f, TopButtonWidth, TopButtonHeight);
            num += TopButtonWidth;
            if (Widgets.ButtonText(rect, "SelectSurvivalToolAssignment".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (SurvivalToolAssignment localOut3 in Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().AllSurvivalToolAssignments)
                {
                    SurvivalToolAssignment localOut = localOut3;
                    list.Add(new FloatMenuOption(localOut.label, delegate ()
                    {
                        SelectedSurvivalToolAssignment = localOut;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            num += 10f;
            Rect rect2 = new Rect(num, 0f, TopButtonWidth, TopButtonHeight);
            num += TopButtonWidth;
            if (Widgets.ButtonText(rect2, "NewSurvivalToolAssignment".Translate(), true, false, true))
            {
                SelectedSurvivalToolAssignment = Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().MakeNewSurvivalToolAssignment();
            }
            num += 10f;
            Rect rect3 = new Rect(num, 0f, TopButtonWidth, TopButtonHeight);
            num += TopButtonWidth;
            if (Widgets.ButtonText(rect3, "DeleteSurvivalToolAssignment".Translate(), true, false, true))
            {
                List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                foreach (SurvivalToolAssignment localOut2 in Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().AllSurvivalToolAssignments)
                {
                    SurvivalToolAssignment localOut = localOut2;
                    list2.Add(new FloatMenuOption(localOut.label, delegate ()
                    {
                        AcceptanceReport acceptanceReport = Current.Game.GetComponent<SurvivalToolAssignmentDatabase>().TryDelete(localOut);
                        if (!acceptanceReport.Accepted)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
                        }
                        else if (localOut == SelectedSurvivalToolAssignment)
                        {
                            SelectedSurvivalToolAssignment = null;
                        }
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list2));
            }
            Rect rect4 = new Rect(0f, TopAreaHeight, inRect.width, inRect.height - TopAreaHeight - this.CloseButSize.y).ContractedBy(10f);
            if (this.SelectedSurvivalToolAssignment == null)
            {
                GUI.color = Color.grey;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, "NoSurvivalToolAssignmentSelected".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
                return;
            }
            GUI.BeginGroup(rect4);
            Rect rect5 = new Rect(0f, 0f, 200f, 30f);
            Dialog_ManageOutfits.DoNameInputRect(rect5, ref this.SelectedSurvivalToolAssignment.label);
            Rect rect6 = new Rect(0f, TopAreaHeight, 300f, rect4.height - 45f - 10f);
            Rect rect7 = rect6;
            ref Vector2 ptr = ref this.scrollPosition;
            ThingFilter filter = this.SelectedSurvivalToolAssignment.filter;
            ThingFilter parentFilter = survivalToolGlobalFilter;
            int openMask = 16;
            ThingFilterUI.DoThingFilterConfigWindow(rect7, ref ptr, filter, parentFilter, openMask);
            GUI.EndGroup();
        }

        public override void PreClose()
        {
            base.PreClose();
            CheckSelectedSurvivalToolAssignmentHasName();
        }

    }
}
