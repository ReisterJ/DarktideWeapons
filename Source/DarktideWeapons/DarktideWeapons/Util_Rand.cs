using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public static class Util_Rand
    {
        public static List<IntVec3> ChooseRandomCell(List<IntVec3> Cells, int chosenCellNum , bool allowSame = true)
        {
            if(chosenCellNum <= 0)
            {
                Log.Error("chosenCellNum must be greater than 0");
                return Cells;
            }
            if(chosenCellNum > Cells.Count && !allowSame)
            {
                Log.Error("chosenCellNum must be less than or equal to Cells count when allowSame is false");
                return Cells;
            }
            List<IntVec3> chosenCells = new List<IntVec3>();
            int max = Cells.Count - 1;
            int chosen = 0;
            while(chosen < chosenCellNum)
            {
                int i = Rand.Range(0, max);
                if (!allowSame)
                {
                    if (chosenCells.Contains(Cells[i]))
                    {
                        continue;
                    }
                }
                chosenCells.Add(Cells[i]);
            }
            return chosenCells;
        }
    }
}
