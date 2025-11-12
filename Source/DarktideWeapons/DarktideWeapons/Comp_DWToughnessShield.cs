using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace DarktideWeapons
{
    [StaticConstructorOnStartup]
    public class Comp_DWToughnessShield: ThingComp
    {
        private Vector3 impactAngleVect;

        protected float energy;

        protected int lastAbsorbDamageTick = -9999;

        public float KillRechargeinGame = 30f;
        
        protected int StartTickstoResetinGame;
        
        protected float MaxToughnessinGame;
        
        public float ToughnessDamageReductionMultiplierinGame;

        protected bool AbletoCounter = false;

        protected bool enableShield = false;

        protected int ticksToReset = -1;

        protected float toughnessExceeded = 0f;

        protected Hediff_DWToughnessShield cachedHediffToughnessShield;

        protected float baseToughnessRegenerationRate;

        protected Comp_CounterAttack compCounterAttack;

        public Comp_Block compBlock;

        //public Comp_DarktideWeapon CompDarktideWeaponPrimary => this.PawnOwner.equipment.Primary.TryGetComp<Comp_DarktideWeapon>();

        private int interval = -1;

        protected bool isFullShield = false;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            
            KillRechargeinGame = DWTSProp.killRechargeToughnessBase;
            StartTickstoResetinGame = DWTSProp.startTickstoResetBase;
            MaxToughnessinGame = DWTSProp.maxToughnessBase;
            ToughnessDamageReductionMultiplierinGame = DWTSProp.toughnessDamageReductionMultiplier;
            enableShield = DWTSProp.spawnEnableShield;
            baseToughnessRegenerationRate = DWTSProp.baseToughnessRegenerationRate;
            base.PostSpawnSetup(respawningAfterLoad);
            //MeleeUtil.DEV_output("postspawn");
        }

        
        public CompProperties_DWToughnessShield DWTSProp
        {
            get
            {
                return (CompProperties_DWToughnessShield)this.props;
            } 
        }
        public float MaxToughness
        {
            get
            {
                return this.MaxToughnessinGame;
            }
        }
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public bool EnableShield
        {
            get
            {
                return this.enableShield;
            }
            set
            {
                this.enableShield = value;
            }
        }
       
        public ShieldState ShieldState
        {
            get
            {
                if (PawnOwner == null || !this.enableShield)
                {
                    return ShieldState.Disabled;
                }

                if (this.energy > 0 || this.ticksToReset < 0)
                {
                    return ShieldState.Active;
                }

                return ShieldState.Resetting;
            }
        }

      
        public virtual Pawn PawnOwner
        {
            get
            {
                if (this.parent is Pawn pawn)
                {
                    return pawn;
                }
                return null;


            }
        }

        protected bool ShouldDisplay
        {
            get
            {
                Pawn pawnOwner = this.PawnOwner;
                return pawnOwner.Spawned && !pawnOwner.Dead && !pawnOwner.Downed && this.enableShield
                        //&& (pawnOwner.InAggroMentalState || pawnOwner.Drafted || (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner) 
                        //|| Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks
                        //|| (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == pawnOwner))
                        ;
            }
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;
            
            yield break;
            //yield break;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            
            if (PawnOwner != null) {

                foreach (Gizmo gizmo2 in this.GetGizmos())
                {
                    yield return gizmo2;
                }

            }
            if (!this.enableShield)
            {
                yield break;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Break",
                    action = new Action(this.ToughnessBreak)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Recharge",
                    action = new Action(this.Recharge)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Output",
                    action = new Action(this.DEVoutput)
                };

                if (this.ticksToReset > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Clear reset",
                        action = delegate ()
                        {
                            this.ticksToReset = -1;
                        }
                    };
                }
            }
            yield break;
        }
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            if (Find.Selector.SelectedPawns.Contains( this.PawnOwner))
            {
                if (this.EnableShield)
                {
                    yield return new Gizmo_ToughnessShieldStatus
                    {
                        ToughnessShield = this
                    };
                }

                if (PawnOwner.equipment == null) yield break;
                List<ThingWithComps> list = PawnOwner.equipment.AllEquipmentListForReading;
                if (list == null || list.FirstOrDefault() == null) yield break;
                for (int i = 0; i < list.Count; i++)
                {
                    ThingWithComps thingWithComps = list[i];
                    if(thingWithComps == null || !(thingWithComps is DW_Equipment))
                    {
                        continue;
                    }
                    foreach (Gizmo gizmo in thingWithComps.GetGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }
            yield break;
        }

        public virtual bool WillCounterAttack(DamageInfo dinfo)
        {
            if (PawnOwner.NonHumanlikeOrWildMan() || dinfo.Instigator == null)
            {
                return false;
            }
            compCounterAttack = PawnOwner.equipment.Primary.TryGetComp<Comp_CounterAttack>();
            if (compCounterAttack != null)
            {
                if (compCounterAttack.CanCounterAttack(PawnOwner, dinfo) && dinfo.Instigator is Pawn pawn)
                {
                    compCounterAttack.CounterAttack(PawnOwner, pawn);
                    return true;
                }
            }
            return false;
        }

        public bool BlockDamageCheck(DamageInfo dinfo) 
        {
            if (PawnOwner.NonHumanlikeOrWildMan())
            {
                return false;
            }
            compBlock = PawnOwner.equipment.Primary.TryGetComp<Comp_Block>();
            if (compBlock != null) { 
                bool flag = compBlock.TryBlockDamage(dinfo);
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (BlockDamageCheck(dinfo))
            {
                dinfo.SetAmount(0f);
                absorbed = true;
                return;
            }
            if (WillCounterAttack(dinfo))
            {
                dinfo.SetAmount(0f);
                absorbed = true;
                return;
            }
            this.isFullShield = false;

            if (this.ShieldState != ShieldState.Active )
            {
                return;
            }

            float toughnessdamage = dinfo.Amount * ToughnessDamageReductionMultiplierinGame;
            if (toughnessdamage > this.MaxToughnessinGame) toughnessdamage = this.energy; 
            ticksToReset = this.StartTickstoResetinGame;
            if (Util_Melee.IsMeleeDamage(dinfo))
            {
                toughnessdamage *= 0.9f;
                float PastToughnessDamage = (this.MaxToughnessinGame - this.energy) / this.MaxToughnessinGame * toughnessdamage;
                if (PastToughnessDamage < 0) PastToughnessDamage = 0;
                dinfo.SetAmount(PastToughnessDamage);
                if (this.energy <= toughnessdamage)
                {
                    this.ToughnessBreak();
                }
                else
                {
                    this.energy -= toughnessdamage;
                }
                //Log.Message("after meleedamage : " + dinfo.Amount);
                return;
            }
            if (dinfo.Def == DamageDefOf.Bomb)
            {
                toughnessdamage *= 0.5f;
            }

            if (this.energy < toughnessdamage)
            {
                this.ToughnessBreak();
                dinfo.SetAmount(toughnessdamage - this.energy);
            }
            else
            {
                this.energy -= toughnessdamage;
                absorbed = true;
                //this.ToughnessAbsorbedDamage(dinfo);
            }
           
           
        }
       

        public override void CompTickInterval(int delta)
        {
            //base.CompTickInterval(delta);
            ShieldRegenerateInterval(delta);
        }
        public override void CompTick()
        {
            if (this.PawnOwner == null )
            {
                this.energy = 0f;
                Log.Error("No owner");
                return;
            }
            if (this.PawnOwner.Map == null )
            {
                return;
            }
            //ShieldRegenerateInterval(1);
            //DarktideWeaponCompTick();


        }
        protected void ShieldRegenerateInterval(int val)
        {
            if (enableShield)
            {
                if (this.ShieldState == ShieldState.Resetting)
                {
                    this.ticksToReset-= val;

                    if (this.ticksToReset <= 0)
                    {
                        this.Reset();
                        return;
                    }
                }
                else if (this.ShieldState == ShieldState.Active)
                {
                    if (this.isFullShield) return;
                    if (this.energy >= this.MaxToughnessinGame)
                    {
                        this.energy = this.MaxToughnessinGame;
                        this.isFullShield = true;
                        return;
                    }
                    this.energy += baseToughnessRegenerationRate * val;

                }
            }
        }

        public override float CompGetSpecialApparelScoreOffset()
        {
            return this.MaxToughnessinGame; //* this.ApparelScorePerEnergyMax;
        }

        //maybe some features here... explosion
        public virtual void ToughnessBreak()
        {
            this.energy = 0f;
        }

        public void Recharge()
        {
            this.energy = this.MaxToughnessinGame;
        }
        

        //Toughness doesn't block pawns casting
        

        public virtual void Recharge_Afterkill()
        {
            //float recharge = KillRechargeinGame * victimBodySize;
            float recharge = KillRechargeinGame;
            if (this.energy + recharge <= this.MaxToughnessinGame)
            {
                this.energy += recharge;
                return;
            }
            this.energy = this.MaxToughnessinGame;

        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
           
            if (pawn != null)
            {
                this.Recharge_Afterkill();
            }
        }
          
        public override void PostDraw()
        {
            base.PostDraw();      
        }

        protected void Draw()
        {
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
                Vector3 vector = this.PawnOwner.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            }
        }

        private void Reset()
        {
            //if (this.PawnOwner.Spawned)
            //{
            //    SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(this.PawnOwner.Position, this.PawnOwner.Map, false));
            //    FleckMaker.ThrowLightningGlow(this.PawnOwner.TrueCenter(), this.PawnOwner.Map, 3f);
            //}
            this.ticksToReset = -1;
            this.energy = this.MaxToughnessinGame;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.energy, "energy", 0f);
            Scribe_Values.Look(ref this.ticksToReset, "ticksToReset", -1);
            Scribe_Values.Look(ref this.KillRechargeinGame, "KillRechargeinGame", 0f);
            Scribe_Values.Look(ref this.enableShield, "enableShield", false);
            Scribe_Values.Look(ref this.MaxToughnessinGame, "MaxToughnessinGame", 0f);
            Scribe_Values.Look(ref this.toughnessExceeded, "toughnessExceeded", 0f);
        }
        public void SetMaxShieldInGame(float maxShieldInGame)
        {
            this.MaxToughnessinGame = maxShieldInGame;
        }
        public void DEVoutput()
        {
#if DEBUG
            Log.Message("this DWTcomp parent: " + this.parent.ToString());
            Log.Message("this DWTcomp parent's holder " + this.PawnOwner.Name);
            //Log.Warning("energy :" + this.energy);
            
            //Log.Message("single selected :"+ Find.Selector.SingleSelectedThing.ThingID);
            //Log.Message("this DWTcomp parent's holder id" + this.PawnOwner.ThingID);
#endif        
        }
    }

    public class CompProperties_DWToughnessShield : CompProperties
    {
        public CompProperties_DWToughnessShield()
        {
            this.compClass = typeof(Comp_DWToughnessShield);
        }
        public float toughnessDamageReductionMultiplier = 1.0f;

        public float toughnessRechargeMultiplier = 1.0f;

        public float maxToughnessBase = 100f;

        public int startTickstoResetBase = 900;

        public float killRechargeToughnessBase = 10f;

        public bool spawnEnableShield = false;

        public float baseToughnessRegenerationRate = 1.0f;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_ToughnessShieldStatus : Gizmo
    {
        public Comp_DWToughnessShield ToughnessShield;

        protected static Color ToughnessShieldBarColor = new Color( 165 / 255f , 204 / 255f , 249 / 255f);

        protected static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(ToughnessShieldBarColor);

        protected static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_ToughnessShieldStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3,   "ToughnessShield".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = ToughnessShield.Energy / Mathf.Max(1f, ToughnessShield.MaxToughness);
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = (TextAnchor)4;
            Widgets.Label(rect4, (ToughnessShield.Energy ).ToString("F0") + " / " + (ToughnessShield.MaxToughness).ToString("F0"));
            Text.Anchor = (TextAnchor)0;
            TooltipHandler.TipRegion(rect2, "ToughnessShieldPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }

}
