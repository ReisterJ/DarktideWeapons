using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using DarktideWeapons.Hediffs;
using DarktideWeapons.ModExtensions;

namespace DarktideWeapons.Util
{
    public class Util_Hediff
    {
        public static void HediffImpact(Pawn pawn,List<HediffDef> applyHediffDefs, List<HediffDefWithLevel> applyHediffDefsWithLevel)
        {
            if(applyHediffDefs != null)
            {
                foreach (HediffDef hediffdef in applyHediffDefs)
                {
                    TryAddHediff(hediffdef, pawn);
                }
            }
            if(applyHediffDefsWithLevel != null)
            {
                foreach (HediffDefWithLevel hediffdef in applyHediffDefsWithLevel)
                {
                    TryAddHediffWithLevel(hediffdef.hediffDef, pawn);
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffdef.hediffDef);
                    if (hediff is Hediff_Level levelHediff)
                    {
                        // ֱ������ level �ֶ���ȷ����ȷ���õȼ�
                        // SetLevelTo ������ĳЩ����¿��ܲ���Ч
                        levelHediff.level = hediffdef.level + levelHediff.level; // ����Ѿ��и� Hediff�����ӵȼ������û�У�������Ϊ hediffdef.level
                        // ͬʱ���� SetLevelTo �Դ����κα�Ҫ���ڲ�����
                        levelHediff.SetLevelTo(levelHediff.level);
                        
                        if (levelHediff is Hediff_Level_Buff bufflevel)
                        {
                            bufflevel.RefreshBuff();
                        }
                        if (levelHediff is Hediff_DOT dot)
                        {
                            dot.RefreshDOT();
                        }
                    }
                }
            }
            
        }

        public static bool TryAddHediff(HediffDef hediffdef, Pawn Target)
        {
            Hediff H = Target.health.hediffSet.GetFirstHediffOfDef(hediffdef);
            if (H == null)
            {
                Target.health.AddHediff(hediffdef, Util_BodyPart.GetTorsoPart(Target), null, null);
                return true;
            }
            else
            {
                if (H is Hediff_DOT dot)
                {
                    dot.ChangeLevel(1);
                    dot.RefreshDOT();
                    return true;
                }
                if (H is Hediff_Level_Buff buff)
                {
                    buff.ChangeLevel(1);
                    buff.RefreshBuff();
                    return true;
                }
            }
            return false;
        }

        public static bool TryAddHediffWithLevel(HediffDef hediffdef, Pawn Target)
        {
            Hediff H = Target.health.hediffSet.GetFirstHediffOfDef(hediffdef);
            if (H == null)
            {
                Target.health.AddHediff(hediffdef, Util_BodyPart.GetTorsoPart(Target), null, null);
                return true;
            }
            return false;
        }
    }
}
