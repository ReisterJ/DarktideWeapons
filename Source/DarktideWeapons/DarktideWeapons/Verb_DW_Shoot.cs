using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Verb_DW_Shoot : Verb_LaunchProjectile
    {
        protected override int ShotsPerBurst => verbProps.burstShotCount;


        public Comp_Block BlockComp 
        {
            get 
            {
                return DarktideWeapon.TryGetComp<Comp_Block>();
            }
        }
        public DW_Equipment DarktideWeapon         
        {
            get
            {
                if (EquipmentSource != null && EquipmentSource is DW_Equipment DWE)
                {
                    return DWE;
                }
                return null;
            }
        }
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (currentTarget.Thing is Pawn pawn && CasterIsPawn && CasterPawn.skills != null)
            {
                float num = (pawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
                CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
            }
        }

        public override bool Available()
        {
            bool flag = base.Available() && (BlockComp?.AllowAttackWhileBlocking() ?? true);
            

            return flag;
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            if (caster == null)
            {
                Log.Error("Verb " + GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
                return false;
            }
            if (!caster.Spawned)
            {
                return false;
            }
            if (state == VerbState.Bursting || !CanHitTarget(castTarg))
            {
                return false;
            }
            if (CausesTimeSlowdown(castTarg))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
            this.surpriseAttack = surpriseAttack;
            canHitNonTargetPawnsNow = canHitNonTargetPawns;
            this.preventFriendlyFire = preventFriendlyFire;
            this.nonInterruptingSelfCast = nonInterruptingSelfCast;
            currentTarget = castTarg;
            currentDestination = destTarg;
            if (CasterIsPawn && verbProps.warmupTime > 0f)
            {
                if (!TryFindShootLineFromTo(caster.Position, castTarg, out var resultingLine))
                {
                    return false;
                }
                CasterPawn.Drawer.Notify_WarmingCastAlongLine(resultingLine, caster.Position);
                float statValue = CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
                int ticks = (verbProps.warmupTime * statValue).SecondsToTicks();
                CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
                if (verbProps.stunTargetOnCastStart && castTarg.Pawn != null)
                {
                    castTarg.Pawn.stances.stunner.StunFor(ticks, null, addBattleLog: false);
                }
            }
            else
            {
                if (verbTracker.directOwner is Ability ability)
                {
                    ability.lastCastTick = Find.TickManager.TicksGame;
                }
                WarmupComplete();
            }
            return true;
        }
        protected override bool TryCastShot()
        {
            bool num = base.TryCastShot();
            this.Notify_ProjectileLaunched();
            if (num)
            {
                if (verbProps.consumeFuelPerShot > 0f && caster.TryGetComp<CompRefuelable>() is CompRefuelable compRefuelable)
                {
                    compRefuelable.ConsumeFuel(verbProps.consumeFuelPerShot);
                }
            }
            if (num && CasterIsPawn)
            {
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
            
            return num;
        }

        public virtual void Notify_ProjectileLaunched()
        {

        }
        public virtual void PostProjectileLaunched()
        {

        }

        public virtual void PreProjectileLaunched()
        {

        }
        public virtual bool CausesTimeSlowdown(LocalTargetInfo castTarg)
        {
            if (!verbProps.CausesTimeSlowdown)
            {
                return false;
            }
            if (!castTarg.HasThing)
            {
                return false;
            }
            Thing thing = castTarg.Thing;
            if (thing.def.category != ThingCategory.Pawn && (thing.def.building == null || !thing.def.building.IsTurret))
            {
                return false;
            }
            Pawn pawn = thing as Pawn;
            bool flag = pawn?.Downed ?? false;
            if ((CasterPawn != null && CasterPawn.Faction == Faction.OfPlayer && CasterPawn.IsShambler) || (pawn != null && pawn.Faction == Faction.OfPlayer && pawn.IsShambler))
            {
                return false;
            }
            if (thing.Faction != Faction.OfPlayer || !caster.HostileTo(Faction.OfPlayer))
            {
                if (caster.Faction == Faction.OfPlayer && thing.HostileTo(Faction.OfPlayer))
                {
                    return !flag;
                }
                return false;
            }
            return true;
        }
    }
}
