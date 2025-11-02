using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace DarktideWeapons.JobDrivers
{
    public class JobDriver_SpecialMeleeAttack : JobDriver_CastAbility
    {
        private const TargetIndex TargetPawnIndex = TargetIndex.A;

        protected Thing Target => job.GetTarget(TargetIndex.A).Thing;

        public Comp_DWChargeWeapon ChargeComp
        {
            get
            {
                return DW_MeleeWeapon?.TryGetComp<Comp_DWChargeWeapon>();
            }
        }
        public DW_Equipment DW_MeleeWeapon
        {
            get
            {
                if(this.pawn.equipment?.Primary is DW_Equipment dw)
                {
                    return dw;
                }
                return null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !job.ability.CanCast && !job.ability.Casting);
            Ability ability = ((Verb_CastAbility)job.verbToUse).ability;
            //yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_General.Wait(45).WithProgressBarToilDelay(TargetIndex.B);
            
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !ability.CanApplyOn(job.targetA));
            yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
        }
    }
}
