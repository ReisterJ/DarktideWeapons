using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.Blessings
{
    public abstract class DW_Blessing : IExposable
    {

        protected String name;

        protected String description;

        protected bool forAttacker;

        protected bool forVictim;

        
        public void ExposeData()
        {

        }
    }
}
