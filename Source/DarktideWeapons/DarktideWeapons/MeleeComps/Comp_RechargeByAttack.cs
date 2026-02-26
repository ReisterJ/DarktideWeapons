using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace DarktideWeapons.MeleeComps
{
    public class Comp_RechargeByAttack : DW_WeaponComp, IMeleeAttacked
    {
        public CompProperties_RechargeByAttack Props => (CompProperties_RechargeByAttack)this.props;

        public float RechargeAmount => Props.rechargeAmount;

        public float rechargeMultiplier = 1f;
        protected float currentRecharge = 0f;
        public void PostMeleeAttacked(MeleeAttackData data)
        {
            if (Props.allowMultipleHitCharge)
            {
                Charge(data.hitPawnNum * RechargeAmount);
                return;
            }
            Charge(RechargeAmount);
        }

        public void Charge(float amount)
        {
            currentRecharge += amount * rechargeMultiplier;
            if (IsFullyCharged())
            {
                currentRecharge = Props.rechargeFull;
            }
        }

        public void ResetCharge(float f = 0f)
        {
            currentRecharge = f;
        }

        public bool IsFullyCharged()
        {
            return currentRecharge >= Props.rechargeFull;
        }
    }



    public class CompProperties_RechargeByAttack : CompProperties
    {
        public float rechargeAmount = 0.1f;

        public float rechargeFull = 1f;

        public bool allowMultipleHitCharge = false;
        public CompProperties_RechargeByAttack()
        {
            this.compClass = typeof(Comp_RechargeByAttack);
        }

    }
}