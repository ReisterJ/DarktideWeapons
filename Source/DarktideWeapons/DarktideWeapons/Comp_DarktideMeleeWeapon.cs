using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DarktideMeleeWeapon : ThingComp
    {
        protected int counter = 0;

        protected Comp_DWToughnessShield comp_DWToughnessShield;

        protected Comp_CounterAttack comp_CounterAttack;
        public CompProperties_DarktideMeleeWeapon Props
        {
            get
            {
                return (CompProperties_DarktideMeleeWeapon)props;
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
        /*
        public Pawn HoldingPawn
        {
            get
            {
                if(this.IsWeapon) // equipment check
                {
                    return this.paren;
                }
            }
        }*/

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            cleaveTargetsinGame = Props.cleaveTargets;
           
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

        }

        public override void CompTick()
        {
            base.CompTick();
            counter++;
            if(comp_CounterAttack == null)
            {
               comp_CounterAttack = parent.TryGetComp<Comp_CounterAttack>();
            }
            if(counter >= 300)
            {
                //Util_Melee.DEV_output(this.parent + " is ticking");
                //Util_Melee.DEV_output("Cleave Target : " + this.cleaveTargetsinGame);
                counter = 0;
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            // 添加一个按钮
            yield return new Command_Action
            {
                defaultLabel = "Inspect", 
                defaultDesc = "Inspect this weapon's details.", 
                icon = TexCommand.DesirePower, 
                action = ShowInspectDialog
            };
        }
        protected void ShowInspectDialog()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("Weapon Info:".Translate());
            //info.AppendLine($"Counter: {counter}".Translate());
            info.AppendLine($"Cleave Targets: {cleaveTargetsinGame}".Translate());
            if (comp_CounterAttack != null)
            {
                info.AppendLine("Enable CounterAttack".Translate());
            }

            
            Find.WindowStack.Add(new Dialog_MessageBox(info.ToString(), title: "Inspect Weapon".Translate()));
        }
        protected virtual void Dodge()
        { 
            
        }
    }

    public class CompProperties_DarktideMeleeWeapon : CompProperties
    {
        public CompProperties_DarktideMeleeWeapon()
        {
            this.compClass = typeof(Comp_DarktideMeleeWeapon);
        }

        public int cleaveTargets = 0;


    }
}
