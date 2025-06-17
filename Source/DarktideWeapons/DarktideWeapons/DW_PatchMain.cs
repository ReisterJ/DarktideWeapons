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

    
    





}
