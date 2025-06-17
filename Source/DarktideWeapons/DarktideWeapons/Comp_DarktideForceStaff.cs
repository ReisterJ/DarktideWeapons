using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DarktideForceStaff : DW_WeaponComp
    {
        protected Comp_DarktideWeapon compDarktideWeapon => parent.TryGetComp<Comp_DarktideWeapon>();

        public float CalmEntropy => Props.calmEntrophy;

        public float CalmPsyfocusCost => Props.calmPsyfocusCost;


        public Pawn HoldingPawn
        {

           get
            {
                if (wielder is Pawn pawn) return pawn;
                return null;
            }
            
        }
        public CompProperties_DarktideForceStaff Props
        {
            get
            {
                return (CompProperties_DarktideForceStaff)this.props;
            }
        }

        
        public virtual bool Available
        {
            get
            {
                if (HoldingPawn != null && HoldingPawn.HasPsylink &&
                    HoldingPawn.GetPsylinkLevel() > Props.requirePsyLevel)
                {
                    return true;
                }
                return false;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(! Available) yield break;
            /*
            yield return new Command_Action
            {
                defaultLabel = "DW_CalmEntrophy",
                defaultDesc = "DW_CalmEntrophyDesc",
                icon = TexCommand.DesirePower,
                action = Ability_CalmEntrophy
            };
            */
        }


        public void Ability_CalmEntrophy()
        {
            ReduceEntrophy(GetEntrophyCalmRate(CalmEntropy));
            compDarktideWeapon.HoldingPawn.psychicEntropy.OffsetPsyfocusDirectly((0f - CalmPsyfocusCost) / compDarktideWeapon.HoldingPawn.psychicEntropy.PsychicSensitivity);
        }

        public virtual void ReduceEntrophy(float entrophy)
        {
            if (compDarktideWeapon.HoldingPawn.psychicEntropy.EntropyValue >= entrophy)
            {
                System.Reflection.FieldInfo entrophyfield = typeof(RimWorld.Pawn_PsychicEntropyTracker).GetField("currentEntropy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                entrophyfield?.SetValue(compDarktideWeapon.HoldingPawn.psychicEntropy, compDarktideWeapon.HoldingPawn.psychicEntropy.EntropyValue - entrophy);
            }
            else
            {
                compDarktideWeapon.HoldingPawn.psychicEntropy.RemoveAllEntropy();
            }
        }
        public virtual float GetEntrophyCalmRate(float baseEntrophy)
        {
            int level = compDarktideWeapon.HoldingPawn.GetPsylinkLevel();
            float calmRate = baseEntrophy * level;
            if (this.parent.TryGetQuality(out QualityCategory category))
            {
                switch (category)
                { 
                    case QualityCategory.Awful:
                        calmRate *= 0.5f;
                        break;
                    case QualityCategory.Poor:
                        calmRate *= 0.8f;
                        break;
                    case QualityCategory.Normal:
                        calmRate *= 1.0f;
                        break;
                    case QualityCategory.Good:
                        calmRate *= 1.1f;
                        break;
                    case QualityCategory.Excellent:
                        calmRate *= 1.25f;
                        break;
                    case QualityCategory.Masterwork:
                        calmRate *= 1.35f;
                        break;
                    case QualityCategory.Legendary:
                        calmRate *= 1.5f;
                        break;
                }
            }
            return calmRate;
        }

        public void DOT_QualityOffset(ref int addLevel)
        {
            if (this.parent.TryGetQuality(out QualityCategory category))
            {
                switch (category)
                {
                    case QualityCategory.Masterwork:
                        addLevel += 1;
                        break;
                    case QualityCategory.Legendary:
                        addLevel += 2;
                        break;
                }
            }
        }

        public void AOE_QualityOffset(ref float radius)
        {
            if (this.parent.TryGetQuality(out QualityCategory category))
            {
                switch (category)
                { 
                    case QualityCategory.Excellent:
                        radius *= 1.1f;
                        break;
                    case QualityCategory.Masterwork:
                        radius *= 1.25f;
                        break;
                    case QualityCategory.Legendary:
                        radius *= 1.5f;
                        break;
                }
            }
        }

        public void ChainTarget_QualityOffset(ref int num)
        {
            if (this.parent.TryGetQuality(out QualityCategory category))
            {
                switch (category)
                {
                    case QualityCategory.Masterwork:
                        num += 1;
                        break;
                    case QualityCategory.Legendary:
                        num += 2;
                        break;
                }
            }
        }
        public override string ShowInfo(Thing wielder)
        {
            string header = "ForceStaff".Translate();

            return header;
        }
    }

    public class CompProperties_DarktideForceStaff : CompProperties
    {

        public CompProperties_DarktideForceStaff()
        {
            this.compClass = typeof(Comp_DarktideForceStaff);
        }

        public float spawnEntrophy = 10f;

        public float calmEntrophy = 5f;

        public int requirePsyLevel = 1;

        public float calmPsyfocusCost = 0.05f;
    }
}
