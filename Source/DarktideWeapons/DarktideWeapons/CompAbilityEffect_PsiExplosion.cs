using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_PsiExplosion : CompAbilityEffect
    {
        protected float RangedDamageMultiplierGlobal => LoadedModManager.GetMod<DW_Mod>().GetSettings<DW_ModSettings>().RangedDamageMultiplierGlobal;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            DoPsiExplosion(target, dest);
        }

        public new CompProperties_AbilityEffect_PsiExplosion Props
        {
            get
            {
                return (CompProperties_AbilityEffect_PsiExplosion)props;
            }
        }
        protected void DoPsiExplosion(LocalTargetInfo target, LocalTargetInfo dest)
        {
            IntVec3 explosionCenter = target.Cell;

            IEnumerable<Thing> targetsInRange = GenRadial.RadialDistinctThingsAround(explosionCenter,this.parent.pawn.Map, Props.explosionRadius, true);

            foreach (Thing thing in targetsInRange)
            {
                if(thing is Pawn || thing is Building)
                {
                    if (!this.parent.pawn.HostileTo(thing))
                    {
                        continue;
                    }
                    float distance = (thing.Position - explosionCenter).LengthHorizontal;

                    float damage = 0f;

                    if (distance <= Props.innerExplosionRadius)
                    {
                        damage = Props.innerExplosionDamage;
                    }
                    else if (distance <= Props.explosionRadius)
                    {
                        damage = Props.explosionDamage;
                    }

                    if (damage > 0)
                    {
                        
                        damage *= RangedDamageMultiplierGlobal;
                        float armorPenetration = Props.explosionArmorPenetration;
                        if(thing is Pawn pawn)
                        {
                            Util_Stagger.StaggerHandler(pawn, Util_Stagger.baseStaggerTick, this.parent.pawn, damage / 10);
                        }

                        DamageInfo damageInfo = new DamageInfo(DamageDefOf.Bomb, damage, armorPenetration, -1, this.parent.pawn);
                        thing.TakeDamage(damageInfo);
                    }
                }
                
            }
        }
    }

    public class CompProperties_AbilityEffect_PsiExplosion : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_PsiExplosion() 
        {
            this.compClass = typeof(CompAbilityEffect_PsiExplosion);
        }
        public float explosionRadius = 3f;
        public float innerExplosionRadius = 1f;

        public float explosionDamage = 20f;
        public float innerExplosionDamage = 40f;

        public float explosionArmorPenetration = 0.5f;

        public float staggerPower = 3f;
    }
}
