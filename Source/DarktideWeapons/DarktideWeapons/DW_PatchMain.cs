using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarktideWeapons.MeleeComps;
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
        //Ĭ���������ģʽ
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
        //Ĭ���������ģʽ
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

    // ���˵���Ӧ������ѵ�����ļ��ܶ�Ӧ�� ThingDef
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
                        Log.Warning($"[DW Neurotrainer Filter] ThingDef '{def.defName}' ��Ӧ�� AbilityDef '{abilityDefName}' �������� DefDatabase�������� ThingDef��");
                    return true;
                }

                var ext = abilityDef.GetModExtension<ModExtension_PsyCastExtendedProperties>();
                bool suppress = ext != null && !ext.ShouldHaveNeurotrainer;

                if (devMode)
                {
                    if (ext == null)
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' �� AbilityDef '{abilityDefName}' �� ModExtension��������");
                    else if (!suppress)
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' �� AbilityDef '{abilityDefName}' ShouldHaveNeurotrainer=true��������");
                    else
                        Log.Message($"[DW Neurotrainer Filter] '{def.defName}' �� AbilityDef '{abilityDefName}' ShouldHaveNeurotrainer=false�������Ρ�");
                }

                if (suppress)
                    suppressedCount++;
                else
                    keptCount++;

                return !suppress;
            }).ToList();

            if (devMode)
                Log.Message($"[DW Neurotrainer Filter] ������ɣ��ܼ� {totalCount} �� ThingDef��" +
                            $"�������� {nonPsytrainerCount}��" +
                            $"�Ҳ��� AbilityDef {noAbilityCount}��" +
                            $"���������� {keptCount}��" +
                            $"���������� {suppressedCount}��");

            __result = filtered;
        }
    }

    // ��ֹ�����εļ���������������ʱ�������� Pawn
    // ���ԣ��� TryGiveAbilityOfLevel ��ǰ׺�б��"���ڽ���������������"
    //       �� GainAbility ��ǰ׺�����ر����εļ���
    //       �� TryGiveAbilityOfLevel �ĺ�׺�м���Ƿ�����ɹ���ʧ�����ںϷ�����
    [HarmonyPatch(typeof(Hediff_Psylink), "TryGiveAbilityOfLevel")]
    public static class Patch_Hediff_Psylink_TryGiveAbilityOfLevel
    {
        [ThreadStatic]
        private static bool s_isActive;
        [ThreadStatic]
        private static int s_abilityCountBefore;

        // �� GainAbility patch ��ȡ
        public static bool IsGrantingPsylinkAbility => s_isActive;

        [HarmonyPrefix]
        public static void Prefix(Hediff_Psylink __instance, int abilityLevel)
        {
            s_isActive = true;
            s_abilityCountBefore = __instance.pawn?.abilities?.abilities?.Count ?? 0;

            if (Prefs.DevMode)
                Log.Message($"[DW PsylinkAbility] Pawn '{__instance.pawn?.LabelShort}' �������еȼ� {abilityLevel} �����鼼�����裬��ǰ��������{s_abilityCountBefore}��");
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
                    Log.Message($"[DW PsylinkAbility] �ȼ� {abilityLevel} ��������ɹ��������� {s_abilityCountBefore} �� {countAfter}�������貹�ڡ�");
                return;
            }

            if (devMode)
                Log.Message($"[DW PsylinkAbility] �ȼ� {abilityLevel} �ļ������豻���أ��޼������仯������ʼ�ӺϷ���ѡ�в��ڡ���");

            List<AbilityDef> validAbilities = DefDatabase<AbilityDef>.AllDefs
                .Where(a => a.level == abilityLevel && __instance.pawn.abilities.GetAbility(a) == null)
                .Where(a =>
                {
                    var ext = a.GetModExtension<ModExtension_PsyCastExtendedProperties>();
                    return ext == null || ext.ShouldHaveNeurotrainer;
                })
                .ToList();

            if (devMode)
                Log.Message($"[DW PsylinkAbility] �ȼ� {abilityLevel} �Ϸ���ѡ���ܣ�{validAbilities.Count} ������" +
                            (validAbilities.Count > 0 ? string.Join(", ", validAbilities.Select(a => a.defName)) : "��"));

            if (validAbilities.Count > 0)
            {
                AbilityDef chosen = validAbilities.RandomElement();
                if (devMode)
                    Log.Message($"[DW PsylinkAbility] ���ڼ��� '{chosen.defName}' �� '{__instance.pawn?.LabelShort}'��");
                __instance.pawn.abilities.GainAbility(chosen);
            }
            else
            {
                if (devMode)
                    Log.Warning($"[DW PsylinkAbility] �ȼ� {abilityLevel} �޺Ϸ���ѡ���ܣ�Pawn '{__instance.pawn?.LabelShort}' ������������ü��ܡ�");
            }
        }
    }

    // ���������������ڼ䣬���ر����μ��ܵ� GainAbility ����
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
                Log.Message($"[DW PsylinkAbility] ���أ����������������豻���μ��� '{def.defName}'������ֹ��");

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

    /// <summary>
    /// 武器前摇旋转动画 —— 临时修改 equippedAngleOffset 来让原版绘制系统自动应用旋转。
    /// </summary>
    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
    public static class Patch_DrawEquipmentAiming_WarmupRotation
    {
        [ThreadStatic]
        private static float s_savedAngleOffset;
        [ThreadStatic]
        private static ThingDef s_savedDef;

        [HarmonyPrefix]
        public static void Prefix(Thing eq)
        {
            if (eq is DW_Equipment dwEquip)
            {
                var animComp = dwEquip.TryGetComp<Comp_WeaponWarmupRotation>();
                if (animComp != null)
                {
                    float extraRotation = animComp.GetExtraRotation();
                    if (extraRotation != 0f)
                    {
                        s_savedDef = eq.def;
                        s_savedAngleOffset = eq.def.equippedAngleOffset;
                        eq.def.equippedAngleOffset += extraRotation;
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Thing eq)
        {
            if (s_savedDef == eq.def)
            {
                eq.def.equippedAngleOffset = s_savedAngleOffset;
                s_savedDef = null;
            }
        }
    }

}
