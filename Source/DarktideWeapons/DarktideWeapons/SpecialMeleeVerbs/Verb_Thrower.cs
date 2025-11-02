using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace DarktideWeapons.Verbs
{
    public class Verb_Thrower : Verb
    {

        public float radius;

        public int lastingTicks;


        protected override bool TryCastShot()
        {
            throw new NotImplementedException();    
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            verbProps.DrawRadiusRing(caster.Position, this);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                DrawHighlightFieldRadiusAroundTarget(target);
            }
        }

        protected List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            List<IntVec3> tmpCells = new List<IntVec3>();
            List<Pair<IntVec3, float>> tmpCellDots = new List<Pair<IntVec3, float>>();
            tmpCellDots.Clear();
            tmpCells.Clear();
            tmpCellDots.Add(new Pair<IntVec3, float>(target.Cell, 999f));
            
            Map map = Caster.Map;
            //int num2 = Mathf.Min(tmpCellDots.Count, Props.numCellsToHit);
            GenRadial.NumCellsInRadius(radius);
            return tmpCells;
        }
    }
}
