using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_EquipmentIntegration : DefModExtension
    {

    }

    public struct IntegrationData
    {
        public List<StatDef> statList;

        public bool RequireResearch;

        public bool RequireWeaponTrait;


    }

    public class HediffComp_EquipmentIntegration : HediffComp
    {

    }
}
