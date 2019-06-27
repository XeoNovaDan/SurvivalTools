using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SurvivalTools
{

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            // Add validator to ThingSetMakerDef
            ST_ThingSetMakerDefOf.MapGen_AncientRuinsSurvivalTools.root.fixedParams.validator = (ThingDef t) =>
            t.IsWithinCategory(ST_ThingCategoryDefOf.SurvivalToolsNeolithic);

            if (ModCompatibilityCheck.MendAndRecycle)
                ResolveMendAndRecycleRecipes();
            ResolveSmeltingRecipeUsers();
            CheckStuffForStuffPropsTool();

            // Add SurvivalToolAssignmentTracker to all appropriate pawns
            foreach (ThingDef tDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.Humanlike == true))
            {
                Log.Message($"Attaching Pawn_SurvivalToolAssignmentTracker to {tDef}");
                if (tDef.comps == null)
                    tDef.comps = new List<CompProperties>();
                tDef.comps.Add(new CompProperties(typeof(Pawn_SurvivalToolAssignmentTracker)));
            }
        }

        private static void ResolveMendAndRecycleRecipes()
        {
            Log.Message("Culling redundant MendAndRecycle recipes for Survival Tools...");
            int cullCount = 0;
            bool categoryMatch = false;
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(r => r.defName.Contains("SurvivalTool") && r.workerClass != typeof(RecipeWorker)))
            {
                categoryMatch = false;
                foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.thingClass == typeof(SurvivalTool)))
                    if (recipe.IsIngredient(thing))
                    {
                        categoryMatch = true;
                        break;
                    }
                if (!categoryMatch)
                {
                    recipe.recipeUsers.Clear();
                    cullCount++;
                    Log.Message($"Culled recipe: {recipe.defName}");
                }

            }
            Log.Message($"Recipe culling complete. Total recipes culled: {cullCount}");
        }

        private static void ResolveSmeltingRecipeUsers()
        {
            foreach (ThingDef benchDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.IsWorkTable))
                if (benchDef.recipes != null)
                {
                    if (benchDef.recipes.Contains(ST_RecipeDefOf.SmeltWeapon))
                        benchDef.recipes.Add(ST_RecipeDefOf.SmeltSurvivalTool);
                    if (benchDef.recipes.Contains(ST_RecipeDefOf.DestroyWeapon))
                        benchDef.recipes.Add(ST_RecipeDefOf.DestroySurvivalTool);
                }
        }

        private static void CheckStuffForStuffPropsTool()
        {
            StringBuilder stuffBuilder = new StringBuilder();
            stuffBuilder.AppendLine("Checking all stuff for StuffPropsTool modExtension...");
            stuffBuilder.AppendLine();
            StringBuilder hasPropsBuilder = new StringBuilder("Has props:\n");
            StringBuilder noPropsBuilder = new StringBuilder("Doesn't have props:\n");

            List<StuffCategoryDef> toolCats = new List<StuffCategoryDef>();
            foreach (ThingDef tool in DefDatabase<ThingDef>.AllDefsListForReading.Where(t => t.IsSurvivalTool()))
                if (!tool.stuffCategories.NullOrEmpty())
                    foreach (StuffCategoryDef category in tool.stuffCategories)
                        if (!toolCats.Contains(category))
                            toolCats.Add(category);

            foreach (ThingDef stuff in DefDatabase<ThingDef>.AllDefsListForReading.Where(
                (ThingDef t) =>
                {
                    if (!t.IsStuff)
                        return false;
                    bool retVal = false;
                    foreach (StuffCategoryDef stuffCat in t.stuffProps.categories)
                        if (toolCats.Contains(stuffCat))
                        {
                            retVal = true;
                            break;
                        }
                    return retVal;
                }))
            {

                string newLine = $"{stuff} ({stuff.modContentPack.Name})";
                if (stuff.HasModExtension<StuffPropsTool>())
                    hasPropsBuilder.AppendLine(newLine);
                else
                    noPropsBuilder.AppendLine(newLine);
            }

            stuffBuilder.Append(hasPropsBuilder);
            stuffBuilder.AppendLine();
            stuffBuilder.Append(noPropsBuilder);
            Log.Message(stuffBuilder.ToString());
        }

    }


}
