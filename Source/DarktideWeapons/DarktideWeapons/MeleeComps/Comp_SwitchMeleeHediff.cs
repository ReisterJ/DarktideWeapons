using DarktideWeapons.SpecialMeleeVerbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.MeleeComps
{
    public class Comp_SwitchMeleeHediff : DW_WeaponComp, IEquipmentMelee
    {
        public CompProperties_SwitchMeleeHediff Props => (CompProperties_SwitchMeleeHediff)props;

        protected HediffDef currentHediff = null;

        private bool HediffListCheck()
        {
            if (Props.hediffsToApply == null)
            {
                Log.Error("FATAL: Comp_SwitchMeleeHediff Props.hediffsToApply is null. Please assign a list of HediffDefs to apply.");
                return false;
            }
            if (Props.hediffsToApply.Count < 1)
            {
                Log.Error("FATAL: Comp_SwitchMeleeHediff Props.hediffsToApply is empty. Please assign at least one HediffDef to apply.");
                return false;
            }
            return true;
        }
        public virtual HediffDef HediffToApply
        {
            get
            {
                if (!HediffListCheck())
                {
                    return null;
                }

                if (currentHediff == null)
                {
                    currentHediff = Props.hediffsToApply.FirstOrDefault(h => h != null);
                }

                return currentHediff;
            }
        }

        public bool TrySwitchHediff(HediffDef newHediff)
        {
            if (!HediffListCheck())
            {
                return false;
            }
            if (Props.hediffsToApply.Contains(newHediff))
            {
                currentHediff = newHediff;
                return true;
            }
            else
            {
                Log.Warning("Comp_SwitchMeleeHediff: Attempted to switch to a HediffDef that is not in the hediffsToApply list. Switch failed.");
            }
            return false;
        }

        public bool TrySwitchHediff()
        {
        
            if (!HediffListCheck())
            {
                return false;
            }
            var list = Props.hediffsToApply;
            if (list == null || list.Count < 2)
            {
                Log.Warning("Comp_SwitchMeleeHediff: Not enough HediffDefs in the hediffsToApply list to switch. At least 2 are required.");
                return false;
            }

            int count = list.Count;
            int currentIndex = -1;
            for (int i = 0; i < count; i++)
            {
                if (list[i] != null && list[i] == currentHediff)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                var first = list.FirstOrDefault(h => h != null);
                if (first != null)
                {
                    currentHediff = first;
                    return true;
                }
                return false;
            }

            int nextIndex = (currentIndex + 1) % count;
            int checkedCount = 0;
            while (checkedCount < count && list[nextIndex] == null)
            {
                nextIndex = (nextIndex + 1) % count;
                checkedCount++;
            }

            if (checkedCount >= count || list[nextIndex] == null)
            {
                return false;
            }

            currentHediff = list[nextIndex];
            return true;
        }
        
        public EquipmentMeleeWorkerData EquipmentMeleeWorker(Thing target, DW_Equipment de)
        {
            Pawn targetPawn = target as Pawn;
            EquipmentMeleeWorkerData data = new EquipmentMeleeWorkerData();
            data.Init();
            if (targetPawn != null && HediffToApply != null)
            {
                data.applyHediffs.Add(HediffToApply);
            }
            return data;
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (PawnOwner == null || !PawnOwner.IsColonistPlayerControlled)
            {
                yield break;
            }

            Command_Action command = new Command_Action
            {
                defaultLabel = (HediffToApply?.label ?? "None"),
                defaultDesc = "DW_Comp_SwitchMeleeHediff_HediffGizmoDesc".Translate() + " : " + (HediffToApply?.label ?? "None".Translate()),
                icon = parent.def.uiIcon,
            };

            yield return command;
        }
        public override string ShowInfo(Thing wielder)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.ShowInfo(wielder));

            if (HediffListCheck() && Props.hediffsToApply != null && Props.hediffsToApply.Count > 0)
            {
                stringBuilder.AppendLine("DW_Comp_SwitchMeleeHediff_AvailableHediffs".Translate() + ":");
                foreach (var hediff in Props.hediffsToApply)
                {
                    if (hediff != null)
                    {
                        stringBuilder.AppendLine("  - " + hediff.label);
                    }
                }
                stringBuilder.AppendLine("DW_Comp_SwitchMeleeHediff_CurrentHediff".Translate() + ": " + (HediffToApply?.label ?? "None".Translate()));
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }

    public class CompProperties_SwitchMeleeHediff : CompProperties
    {
        public CompProperties_SwitchMeleeHediff()
        {
            this.compClass = typeof(Comp_SwitchMeleeHediff);
        }

        public List<HediffDef> hediffsToApply;
    }
}
