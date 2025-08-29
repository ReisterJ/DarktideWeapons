using RimWorld;
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

        public virtual bool QualityUpgrade(QualityCategory q)
        {
            CompQuality compQuality = this.TryGetComp<CompQuality>();
            if (compQuality == null) return false;
            compQuality.SetQuality(q, ArtGenerationContext.Colony);
            return true;
        }

   
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            this.holder = pawn;
            //caution here     
            Linked_CompDWToughnessShield = pawn.TryGetComp<Comp_DWToughnessShield>();
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
        }

        public virtual void ChangeVerb()
        {
            if(holder == null)
            {
                return;
            }
            switchverb = !switchverb;
            CompEquippable compEquippable = this.GetComp<CompEquippable>();
            
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
        protected virtual void ShowInspectDialog()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("Weapon Info:".Translate());
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
