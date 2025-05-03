using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DarktideWeapons
{
    public class DW_Projectile : Bullet
    {
        protected bool willPenetrate = false;

        public int penetrateNum = 0;

        protected int PenetratedTarget = 0;

        protected bool preventFriendlyFireinGame = false;

        public float armorPenetrationinGame = 0f;

        public float critChanceinGame = 0f;

        public float critDamageMultiplierinGame = 1f;

        public float stunChanceinGame = 0f;

        public int stunTicksinGame = 0;

        public float critArmorPenetrationMultiplier = 1f;

        public float DamageMultiplier_Outer = 1f;

        public float DamageMultiplier_Mech = 1f;

        public float DamageMultiplier_Humanlike = 1f;

        public float DamageMultiplier_Animal = 1f;

        //public float DamageMultiplier_Vehicle = 1f;

        public float DamageMultiplier_HeavyArmor = 1f;

        public float DamageMultiplier_Anomoly = 1f;

        public bool penetrateWall = false;

        protected IntVec3 stopPoint = new IntVec3(0, -1, 0);

        protected bool usedTargetHit = false;

        protected bool isLaser = false;

        List<IntVec3> checkedCells = new List<IntVec3>();

        List<Thing> targetsList = new List<Thing>();
        protected ModExtension_ProjectileProperties projectileProps => this.def.GetModExtension<ModExtension_ProjectileProperties>();

        protected float originalTargetDistance = 0;

        protected Sustainer ambientSustainer;

        public float penetrateDamageFalloffRatio = 0.95f;

        public float effectiveRange = 20f;

        protected bool isPlasma = false;
        protected virtual void WeaponQuality_bias()
        {
            switch (this.equipmentQuality)
            {
                case QualityCategory.Excellent:
                    this.critChanceinGame = this.projectileProps.critChance * Util_Ranged.Quality_Excellent_Multiplier;
                    this.stunChanceinGame = this.projectileProps.stunChance * Util_Ranged.Quality_Master_Multiplier;
                    break;

                case QualityCategory.Masterwork:
                    this.critChanceinGame = this.projectileProps.critChance * Util_Ranged.Quality_Master_Multiplier;
                    this.stunChanceinGame = this.projectileProps.stunChance * Util_Ranged.Quality_Master_Multiplier;
                    break;
                case QualityCategory.Legendary:
                    this.stunChanceinGame = this.projectileProps.stunChance * Util_Ranged.Quality_Legendary_Multiplier;
                    this.stunTicksinGame = (int)((float)this.projectileProps.stunTicks * Util_Ranged.Quality_Legendary_Stun_Tick_Multiplier);
                    this.critChanceinGame = this.projectileProps.critChance * Util_Ranged.Quality_Legendary_Multiplier;
                    this.critDamageMultiplierinGame = this.projectileProps.critDamageMultiplier * Util_Ranged.Quality_Legendary_Multiplier;
                    break;

                default:

                    break;
            }
        }
        public override void Tick()
        {
            /*
            if (this.Position == stopPoint)
            {
                Util_Ranged.DEV_output("Projectile reaches stop point : " + stopPoint);
                lifetime = 0;
                ticksToImpact = 0;
                this.Destroy();
                //this.ImpactSomething();
                return;
            }
           */
            if(this.penetrateNum <= 0)
            {
                base.Tick();
                return;
            }


            lifetime--;
            
            if (landed)
            {
                //return;
            }
            Vector3 exactPosition = ExactPosition;
            ticksToImpact--;
            if (!ExactPosition.InBounds(base.Map))
            {
                ticksToImpact++;
                base.Position = ExactPosition.ToIntVec3();
                Destroy();
                return;
            }
            Vector3 exactPosition2 = ExactPosition;
            if (this.CheckForFreeInterceptBetween(exactPosition, exactPosition2))
            {
                return;
            }
            base.Position = ExactPosition.ToIntVec3();
            if (ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && def.projectile.soundImpactAnticipate != null)
            {
                def.projectile.soundImpactAnticipate.PlayOneShot(this);
            }
            
            if (ticksToImpact <= 0)
            {
                if (DestinationCell.InBounds(base.Map))
                {
                    base.Position = DestinationCell;
                }
                ImpactSomething();
            }
            
            else if (ambientSustainer != null)
            {
                ambientSustainer.Maintain();
            }
            if(lifetime <= 0)
            {
               this.Destroy();
            }
        }
        protected float FlyingTicks
        {
            get
            {
                float num = this.projectileProps.effectiveRange / def.projectile.SpeedTilesPerTick;
                if (num <= 0f)
                {
                    num = 0.001f;
                }
                return num;
            }
        }

        protected void Initiate(Thing equipment = null)
        {
            if (this.projectileProps != null)
            {
                this.critChanceinGame = this.projectileProps.critChance;
                this.critDamageMultiplierinGame = this.projectileProps.critDamageMultiplier;
                this.stunChanceinGame = this.projectileProps.stunChance;
                this.stunTicksinGame = this.projectileProps.stunTicks;
                this.critArmorPenetrationMultiplier = this.projectileProps.critArmorPenetrationMultiplier;
                this.penetrateNum = this.projectileProps.penetrationPower;
                this.penetrateWall = this.projectileProps.penetrateWall;
                this.effectiveRange = this.projectileProps.effectiveRange;
                this.armorPenetrationinGame = this.ArmorPenetration;
            }
            if (equipment != null)
            {
                Util_Ranged.DEV_output("Projectile launch equipment : " + equipment.def.label);
                if (equipment is ThingWithComps thingWithComps)
                {
                    Comp_DarktidePlasma compPlasma = thingWithComps.TryGetComp<Comp_DarktidePlasma>();
                    if (compPlasma != null)
                    {
                        this.isPlasma = true;
                        Util_Ranged.DEV_output("Plasma weapon found , name : " + thingWithComps.Label);
                        Util_Ranged.DEV_output("Plasma weapon mode : " + compPlasma.plasmaWeaponMode.ToString());
                        if(compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal)
                        {
                            this.penetrateWall = false;
                        }
                        if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged)
                        {
                            this.DamageMultiplier_Outer *= compPlasma.chargedModeDamageMultiplier;
                            this.armorPenetrationinGame *= compPlasma.chargedModeArmorPenetrationMultiplier;
                            this.penetrateNum *= 2;
                            this.penetrateWall = true;
                            this.effectiveRange *= 1.2f;
                        }
                        compPlasma.HeatBuild();
                    }
                }

            }
            Util_Ranged.DEV_output("-----PROJECTILE INIT COMPLETED-----");
        }

        public void DrawProjectilePath(IntVec3 origin, IntVec3 dest, float range, Color color)
        {
            
            Vector3 direction = (dest - origin).ToVector3().normalized;

            
            Vector3 endPoint = origin.ToVector3() + direction * range;

           
            GenDraw.DrawLineBetween(origin.ToVector3Shifted(), endPoint, SimpleColor.Blue);

        }
        // Damage calculation
        protected virtual DamageInfo CalculateDamage(Thing hitThing)
        {
            bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
            float damageAmount = this.DamageAmount;
            float armorPenetration = this.armorPenetrationinGame;
            if (Util_Crit.IsCrit(this.critChanceinGame))
            {
                damageAmount *= this.critDamageMultiplierinGame;
                armorPenetration *= this.critArmorPenetrationMultiplier;
                Util_Crit.CritMoteMaker(hitThing);
            }
            if(this.penetrateNum > 0)
            {
                int temp = Math.Max(this.PenetratedTarget - 1, 0);
                damageAmount *= Mathf.Pow(this.penetrateDamageFalloffRatio, temp);
            }
            
            damageAmount *= DamageMultiplier_Outer;
            DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, damageAmount, armorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            return dinfo;
        }

        // try get the target list in the projectile flying path
        public List<Thing> GetShotLineTargetList(List<IntVec3> cells)
        {
            List<Thing> list = new List<Thing>();
            int tempPenetratNum = 0;
            IntVec3 origincell = origin.ToIntVec3();
            foreach (IntVec3 cell in cells)
            {
                if((cell - origincell).Magnitude <= originalTargetDistance)
                {
                    continue;
                }
                List<Thing> tempThings = cell.GetThingList(base.Map);
                foreach (Thing thing in tempThings)
                {

                    if (penetrateNum <= tempPenetratNum)
                    {
                        return list;
                    }
                    if (thing == null || thing == launcher || thing == intendedTarget.Thing || thing == usedTarget.Thing)
                    {
                        continue;
                    }
                    if (thing is Pawn pawn && pawn.GetPosture() == 0)
                    {
                        tempPenetratNum++;
                        Util_Ranged.DEV_output("Penetrate pawn , victim name : " + pawn.Name + " victim pos : " + cell);
                        list.Add(thing);
                    }
                    if (thing is Building building && !this.penetrateWall)
                    {
                        //before hitting the main target, buildings won't be seen as a target 
                        //you won't shoot a thing without line of sight if your weapon can't penetrate the obstacle
                        //After that , projectile goes through the intended target may hit wall or something then stops
                        bool penetrateCheck = false;
                        float penetrateChance = Util_Ranged.PenetrateWall_Probability_Base;
                        if (launcher is Pawn shooter)
                        {
                            int shootLevel = shooter.skills.GetSkill(SkillDefOf.Shooting).Level;
                            int intellectLevel = shooter.skills.GetSkill(SkillDefOf.Intellectual).Level;
                            penetrateChance *= (shootLevel * 5f + intellectLevel * 2f);
                        }
                        if (building.def.passability == Traversability.Impassable && ((cell - origincell).Magnitude > originalTargetDistance))
                        {
                            list.Add(thing);
                            if (!Rand.Chance(penetrateChance))
                            {
                                stopPoint = cell;
                                Util_Ranged.DEV_output("Hitting wall,projectile stop point : " + cell);
                                return list;
                            }
                            tempPenetratNum++;
                        }
                        if (building.def.passability != Traversability.Impassable && ((cell - origincell).Magnitude > originalTargetDistance))
                        {

                            list.Add(thing);
                            if (Rand.Chance(building.def.fillPercent) && !Rand.Chance(penetrateChance))
                            {
                                stopPoint = cell;
                                Util_Ranged.DEV_output("Hitting low cover,penetrate check failed , projectile stop point : " + cell);
                                return list;
                            }
                            tempPenetratNum++;
                        }
                    }
                }
            }
            return list;
        }




        // After launch , the projectile will penetrate the target and hit the next target
        // The projectile will stop when it hits a wall or a target that cannot be penetrated
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (projectileProps != null)
            {
                this.Initiate(equipment);

            }
            Util_Ranged.DEV_output("originTargetCell : " + usedTarget.Cell);
            if (penetrateNum > 0)
            {
                List<IntVec3> cells = Util_Ranged.GetLineSegmentCells(origin.ToIntVec3(), usedTarget.Cell, projectileProps.effectiveRange);
                IntVec3 cellLast = cells.LastOrDefault();
                //IntVec3 adjcentCellRandom = GenAdj.RandomAdjacentCell8Way(cellLast);
                Util_Ranged.DEV_output("newTargetCell : " + cellLast);
                base.Launch(launcher, origin, cellLast, intendedTarget, ProjectileHitFlags.IntendedTarget, preventFriendlyFire, equipment, targetCoverDef);
            }
            else
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            //originalTargetDistance = (origin.ToIntVec3() - destination.ToIntVec3()).Magnitude;
            
            if (!def.projectile.soundAmbient.NullOrUndefined())
            {
                ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            Util_Ranged.DEV_output("-----LAUNCH COMPLETED-----");
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            
            Map map = base.Map;
            IntVec3 position = base.Position;
            GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
            if (!blockedByShield && def.projectile.landedEffecter != null)
            {
                def.projectile.landedEffecter.Spawn(base.Position, base.Map).Cleanup();
            }
            Util_Ranged.DEV_output("Impact something");
            Util_Ranged.DEV_output("PenetrateNum : " + this.penetrateNum);  
            Util_Ranged.DEV_output("PenetratedTarget : " + this.PenetratedTarget);
            Util_Ranged.DEV_output("Lifetime : " + this.lifetime);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            NotifyImpact(hitThing, map, position);
            if (this.PenetratedTarget >= penetrateNum)
            {
                this.Destroy();
            }
            if (hitThing != null)
            {
                Util_Ranged.DEV_output("impact thing : " + hitThing);
                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = CalculateDamage(hitThing);
                dinfo.SetWeaponQuality(equipmentQuality);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn2 = hitThing as Pawn;
                pawn2?.stances?.stagger.Notify_BulletImpact(this);
                if (def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                }
                if (Rand.Chance(def.projectile.bulletChanceToStartFire) && (pawn2 == null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(pawn2))))
                {
                    hitThing.TryAttachFire(def.projectile.bulletFireSizeRange.RandomInRange, launcher);
                }
                return;
            }
            if (!blockedByShield)
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(DamageAmount) * 1f, 4f);
                }
                else
                {
                    FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
                }


            }
            else
            {

            }
            if (Rand.Chance(def.projectile.bulletChanceToStartFire))
            {
                FireUtility.TryStartFireIn(base.Position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
            }
        }

        protected virtual new void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
        {
            BulletImpactData bulletImpactData = default(BulletImpactData);
            bulletImpactData.bullet = this;
            bulletImpactData.hitThing = hitThing;
            bulletImpactData.impactPosition = position;
            BulletImpactData impactData = bulletImpactData;
            hitThing?.Notify_BulletImpactNearby(impactData);
            int num = 9;
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = position + GenRadial.RadialPattern[i];
                if (!c.InBounds(map))
                {
                    continue;
                }
                List<Thing> thingList = c.GetThingList(map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (thingList[j] != hitThing)
                    {
                        thingList[j].Notify_BulletImpactNearby(impactData);
                    }
                }
            }
        }

        
        protected override void ImpactSomething()
        {
            usedTargetHit = true;
            //mortar
            if (def.projectile.flyOverhead)
            {
                RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
                if (roofDef != null)
                {
                    if (roofDef.isThickRoof)
                    {
                        //ThrowDebugText("hit-thick-roof", base.Position);
                        if (!def.projectile.soundHitThickRoof.NullOrUndefined())
                        {
                            def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(base.Position, base.Map));
                        }
                        Destroy();
                        return;
                    }
                    if (base.Position.GetEdifice(base.Map) == null || base.Position.GetEdifice(base.Map).def.Fillage != FillCategory.Full)
                    {
                        RoofCollapserImmediate.DropRoofInCells(base.Position, base.Map);
                    }
                }
            }

            if (usedTarget.HasThing && CanHit(usedTarget.Thing))
            {
                if (usedTarget.Thing is Pawn p && p.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && !Rand.Chance(0.5f))
                {
                    //ThrowDebugText("miss-laying", base.Position);
                    Impact(null);
                }
                else
                {
                    Impact(usedTarget.Thing);
                }
                return;
            }
            List<Thing> list = VerbUtility.ThingsToHit(base.Position, base.Map, CanHit);
            list.Shuffle();
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                float num;
                if (thing is Pawn pawn)
                {
                    num = 0.5f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f)
                    {
                        num *= 0.5f;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        num *= VerbUtility.InterceptChanceFactorFromDistance(origin, base.Position);
                    }
                }
                else
                {
                    num = 1.5f * thing.def.fillPercent;
                }
                if (Rand.Chance(num))
                {
                    //ThrowDebugText("hit-" + num.ToStringPercent(), base.Position);
                    Impact(list.RandomElement());
                    return;
                }
                //ThrowDebugText("miss-" + num.ToStringPercent(), base.Position);
            }
            Impact(null);
        }

        protected void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), base.Map, text);
            }
        }

        

        protected virtual bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            if (lastExactPos == newExactPos)
            {
                return false;
            }
            List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);

            //laser will penetrate the shield
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, lastExactPos, newExactPos) && !penetrateWall)
                {
                    Impact(null, blockedByShield: true);
                    return true;
                }
            }

            IntVec3 intVecLastExactPos = lastExactPos.ToIntVec3();
            IntVec3 intVecNewExactPos = newExactPos.ToIntVec3();
            if (intVecNewExactPos == intVecLastExactPos)
            {
                return false;
            }
            if (!intVecLastExactPos.InBounds(base.Map) || !intVecNewExactPos.InBounds(base.Map))
            {
                return false;
            }
            if (intVecNewExactPos.AdjacentToCardinal(intVecLastExactPos))
            {
                return CheckForFreeIntercept(intVecNewExactPos);
            }
            if (VerbUtility.InterceptChanceFactorFromDistance(origin, intVecNewExactPos) <= 0f)
            {
                return false;
            }
            Vector3 vect = lastExactPos;
            Vector3 v = newExactPos - lastExactPos;
            Vector3 vector = v.normalized * 0.2f;
            int num = (int)(v.MagnitudeHorizontal() / 0.2f);
            checkedCells.Clear();
            int num2 = 0;
            IntVec3 intVec3;
            do
            {
                vect += vector;
                intVec3 = vect.ToIntVec3();
                if (!checkedCells.Contains(intVec3))
                {
                    if (CheckForFreeIntercept(intVec3))
                    {
                        return true;
                    }
                    checkedCells.Add(intVec3);
                }
                num2++;
                if (num2 > num)
                {
                    return false;
                }
            }
            while (!(intVec3 == intVecNewExactPos));
            return false;
        }

        protected virtual bool CheckForFreeIntercept(IntVec3 c)
        {
            if (destination.ToIntVec3() == c)
            {
                return false;
            }
            /*
            float num = VerbUtility.InterceptChanceFactorFromDistance(origin, c);
            if (num <= 0f)
            {
                return false;
            }
            */
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (!CanHit(thing))
                {
                    continue;
                }
                //wall hit check
                bool openDoorHitFlag = false;
                if (thing.def.Fillage == FillCategory.Full)
                {
                    if (thing is Building_Door Door && Door.Open == true)
                    {
                        openDoorHitFlag = true;
                    }
                    else
                    {
                        if (penetrateWall)
                        {
                            Util_Ranged.DEV_output("Penetrate Wall");
                            if (penetrateNum > 0)
                            {
                                PenetratedTarget++;
                            }
                            Impact(thing);
                            return true;
                        }
                        ThrowDebugText("int-wall", c);
                        if(penetrateNum > 0)
                        {
                            PenetratedTarget = 9999;
                        }
                        Impact(thing);
                        return true;
                    }
                }
                float pawnHitProbability = 0f;
                float coverHitProbablility = 0f;
                if (thing is Pawn pawn)
                {
                    pawnHitProbability = Util_Ranged.Intercept_PawnBodySize_Factor * Mathf.Clamp(pawn.BodySize, 0.5f, 3f);
                    if (pawn.GetPosture() != 0)
                    {
                        pawnHitProbability *= Util_Ranged.Intercept_PawnPosture_Downed_Factor;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        if (preventFriendlyFire)
                        {
                            pawnHitProbability = 0f;
                            ThrowDebugText("ff-miss", c);
                        }
                        else
                        {
                            pawnHitProbability *= Find.Storyteller.difficulty.friendlyFireChanceFactor;
                        }
                    }
                    if (pawnHitProbability > 1E-05f)
                    {
                        if(penetrateNum > 0)
                        {
                            PenetratedTarget++;
                            Util_Ranged.DEV_output("Penetrate pawn , victim name : " + pawn.Name + " victim pos : " + c);
                            Impact(thing);

                            return true;
                        }
                        if (Rand.Chance(pawnHitProbability))
                        {
                            ThrowDebugText("int-" + pawnHitProbability.ToStringPercent(), c);
                            Util_Ranged.DEV_output("Penetrate pawn , victim name : " + pawn.Name + " victim pos : " + c);
                            Impact(thing);
                            
                            return true;
                        }
                        ThrowDebugText(pawnHitProbability.ToStringPercent(), c);
                    }

                }
                else if (thing.def.fillPercent > 0.25f)
                {

                    coverHitProbablility = (openDoorHitFlag ? 0.05f : 
                        ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * Util_Ranged.CoverHitFactor_NotCloseToTarget) : 
                        (thing.def.fillPercent * Util_Ranged.CoverHitFactor_CloseToTarget)));
                    if (coverHitProbablility > 1E-05f)
                    {
                        if (Rand.Chance(coverHitProbablility))
                        {
                            if (penetrateWall)
                            {
                                if (penetrateNum > 0)
                                {
                                    PenetratedTarget++;
                                }
                                Impact(thing);
                                return true;
                            }
                            else
                            {
                                //if penetrated
                                if (Rand.Chance(GetCoverPenetrationChance(thing)))
                                {
                                    if (penetrateNum > 0)
                                    {
                                        PenetratedTarget++;
                                    }
                                }
                                //blocked
                                else
                                {
                                    PenetratedTarget = 9999;
                                }

                                

                                ThrowDebugText("int-" + coverHitProbablility.ToStringPercent(), c);
                                Impact(thing);
                                return true;
                            }

                            
                        }
                        
                        ThrowDebugText(coverHitProbablility.ToStringPercent(), c);
                    }
                }
                //pawnHitProbability *= num;
                
            }
            
            return false;
        }

        public virtual float GetCoverPenetrationChance(Thing thing)
        {
            float chance = 0.8f;
            if(thing.def.fillPercent > 0.2f)
            {
                chance =(( Math.Max(this.penetrateNum - this.PenetratedTarget,0)) * this.DamageAmount / thing.HitPoints);
            }
            return chance;
        }
    }
}