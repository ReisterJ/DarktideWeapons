using DarktideWeapons.Blessings;
using DarktideWeapons.Windows;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace DarktideWeapons.JobDrivers
{
    /// <summary>
    /// Job driver for interacting with Building_Omnissiah.
    /// Walks the pawn to the building's interaction cell, then opens Window_Omnissiah.
    /// </summary>
    public class JobDriver_OmniBlessing : JobDriver
    {
        private Building_Omnissiah TargetBuilding => (Building_Omnissiah)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetBuilding, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);

            // Walk to the building
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            // Open the blessing window
            Toil openWindow = new Toil();
            openWindow.defaultCompleteMode = ToilCompleteMode.Instant;
            openWindow.initAction = () =>
            {
                Thing socketedWeapon = pawn.equipment?.AllEquipmentListForReading
                    .FirstOrDefault(e => e.TryGetComp<Comp_BlessingSocket>() != null);

                if (socketedWeapon == null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }

                Comp_BlessingSocket socket = socketedWeapon.TryGetComp<Comp_BlessingSocket>();
                Find.WindowStack.Add(new Window_Omnissiah(pawn, socketedWeapon, socket));
            };
            yield return openWindow;
        }
    }
}
