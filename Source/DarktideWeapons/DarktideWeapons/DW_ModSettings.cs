using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{
    public class DW_ModSettings : ModSettings
    {
        public float RangedDamageMultiplierGlobal = 1f;

        public float MeleeDamageMultiplierGlobal = 1f;

        public int DEBUGLEVEL = 0;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref RangedDamageMultiplierGlobal, "RangedDamageMultiplierGlobal",1f);
            Scribe_Values.Look(ref MeleeDamageMultiplierGlobal, "MeleeDamageMultiplierGlobal", 1f);
            
            base.ExposeData();
        }
    }

    [StaticConstructorOnStartup]
    public class DW_Mod : Mod
    {
        DW_ModSettings settings;
        public DW_Mod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<DW_ModSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            //listingStandard.CheckboxLabeled("exampleBoolExplanation", ref settings.exampleBool, "exampleBoolToolTip");
            listingStandard.Label("RangedDamageMultiplierGlobalLabel".Translate() + " : " + settings.RangedDamageMultiplierGlobal);
            settings.RangedDamageMultiplierGlobal = listingStandard.Slider(settings.RangedDamageMultiplierGlobal, 0.5f, 10f);
            listingStandard.Label("MeleeDamageMultiplierGlobalLabel".Translate() + " : " + settings.MeleeDamageMultiplierGlobal);
            settings.MeleeDamageMultiplierGlobal = listingStandard.Slider(settings.MeleeDamageMultiplierGlobal, 0.5f, 10f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "DW_ModName".Translate();
        }
    }
}
