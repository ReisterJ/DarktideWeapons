using DarktideWeapons.MeleeComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public class Verb_PushAttack : Verb_AbilityMelee,IPushAttack
    {
        public Comp_PushAttack PushAttackComp
        {
            get
            {
                return DW_equipment.TryGetComp<Comp_PushAttack>();
            }
        }

        public bool TryStartPushAttack()
        {
            if (currentTarget.Pawn == null || currentTarget.Pawn.Dead)
                return false;

            if (!CasterPawn.Spawned)
            {
                return false;
            }
            this.tool = this.PushAttackComp.Props.pushAttackToolPrimary;
            DoPushAttack();
            return true;
        }

        protected void DoPushAttack()
        {
            Pawn targetPawn = currentTarget.Pawn;
            if (targetPawn != null && !targetPawn.Dead)
            {
                Util_Stagger.StunHandler(targetPawn, this.verbProps.burstShotCount * verbProps.ticksBetweenBurstShots, CasterPawn);
                PushAttackStatSet(out float dam, out float ap, out DamageDef damageDef);
                var dinfo = new DamageInfo(damageDef, dam, ap, -1, CasterPawn, hitPart: Util_Melee.TryHitCorePart(CasterPawn, targetPawn));
                targetPawn.TakeDamage(dinfo);
            }
        }
        protected void PushAttackStatSet(out float dam, out float ap, out DamageDef damageDef)
        {
            if (PushAttackComp == null)
            {
                Log.Error("SpecialMelee called but SpecialMeleeComp is null");
                dam = 1f;
                ap = 0.1f;
                damageDef = DamageDefOf.Blunt;
                return;
            }
            dam = this.tool.power * CasterPawn.GetStatValue(StatDefOf.MeleeDamageFactor);
            dam *= CasterPawn.ageTracker.CurLifeStage.meleeDamageFactor;
            dam *= Util_Melee.PawnMeleeLevelDamageMultiplier(CasterPawn) * MeleeDamageMultiplierGlobal;
            ap = this.tool.armorPenetration;
            ap *= DW_equipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier);
            damageDef = this.tool.Maneuvers.FirstOrDefault().verb.meleeDamageDef;
            bool crit = Util_Crit.IsCrit(this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critChance);
            if (crit)
            {
                dam *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critDamageMultiplier;
                ap *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critArmorPenetrationMultiplier;
            }
        }
    }
}
