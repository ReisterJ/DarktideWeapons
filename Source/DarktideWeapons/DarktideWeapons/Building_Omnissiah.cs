using DarktideWeapons.Blessings;
using DarktideWeapons.Windows;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace DarktideWeapons
{
    /// <summary>
    /// The Omnissiah's Forge building. Any pawn that equips a weapon with Comp_BlessingSocket
    /// can right-click this building to open the blessing UI and modify their weapon.
    /// </summary>
    public class Building_Omnissiah : Building
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
                yield return opt;

            if (selPawn == null || selPawn.Dead || selPawn.Downed)
                yield break;

            // Find the first equipped weapon that has a blessing socket
            Thing socketedWeapon = selPawn.equipment?.AllEquipmentListForReading
                .FirstOrDefault(e => e.TryGetComp<Comp_BlessingSocket>() != null);

            if (socketedWeapon == null)
            {
                yield return new FloatMenuOption("DW_NoSocketedWeapon".Translate(), null);
                yield break;
            }

            if (!selPawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotReach".Translate(), null);
                yield break;
            }

            yield return new FloatMenuOption(
                "DW_BlessWeapon".Translate(socketedWeapon.LabelShortCap),
                () =>
                {
                    Job job = JobMaker.MakeJob(DW_JobDefOf.DW_UseOmniBlessing, this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                },
                MenuOptionPriority.High
            );
        }
    }
}
