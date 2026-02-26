using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public interface IEquipmentMelee
    {
        EquipmentMeleeWorkerData EquipmentMeleeWorker(Thing target, DW_Equipment de);
    }

    public struct EquipmentMeleeWorkerData
    {
        public float damageMulti;
        public float armorPenetrationMulti;
        public List<HediffDef> applyHediffs;
        public List<HediffDefWithLevel> applyHediffsWithLevel;
        public float critChanceMulti;
        public float staggerLevelMutli;

        public void Init()
        {
            damageMulti = 1f;
            armorPenetrationMulti = 1f;
            applyHediffs = new List<HediffDef>();
            applyHediffsWithLevel = new List<HediffDefWithLevel>();
            critChanceMulti = 1f;
            staggerLevelMutli = 1f;
        }

        public void Add(EquipmentMeleeWorkerData data2)
        {
            damageMulti *= data2.damageMulti;
            armorPenetrationMulti *= data2.armorPenetrationMulti;
            critChanceMulti *= data2.critChanceMulti;
            staggerLevelMutli *= data2.staggerLevelMutli;
            if (data2.applyHediffs != null)
            {
                applyHediffs.AddRange(data2.applyHediffs);
            }
            if (data2.applyHediffsWithLevel != null)
            {
                applyHediffsWithLevel.AddRange(data2.applyHediffsWithLevel);
            }
        }
    }
}
