using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_CannotCastWhileBlocking : CompAbilityEffect
    {
        public Comp_Block BlockComp
        {
            get
            {
                return parent.pawn.equipment.Primary.TryGetComp<Comp_Block>();
            }
        }
        public override bool CanCast
        {
            get
            {
                if (BlockComp.isBlocking && !BlockComp.AllowAttackWhileBlocking())
                {
                    return false;
                }
                return base.CanCast;
            }
        }
        public override bool GizmoDisabled(out string reason)
        {
            if (BlockComp.isBlocking && !BlockComp.AllowAttackWhileBlocking())
            {
                reason = "CantCastWhileBlocking".Translate();
                return true;
            }
          
            return base.GizmoDisabled(out reason);
        }

    }

    public class CompProperties_CannotCastWhileBlocking : CompProperties_AbilityEffect
    {
        public CompProperties_CannotCastWhileBlocking()
        {
            this.compClass = typeof(CompAbilityEffect_CannotCastWhileBlocking);
        }
    }

    public class CompAbilityEffect_CastWhileBlocking : CompAbilityEffect
    {
        public Comp_Block BlockComp
        {
            get
            {
                return parent.pawn.equipment.Primary.TryGetComp<Comp_Block>();
            }
        }
        public override bool CanCast
        {
            get
            {
                if (BlockComp.isBlocking )
                {
                    return base.CanCast;
                }
                return false;
            }
        }
        public override bool GizmoDisabled(out string reason)
        {
            if (!BlockComp.isBlocking)
            {
                reason = "MustCastWhileBlocking".Translate();
                return true;
            }

            return base.GizmoDisabled(out reason);
        }

    }

    public class CompProperties_CastWhileBlocking : CompProperties_AbilityEffect
    {
        public CompProperties_CastWhileBlocking()
        {
            this.compClass = typeof(CompAbilityEffect_CastWhileBlocking);
        }
    }
}
