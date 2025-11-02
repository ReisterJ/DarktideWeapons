using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarktideWeapons
{
    public class DW_Projectile_Tracking : DW_Projectile
    {
        private const int MaxTicks = 300;
        private int currentTick = 0;
        
        private bool targetAcquired = false;
        private GameObject currentTarget;

        protected override void Tick()
        {
            base.Tick();
            currentTick++;

            if (currentTick < MaxTicks)
            {
                if (!targetAcquired)
                {
                    
                }
                else
                {
                   
                }
            }
        }

       

        
    }
}
