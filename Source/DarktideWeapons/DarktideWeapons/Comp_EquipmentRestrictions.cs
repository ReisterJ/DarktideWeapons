using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace DarktideWeapons
{
    /// <summary>
    /// 技能要求结构体
    /// </summary>
    public struct SkillRequirement
    {
        public SkillDef skill;
        public int minLevel;

        public SkillRequirement(SkillDef skill, int minLevel)
        {
            this.skill = skill;
            this.minLevel = minLevel;
        }

        public bool PawnMeetsRequirement(Pawn pawn)
        {
            if (pawn?.skills == null) return false;
            SkillRecord skillRecord = pawn.skills.GetSkill(skill);
            if (skillRecord == null) return false;
            return skillRecord.Level >= minLevel;
        }

        public string GetFailReason(Pawn pawn)
        {
            if (pawn?.skills == null) return $"{"DW_NoSkills".Translate()}";
            SkillRecord skillRecord = pawn.skills.GetSkill(skill);
            int currentLevel = skillRecord?.Level ?? 0;
            return "DW_SkillRequirementNotMet".Translate(skill.LabelCap, minLevel, currentLevel);
        }
    }


    public class CompProperties_EquipmentRestrictions : CompProperties
    {

        public List<SkillRequirement> skillRequirements = new List<SkillRequirement>();

        public List<GeneDef> geneRequirements = new List<GeneDef>();

        public List<ThingDef> equipmentRequirements = new List<ThingDef>();

        public List<ThingDef> raceRequirements = new List<ThingDef>();

        public CompProperties_EquipmentRestrictions()
        {
            this.compClass = typeof(Comp_EquipmentRestrictions);
        }
    }

    public class Comp_EquipmentRestrictions : ThingComp
    {
        public CompProperties_EquipmentRestrictions Props => (CompProperties_EquipmentRestrictions)props;

        public bool CanEquip(Pawn pawn, out string reason)
        {
            reason = null;

            // 检查技能要求
            if (!CheckSkillRequirements(pawn, out reason))
            {
                return false;
            }

            // 检查基因要求
            if (!CheckGeneRequirements(pawn, out reason))
            {
                return false;
            }

            // 检查装备要求
            if (!CheckEquipmentRequirements(pawn, out reason))
            {
                return false;
            }

            // 检查种族要求
            if (!CheckRaceRequirements(pawn, out reason))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查技能要求
        /// </summary>
        private bool CheckSkillRequirements(Pawn pawn, out string reason)
        {
            reason = null;
            if (Props.skillRequirements == null || Props.skillRequirements.Count == 0)
            {
                return true;
            }

            List<string> failedSkills = new List<string>();
            foreach (SkillRequirement skillReq in Props.skillRequirements)
            {
                if (!skillReq.PawnMeetsRequirement(pawn))
                {
                    failedSkills.Add(skillReq.GetFailReason(pawn));
                }
            }

            if (failedSkills.Count > 0)
            {
                reason = string.Join("\n", failedSkills);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查基因要求
        /// </summary>
        private bool CheckGeneRequirements(Pawn pawn, out string reason)
        {
            reason = null;
            if (Props.geneRequirements == null || Props.geneRequirements.Count == 0)
            {
                return true;
            }

            // 检查殖民者是否有基因系统
            if (pawn?.genes == null)
            {
                reason = "DW_NoGeneSystem".Translate();
                return false;
            }

            List<string> missingGenes = new List<string>();
            foreach (GeneDef geneDef in Props.geneRequirements)
            {
                if (!pawn.genes.HasActiveGene(geneDef))
                {
                    missingGenes.Add(geneDef.LabelCap);
                }
            }

            if (missingGenes.Count > 0)
            {
                reason = "DW_MissingGenes".Translate(string.Join(", ", missingGenes));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查装备要求
        /// </summary>
        private bool CheckEquipmentRequirements(Pawn pawn, out string reason)
        {
            reason = null;
            if (Props.equipmentRequirements == null || Props.equipmentRequirements.Count == 0)
            {
                return true;
            }

            // 检查殖民者是否有装备栏
            if (pawn?.apparel == null)
            {
                reason = "DW_NoApparelSystem".Translate();
                return false;
            }

            // 检查是否穿戴了任一要求的装备
            bool hasRequiredEquipment = false;
            foreach (ThingDef equipDef in Props.equipmentRequirements)
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def == equipDef)
                    {
                        hasRequiredEquipment = true;
                        break;
                    }
                }
                if (hasRequiredEquipment) break;
            }

            if (!hasRequiredEquipment)
            {
                string requiredEquipNames = string.Join(", ", Props.equipmentRequirements.Select(e => e.LabelCap));
                reason = "DW_MissingRequiredEquipment".Translate(requiredEquipNames);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查种族要求
        /// </summary>
        private bool CheckRaceRequirements(Pawn pawn, out string reason)
        {
            reason = null;
            if (Props.raceRequirements == null || Props.raceRequirements.Count == 0)
            {
                return true;
            }

            // 检查殖民者的种族是否在允许列表中
            ThingDef pawnRace = pawn?.def;
            if (pawnRace == null)
            {
                reason = "DW_UnknownRace".Translate();
                return false;
            }

            if (!Props.raceRequirements.Contains(pawnRace))
            {
                string allowedRaces = string.Join(", ", Props.raceRequirements.Select(r => r.LabelCap));
                reason = "DW_RaceNotAllowed".Translate(pawnRace.LabelCap, allowedRaces);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取所有限制的描述，用于物品信息显示
        /// </summary>
        public string GetRestrictionsDescription()
        {
            StringBuilder sb = new StringBuilder();

            // 技能要求描述
            if (Props.skillRequirements != null && Props.skillRequirements.Count > 0)
            {
                sb.AppendLine("DW_SkillRequirementsLabel".Translate());
                foreach (SkillRequirement skillReq in Props.skillRequirements)
                {
                    sb.AppendLine($"  - {skillReq.skill.LabelCap}: {skillReq.minLevel}");
                }
            }

            // 基因要求描述
            if (Props.geneRequirements != null && Props.geneRequirements.Count > 0)
            {
                sb.AppendLine("DW_GeneRequirementsLabel".Translate());
                foreach (GeneDef geneDef in Props.geneRequirements)
                {
                    sb.AppendLine($"  - {geneDef.LabelCap}");
                }
            }

            // 装备要求描述
            if (Props.equipmentRequirements != null && Props.equipmentRequirements.Count > 0)
            {
                sb.AppendLine("DW_EquipmentRequirementsLabel".Translate());
                foreach (ThingDef equipDef in Props.equipmentRequirements)
                {
                    sb.AppendLine($"  - {equipDef.LabelCap}");
                }
            }

            // 种族要求描述
            if (Props.raceRequirements != null && Props.raceRequirements.Count > 0)
            {
                sb.AppendLine("DW_RaceRequirementsLabel".Translate());
                foreach (ThingDef raceDef in Props.raceRequirements)
                {
                    sb.AppendLine($"  - {raceDef.LabelCap}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 在物品描述中显示限制信息
        /// </summary>
        public override string CompInspectStringExtra()
        {
            string restrictions = GetRestrictionsDescription();
            if (!string.IsNullOrEmpty(restrictions))
            {
                return restrictions.TrimEnd();
            }
            return base.CompInspectStringExtra();
        }
    }
}

namespace DarktideWeapons.HarmonyPatches
{
    /// <summary>
    /// Harmony补丁 - 拦截EquipmentUtility.CanEquip方法
    /// </summary>
    [HarmonyPatch(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) })]
    public static class Patch_EquipmentUtility_CanEquip
    {
        [HarmonyPostfix]
        public static void Postfix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        {
            // 如果已经不能装备，就不需要继续检查了
            if (!__result)
            {
                return;
            }

            // 检查物品是否有装备限制组件
            Comp_EquipmentRestrictions restrictionComp = thing.TryGetComp<Comp_EquipmentRestrictions>();
            if (restrictionComp != null)
            {
                string reason;
                if (!restrictionComp.CanEquip(pawn, out reason))
                {
                    __result = false;
                    cantReason = reason;
                }
            }
        }
    }
}
