using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using UnityEngine;

namespace DarktideWeapons.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        public static Harmony HarmonyInstance;

        static PatchMain()
        {
            HarmonyInstance = new Harmony("RJ_DarktideWeapons");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

    }


    [HarmonyPatch(typeof(VerbTracker), "PrimaryVerb", MethodType.Getter)]
    public static class Patch_VerbTracker_PrimaryVerb
    {
        //默认两个射击模式
        [HarmonyPostfix]
        public static void Postfix(VerbTracker __instance, ref Verb __result)
        {
            CompEquippable compEquippable = __instance.directOwner as CompEquippable;
            if (compEquippable == null) return ;
            if (compEquippable.parent is DW_Equipment dwEquip && __instance.AllVerbs.Count > 1)
            {
                __result = dwEquip.switchverb ? __instance.AllVerbs[1] : __instance.AllVerbs[0];
                return ;
            }
            //return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public static class Patch_Pawn_TryGetAttackVerb
    {
        //默认两个射击模式
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref Verb __result)
        {
            if(__instance?.equipment?.Primary is DW_Equipment DWequipment)
            {
                if (DWequipment.def.IsRangedWeapon)
                {
                    CompEquippable compEquippable = DWequipment.TryGetComp<CompEquippable>();
                    if (compEquippable == null) return;
                    if (compEquippable.verbTracker.AllVerbs.Count > 1)
                    {
                        __result = DWequipment.switchverb ? compEquippable.verbTracker.AllVerbs[1] : compEquippable.verbTracker.AllVerbs[0];
                        return;
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Verb), "Available")]
    public static class Patch_Verb_Available
    {
        [HarmonyPostfix]
        public static void Postfix(Verb __instance, ref bool __result)
        {

            if (__instance.EquipmentSource.TryGetComp<Comp_Block>()!= null)
            {
                Comp_Block BlockComp = __instance.CasterPawn?.equipment.Primary.TryGetComp<Comp_Block>();
                if (BlockComp.isBlocking && !BlockComp.AllowAttackWhileBlocking())
                {
                    __result = false;
                }
                return;
            }
        }
    }

    // 使用 Postfix 并修改返回的 IEnumerable<ThingDef>
    [HarmonyPatch(typeof(ThingDefGenerator_Neurotrainer))]
    [HarmonyPatch("ImpliedThingDefs")]
    public static class Patch_ThingDefGenerator_Neurotrainer_ImpliedThingDefs
    {
        
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<ThingDef> __result)
        {
            List<ThingDef> filteredDefs = new List<ThingDef>();
            int abilityIndex = 0;
            foreach (ThingDef neurotrainerDef in __result)
            {
                Log.Message("Checking neurotrainer def: " + neurotrainerDef.defName);
                if(!neurotrainerDef.defName.StartsWith(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_"))
                {
                    filteredDefs.Add(neurotrainerDef);
                    continue;
                }
                List<AbilityDef> abilityDefs = DefDatabase<AbilityDef>.AllDefs.ToList();
                string abilityDefName = neurotrainerDef.defName.Replace(ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_", "");
                while(abilityIndex < abilityDefs.Count())
                {
                    Log.Message("Checking abilityDef: " + abilityDefs[abilityIndex].defName);
                    if (abilityDefs[abilityIndex].defName == abilityDefName)
                    {
                        break;
                    }
                    abilityIndex++;
                }
                
                if (!abilityDefs[abilityIndex].HasModExtension<ModExtension_PsyCastExtendedProperties>())
                {
                    filteredDefs.Add(neurotrainerDef);
                }
                else
                {
                    if(abilityDefs[abilityIndex].GetModExtension<ModExtension_PsyCastExtendedProperties>()?.ShouldHaveNeurotrainer == true)
                    {
                        filteredDefs.Add(neurotrainerDef);
                    }
                }
            }
            __result = filteredDefs;
        }
    }





}
