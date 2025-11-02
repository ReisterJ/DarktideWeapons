using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Security.Policy;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public abstract class Verb_AbilityMelee : Verb_CastAbility
    {
        public float MeleeDamageMultiplierGlobal => LoadedModManager.GetMod<DW_Mod>().GetSettings<DW_ModSettings>().MeleeDamageMultiplierGlobal;

        public float MeleeAPMultiplierGlobal => LoadedModManager.GetMod<DW_Mod>().GetSettings<DW_ModSettings>().MeleeAPMultiplierGlobal;

        public DW_Equipment DW_equipment => this.CasterPawn?.equipment.Primary as DW_Equipment;



        
    }
}
