using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Hediff_DWToughnessShield : Hediff_Level
    {
        public Comp_DWToughnessShield CompToughnessShield
        {
            get
            {
                if(this.pawn.TryGetComp<Comp_DWToughnessShield>() != null)
                {
                    return this.pawn.TryGetComp<Comp_DWToughnessShield>();
                }
                return null;
            }
        }

        protected CompProperties_DWToughnessShield compProperties_DWToughnessShield;
        public ModExtension_ToughnessShield ToughnessShieldExtension
        {
            get
            {
                return def.GetModExtension<ModExtension_ToughnessShield>();
            }
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (CompToughnessShield != null)
            {
                this.CompToughnessShield.EnableShield = true;
                float shieldnum = CompToughnessShield.DWTSProp.maxToughnessBase + ToughnessShieldExtension.GetShieldIncrement(level);
                CompToughnessShield.SetMaxShieldInGame(shieldnum);
            }
            /*
            ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(Comp_DWToughnessShield));
            thingComp.parent = pawn;
            pawn.AllComps.Add( thingComp );
            thingComp.Initialize(compProperties_DWToughnessShield);
            */
        }

        public void ChangeLevel(int levelOffset, bool sendLetter)
        {
            int abilityLevel = 0;
            if (levelOffset > 0)
            {
                float num = Math.Min(levelOffset, def.maxSeverity - (float)level);
                for (int i = 0; (float)i < num; i++)
                {
                    abilityLevel = level + 1 + i;
                }
                if (CompToughnessShield != null)
                {
                    this.CompToughnessShield.EnableShield = true;
                    float shieldnum = CompToughnessShield.DWTSProp.maxToughnessBase + ToughnessShieldExtension.GetShieldIncrement(abilityLevel);
                    CompToughnessShield.SetMaxShieldInGame(shieldnum);
                    //MeleeUtil.DEV_output(abilityLevel);
                    //MeleeUtil.DEV_output(this.CompToughnessShield.EnableShield);
                }
            }
            if( levelOffset < 0)
            {
                float num = levelOffset + (float)level;
                if(num > def.minSeverity)
                {
                    if (CompToughnessShield != null)
                    {
                        float shieldnum = CompToughnessShield.DWTSProp.maxToughnessBase + ToughnessShieldExtension.GetShieldIncrement((int)num);
                        CompToughnessShield.SetMaxShieldInGame(shieldnum);
                    }
                }
            }
            base.ChangeLevel(levelOffset);
        }

        public override void ChangeLevel(int levelOffset)
        {
            ChangeLevel(levelOffset, sendLetter: true);
        }

        public string MakeLetterTextNewPsylinkLevel(Pawn pawn, int abilityLevel)
        {
            string text = ((abilityLevel == 1) ? "LetterToughnessShieldLevelGained_First" : "LetterToughnessShieldLevelGained_NotFirst").Translate(pawn.Named("USER"));
           
            text += "\n\n" + "LetterToughnessShieldLevelGained".Translate(pawn.Named("USER"), abilityLevel);
            
            return text;
        }

        public override void PostRemoved()
        {
            if( this.CompToughnessShield != null)
            {
                this.CompToughnessShield.EnableShield = false;
            }
            base.PostRemoved();
        }
    }
}
