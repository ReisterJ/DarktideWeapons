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


    [HarmonyPatch(typeof(ThingDefGenerator_Neurotrainer))]
    [HarmonyPatch("ImpliedThingDefs")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    static class ThingDefGenerator_Neurotrainer_Patch {
       
        [HarmonyPrefix]
        static bool Prefix(ref IEnumerable<ThingDef> __result )
        {
            return true;
        }
    }


    [HarmonyPatch(typeof(VerbTracker), "PrimaryVerb", MethodType.Getter)]
    public static class Patch_VerbTracker_PrimaryVerb
    {
        public static bool Prefix(VerbTracker __instance, ref Verb __result)
        {
            Thing owner = __instance.directOwner as Thing;
            if (owner is DW_Equipment dwEquip && __instance.AllVerbs.Count > 1)
            {
                __result = dwEquip.switchverb ? __instance.AllVerbs[1] : __instance.AllVerbs[0];
                return false;
            }
            return true;
        }
    }






}
