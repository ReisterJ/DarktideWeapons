using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarktideWeapons.ModExtensions;
using Verse;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using UnityEngine;

namespace DarktideWeapons.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class PatchMain
    {
        public static Harmony HarmonyInstance;

        static PatchMain()
        {
            HarmonyInstance = new Harmony("RJ_DarktideWeapons");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

    }


    [HarmonyPatch(typeof(VerbTracker), "PrimaryVerb", MethodType.Getter)]
    public static class Patch_VerbTracker_PrimaryVerb
    {
        //默认两个射击模式
        [HarmonyPostfix]
        public static void Postfix(VerbTracker __instance, ref Verb __result)
        {
            CompEquippable compEquippable = __instance.directOwner as CompEquippable;
            if (compEquippable == null) return ;
            if (compEquippable.parent is DW_Equipment dwEquip && __instance.AllVerbs.Count > 1)
            {
                __result = dwEquip.switchverb ? __instance.AllVerbs[1] : __instance.AllVerbs[0];
                return ;
            }
            //return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public static class Patch_Pawn_TryGetAttackVerb
    {
        //默认两个射击模式
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref Verb __result)
        {
            if(__instance?.equipment?.Primary is DW_Equipment DWequipment)
            {
                if (DWequipment.def.IsRangedWeapon)
                {
                    CompEquippable compEquippable = DWequipment.TryGetComp<CompEquippable>();
                    if (compEquippable == null) return;
                    if (compEquippable.verbTracker.AllVerbs.Count > 1)
                    {
                        __result = DWequipment.switchverb ? compEquippable.verbTracker.AllVerbs[1] : compEquippable.verbTracker.AllVerbs[0];
                        return;
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Verb), "Available")]
    public static class Patch_Verb_Available
    {
        [HarmonyPostfix]
        public static void Postfix(Verb __instance, ref bool __result)
        {

            if (__instance.EquipmentSource.TryGetComp<Comp_Block>()!= null)
            {
                Comp_Block BlockComp = __instance.CasterPawn?.equipment.Primary.TryGetComp<Comp_Block>();
                if (BlockComp.isBlocking && !BlockComp.AllowAttackWhileBlocking())
                {
                    __result = false;
                }
                return;
            }
        }
    }

    // 过滤掉不应该有神经训练器的技能对应的 ThingDef
    [HarmonyPatch(typeof(ThingDefGenerator_Neurotrainer))]
    [HarmonyPatch("ImpliedThingDefs")]
    public static class Patch_ThingDefGenerator_Neurotrainer_ImpliedThingDefs
    {
        private static bool ShouldSuppressNeurotrainer(AbilityDef abilityDef)
        {
            var ext = abilityDef.GetModExtension<ModExtension_PsyCastExtendedProperties>();
            return ext != null && !ext.ShouldHaveNeurotrainer;
        }

        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<ThingDef> __result)
        {
            string prefix = ThingDefGenerator_Neurotrainer.PsytrainerDefPrefix + "_";
            bool devMode = Prefs.DevMode;

            int totalCount = 0, keptCount = 0, suppressedCount = 0, noAbilityCount = 0, nonPsytrainerCount = 0;

            List<ThingDef> filtered = __result.Where(def =>
            {
                totalCount++;

                if (!def.defName.StartsWith(prefix))
                {
                    nonPsytrainerCount++;
                    return true;
                }

                string abilityDefName = def.defName.Substring(prefix.Length);
                AbilityDef abilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail(abilityDefName);

                if (abilityDef == null)
                {
                    noAbilityCount++;
                    if (devMode)
                        Log.Warning($"[DW Neurotrainer Filter] ThingDef '{def.defName}' 对应的 AbilityDef '{abilityDefName}' 不存在于 DefDatabase，保留该 ThingDef。");
                    return true;
                }

                var ext = abilityDef.GetModExtension<ModExtension_PsyCastExtendedProperties>();
                bool suppress = ext != null && !ext.ShouldHaveNeurotrainer;

                if (devMode)
                {
                    if (ext == null)
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' → AbilityDef '{abilityDefName}' 无 ModExtension，保留。");
                    else if (!suppress)
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' → AbilityDef '{abilityDefName}' ShouldHaveNeurotrainer=true，保留。");
                    else
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' → AbilityDef '{abilityDefName}' ShouldHaveNeurotrainer=false，已屏蔽。");
                }

                if (suppress)
                    suppressedCount++;
                else
                    keptCount++;

                return !suppress;
            }).ToList();

            if (devMode)
                Log.Message($"[DW Neurotrainer Filter] 过滤完成：总计 {totalCount} 个 ThingDef，" +
                            $"非启灵器 {nonPsytrainerCount}，" +
                            $"找不到 AbilityDef {noAbilityCount}，" +
                            $"保留启灵器 {keptCount}，" +
                            $"屏蔽启灵器 {suppressedCount}。");

            __result = filtered;
        }
    }

    // 阻止被屏蔽的技能在启灵神经升级时随机分配给 Pawn
    // 策略：在 TryGiveAbilityOfLevel 的前缀中标记"正在进行启灵升级授予"
    //       在 GainAbility 的前缀中拦截被屏蔽的技能
    //       在 TryGiveAbilityOfLevel 的后缀中检测是否授予成功，失败则补授合法技能
    [HarmonyPatch(typeof(Hediff_Psylink), "TryGiveAbilityOfLevel")]
    public static class Patch_Hediff_Psylink_TryGiveAbilityOfLevel
    {
        [ThreadStatic]
        private static bool s_isActive;
        [ThreadStatic]
        private static int s_abilityCountBefore;

        // 供 GainAbility patch 读取
        public static bool IsGrantingPsylinkAbility => s_isActive;

        [HarmonyPrefix]
        public static void Prefix(Hediff_Psylink __instance, int abilityLevel)
        {
            s_isActive = true;
            s_abilityCountBefore = __instance.pawn?.abilities?.abilities?.Count ?? 0;

            if (Prefs.DevMode)
                Log.Message($"[DW PsylinkAbility] Pawn '{__instance.pawn?.LabelShort}' 即将进行等级 {abilityLevel} 的启灵技能授予，当前技能数：{s_abilityCountBefore}。");
        }

        [HarmonyPostfix]
        public static void Postfix(Hediff_Psylink __instance, int abilityLevel)
        {
            s_isActive = false;

            bool devMode = Prefs.DevMode;
            int countAfter = __instance.pawn?.abilities?.abilities?.Count ?? 0;

            if (countAfter > s_abilityCountBefore)
            {
                if (devMode)
                    Log.Message($"[DW PsylinkAbility] 等级 {abilityLevel} 技能授予成功（技能数 {s_abilityCountBefore} → {countAfter}），无需补授。");
                return;
            }

            if (devMode)
                Log.Message($"[DW PsylinkAbility] 等级 {abilityLevel} 的技能授予被拦截（无技能数变化），开始从合法候选中补授……");

            List<AbilityDef> validAbilities = DefDatabase<AbilityDef>.AllDefs
                .Where(a => a.level == abilityLevel && __instance.pawn.abilities.GetAbility(a) == null)
                .Where(a =>
                {
                    var ext = a.GetModExtension<ModExtension_PsyCastExtendedProperties>();
                    return ext == null || ext.ShouldHaveNeurotrainer;
                })
                .ToList();

            if (devMode)
                Log.Message($"[DW PsylinkAbility] 等级 {abilityLevel} 合法候选技能（{validAbilities.Count} 个）：" +
                            (validAbilities.Count > 0 ? string.Join(", ", validAbilities.Select(a => a.defName)) : "无"));

            if (validAbilities.Count > 0)
            {
                AbilityDef chosen = validAbilities.RandomElement();
                if (devMode)
                    Log.Message($"[DW PsylinkAbility] 补授技能 '{chosen.defName}' 给 '{__instance.pawn?.LabelShort}'。");
                __instance.pawn.abilities.GainAbility(chosen);
            }
            else
            {
                if (devMode)
                    Log.Warning($"[DW PsylinkAbility] 等级 {abilityLevel} 无合法候选技能，Pawn '{__instance.pawn?.LabelShort}' 本次升级不获得技能。");
            }
        }
    }

    // 在启灵升级授予期间，拦截被屏蔽技能的 GainAbility 调用
    [HarmonyPatch(typeof(Pawn_AbilityTracker), "GainAbility")]
    public static class Patch_Pawn_AbilityTracker_GainAbility_PsylinkFilter
    {
        [HarmonyPrefix]
        public static bool Prefix(AbilityDef def)
        {
            if (!Patch_Hediff_Psylink_TryGiveAbilityOfLevel.IsGrantingPsylinkAbility)
                return true;

            var ext = def.GetModExtension<ModExtension_PsyCastExtendedProperties>();
            bool shouldBlock = ext != null && !ext.ShouldHaveNeurotrainer;

            if (shouldBlock && Prefs.DevMode)
                Log.Message($"[DW PsylinkAbility] 拦截：启灵升级尝试授予被屏蔽技能 '{def.defName}'，已阻止。");

            return !shouldBlock;
        }
    }

    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class Patch_Thing_TakeDamage
    {
        [HarmonyPostfix]
        public static void Postfix(Thing __instance, DamageInfo dinfo)
        {
            if (__instance is Pawn)
            {
                //Log.Message($"Thing {__instance.Label} is a Pawn and took damage");
                if(dinfo.Instigator == null) return;
                if (dinfo.Instigator is Pawn instigatorPawn)
                {
                    //Log.Message($"Damage instigator {instigatorPawn.Label} is a Pawn");
                    ThingWithComps equipment = instigatorPawn.equipment?.Primary;
                    if(equipment == null) return;
                    if(equipment is DW_Equipment dwEquip)
                    {
                        
                    }
                }
            }
        }
    }

}
