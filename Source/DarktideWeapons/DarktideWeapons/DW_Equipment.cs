using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class DW_Equipment : ThingWithComps
    {
        protected int counter = 0;

        protected Thing holder;

        public List<DW_WeaponComp> weaponComps = new List<DW_WeaponComp>();

        public Comp_DWToughnessShield Linked_CompDWToughnessShield;

        public Comp_DWSwtichMode Comp_DWSwtichMode => this.TryGetComp<Comp_DWSwtichMode>();
        protected CompEquippable Comp_Equippable => this.TryGetComp<CompEquippable>(); 

        public Comp_DWChargeWeapon Comp_DWChargeWeapon => this.TryGetComp<Comp_DWChargeWeapon>();

        protected int maxCheck = 10;

        public bool switchverb = false;

        public Pawn HoldingPawn
        {
            get
            {
                if (holder is Pawn pawn) return pawn;
                return null;
            }
            set
            {
                holder = value;
            }
        }

        protected override void Tick()
        {
           
            base.Tick();
        }
        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if(HoldingPawn?.Drafted == false && switchverb)
            {
                ChangeVerb(setMainMode : true);
            }
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
        }
        public virtual bool QualityUpgrade(QualityCategory q)
        {
            CompQuality compQuality = this.TryGetComp<CompQuality>();
            if (compQuality == null) return false;
            compQuality.SetQuality(q, ArtGenerationContext.Colony);
            return true;
        }

   
        public override void Notify_Equipped(Pawn pawn)
        {
            this.holder = pawn;
            //caution here     
            Linked_CompDWToughnessShield = pawn.TryGetComp<Comp_DWToughnessShield>();
            base.Notify_Equipped(pawn);

            weaponComps.Clear();
            using (var enumerator = AllComps.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is DW_WeaponComp dw)
                    {
                        weaponComps.Add(dw);
                    }
                }
            }
            ChangeVerb(setMainMode: true);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            ChangeVerb(setMainMode: true);
            base.Notify_Unequipped(pawn);
            this.holder = null;
        }

        public virtual void ChangeVerb(bool setMainMode = false)
        {
            if(holder == null)
            {
                return;
            }
            switchverb = !switchverb;
            if (setMainMode)
            {
                switchverb = false;
            }
            if(Comp_DWSwtichMode != null)
            {
                Comp_DWSwtichMode.wielder = holder;
                Comp_DWSwtichMode.EquipmentChangeVerbWithHediff(switchverb);
            }
            
            using (var enumerator = weaponComps.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (switchverb)
                    {
                        enumerator.Current.SwitchMode(switchverb);
                    }
                }
            }

        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = "DWInspectWeapon".Translate(),
                defaultDesc = "DWInspectWeaponDesc".Translate(),
                icon = TexCommand.DesirePower,
                action = new Action(ShowInspectDialog)
            };
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }

        public override string GetInspectString()
        {
            string s = " ";
            if (this.def.IsRangedWeapon)
            {
                if (Comp_Equippable.VerbProperties.Count > 1)
                {
                    if (!switchverb)
                    {
                        return (s + "DWCurrentFireMode".Translate() + " : " + "DWMainMode".Translate());
                    }
                    else
                    {
                        return (s + "DWCurrentFireMode".Translate() + " : " + "DWAuxiliaryMode".Translate());
                    }
                }
            }
            return s;
        }
        protected virtual void ShowInspectDialog()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("DWWeaponInfo:".Translate());
            if (this.def.IsRangedWeapon) 
            {
                info.AppendLine("DWWeaponType".Translate() + " : " + "DWWeaponTypeRanged".Translate());
                if(Comp_Equippable.VerbProperties.Count > 1)
                {
                    if (!switchverb)
                    {
                        info.AppendLine("DWCurrentFireMode".Translate() + " : " + "DWMainMode".Translate());
                    }
                    else
                    {
                        info.AppendLine("DWCurrentFireMode".Translate() + " : " + "DWAuxiliaryMode".Translate());
                    }
                }
            }
                
            using (var enumerator = weaponComps.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    info.AppendLine(enumerator.Current.ShowInfo(holder));
                }
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
}
