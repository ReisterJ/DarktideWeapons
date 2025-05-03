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

        protected Comp_CounterAttack comp_CounterAttack;

        protected int comp_CounterAttackcheckFail = 1;

        protected Comp_DarktidePlasma comp_DarktidePlasma;

        protected int comp_DarktidePlasmacheckFail = 1;

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
           
        }

        public override void CompTick()
        {
            base.CompTick();
            counter++;
            
            if(comp_CounterAttack == null && (counter % (int)Math.Pow(2,comp_CounterAttackcheckFail) == 0) )
            {
               if(comp_CounterAttackcheckFail < maxCheck) comp_CounterAttackcheckFail++;
               comp_CounterAttack = parent.TryGetComp<Comp_CounterAttack>();
               if (comp_CounterAttack != null) comp_CounterAttackcheckFail = 1;
            }
            if(comp_DarktidePlasma == null && (counter % (int)Math.Pow(2, comp_DarktidePlasmacheckFail) == 0))
            {
                if (comp_DarktidePlasmacheckFail < maxCheck) comp_DarktidePlasmacheckFail++;
                comp_DarktidePlasma = parent.TryGetComp<Comp_DarktidePlasma>();
                if(comp_DarktidePlasma != null) comp_DarktidePlasmacheckFail = 1;
            }
            
            if(comp_DarktidePlasma != null)
            {
                comp_DarktidePlasma.wielder = holder;
                comp_DarktidePlasma.CompTick();
            }
            if(counter % 600 == 0 )
            {
               // DEV("Comp_DarktideWeapon is Ticking");

                if (comp_DarktidePlasma != null)
                {
                   // DEV("Comp_Plasma is Ticking");
                }
            }
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
            if(comp_DarktidePlasma != null)
            {
                foreach (var gizmo2 in comp_DarktidePlasma.CompGetGizmosExtra())
                {
                    yield return gizmo2;
                }
            }
            
            yield return new Command_Action
            {
                defaultLabel = "Inspect".Translate(), 
                defaultDesc = "InspectDesc".Translate(), 
                icon = TexCommand.DesirePower, 
                action = new Action( ShowInspectDialog )
            };
        }
        private void ShowInspectDialog()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("Weapon Info:".Translate());
            //info.AppendLine($"Counter: {counter}".Translate());
            //info.AppendLine($"Cleave Targets: {cleaveTargetsinGame}".Translate());
            if (comp_CounterAttack != null)
            {
                info.AppendLine("Enable CounterAttack".Translate());
            }

            
            Find.WindowStack.Add(new Dialog_MessageBox(info.ToString(), title: "Inspect Weapon".Translate()));
        }

        public override bool CompAllowVerbCast(Verb verb)
        {
            if(verb is Verb_LaunchProjectile)
            {
                if (comp_DarktidePlasma != null)
                {
                    if (!comp_DarktidePlasma.CompAllowVerbCast(verb))
                    {
                        Util_Ranged.DEV_output("Plasma Weapon is Cooling or Overheated , Cant shoot now");
                    }
                    return comp_DarktidePlasma.CompAllowVerbCast(verb);
                }
            }
            
            return true;
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
