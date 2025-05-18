using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DarktideWeapon : ThingComp
    {
        protected int counter = 0;

        protected Comp_DWToughnessShield comp_DWToughnessShield;

        protected int comp_DWToughnessShieldcheckFail = 1;

        public Comp_CounterAttack comp_CounterAttack;

        protected int comp_CounterAttackcheckFail = 1;

        public Comp_DarktidePlasma comp_DarktidePlasma;

        protected int comp_DarktidePlasmacheckFail = 1;

        public Comp_DarktideForceStaff comp_DarktideForceStaff;

        protected Thing holder;

        protected int maxCheck = 10;
        public Pawn HoldingPawn
        {
            get
            {
                if(holder is Pawn pawn) return pawn;
                return null;
            }
            set             
            {
                holder = value;
            }
        }
        public CompProperties_DarktideWeapon Props
        {
            get
            {
                return (CompProperties_DarktideWeapon)props;
            }
        }

        public int cleaveTargetsinGame;

        public bool IsWeapon
        {
            get
            {
                if (parent.TryGetComp<CompEquippable>() != null) // equipment check
                {
                    return true;
                }
                return false;
            }
        }
       

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            comp_CounterAttack = parent.TryGetComp<Comp_CounterAttack>();
            comp_DarktidePlasma = parent.TryGetComp<Comp_DarktidePlasma>();
            comp_DarktideForceStaff = parent.TryGetComp<Comp_DarktideForceStaff>();
        }

        public virtual bool QualityUpgrade(QualityCategory q)
        {
            CompQuality compQuality = parent.TryGetComp<CompQuality>();
            if(compQuality == null) return false;
            compQuality.SetQuality(q, ArtGenerationContext.Colony);
            return true;
        }

        public void HolderSet()
        {
            if (comp_DarktidePlasma != null)
            {
                comp_DarktidePlasma.wielder = holder;
            }
            if (comp_DarktideForceStaff != null)
            {
                comp_DarktideForceStaff.wielder = holder;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            counter++;
            /*
            if(comp_CounterAttack == null && (counter % (int)Math.Pow(2,comp_CounterAttackcheckFail) == 0) )
            {
               if(comp_CounterAttackcheckFail < maxCheck) comp_CounterAttackcheckFail++;
               comp_CounterAttack = parent.TryGetComp<Comp_CounterAttack>();
               if (comp_CounterAttack != null) comp_CounterAttackcheckFail = 1;
            }
            */
            if(comp_DarktidePlasma == null && (counter % (int)Math.Pow(2, comp_DarktidePlasmacheckFail) == 0))
            {
                if (comp_DarktidePlasmacheckFail < maxCheck) comp_DarktidePlasmacheckFail++;
                comp_DarktidePlasma = parent.TryGetComp<Comp_DarktidePlasma>();
                if(comp_DarktidePlasma != null) comp_DarktidePlasmacheckFail = 1;
            }
            
            HolderSet();
           
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            comp_DWToughnessShield?.Recharge_Afterkill();
        }

        public override void Notify_Equipped(Pawn pawn)
        {  
            base.Notify_Equipped(pawn);
            //caution here     
            comp_DWToughnessShield = pawn.TryGetComp<Comp_DWToughnessShield>();   
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            
            
            yield return new Command_Action
            {
                defaultLabel = "DWInspectWeapon".Translate(), 
                defaultDesc = "DWInspectWeaponDesc".Translate(), 
                icon = TexCommand.DesirePower, 
                action = new Action( ShowInspectDialog )
            };
        }
        protected void ShowInspectDialog()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("Weapon Info:".Translate());
            //info.AppendLine($"Counter: {counter}".Translate());
            //info.AppendLine($"Cleave Targets: {cleaveTargetsinGame}".Translate());
            if (comp_CounterAttack != null)
            {
                info.AppendLine(comp_CounterAttack.ShowInfo(this.HoldingPawn));
                
            }

            
            Find.WindowStack.Add(new Dialog_MessageBox(info.ToString(), title: "DWInspectWeapon".Translate()));
        }

        
        protected virtual void Dodge()
        { 
            
        }

        protected void DEV(Object o)
        {
#if DEBUG
            Log.Message(o);
#endif
        }

    }

    public class CompProperties_DarktideWeapon : CompProperties
    {
        public CompProperties_DarktideWeapon()
        {
            this.compClass = typeof(Comp_DarktideWeapon);
        }

    }
}
