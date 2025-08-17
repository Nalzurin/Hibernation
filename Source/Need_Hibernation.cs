using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Hibernation
{
    public class Need_Hibernation : Need
    {
        public int lastHibernationTick = -999;

        [Unsaved(false)]
        private Gene_Hibernation cachedHibernationGene;

        public const float LevelForAlert = 0.1f;

        public const float FallPerDay = 1f / 30f;

        public const float GainPerDayHibernating = 0.2f;

        private const float Interval = 400f;

        public bool Hibernating => Find.TickManager.TicksGame <= lastHibernationTick + pawn.UpdateRateTicks;

        private Gene_Hibernation HibernationGene
        {
            get
            {
                if (cachedHibernationGene == null)
                {
                    cachedHibernationGene = pawn.genes?.GetFirstGeneOfType<Gene_Hibernation>();
                }
                return cachedHibernationGene;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (IsFrozen)
                {
                    return 0;
                }
                if (!Hibernating)
                {
                    return -1;
                }
                return 1;
            }
        }

        public Need_Hibernation(Pawn pawn)
            : base(pawn)
        {
            threshPercents = new List<float> { 0.1f };
        }

        public override void SetInitialLevel()
        {
            CurLevel = 1f;
        }

        public override void NeedInterval()
        {
            if (!IsFrozen)
            {
                CurLevel += (Hibernating ? 0.2f : (-1f / 30f)) / 400f;
                CheckForStateChange();
            }
        }

        private void CheckForStateChange()
        {
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(DefOfs.HibernationExhaustion);
            if (firstHediffOfDef != null && CurLevel > 0f)
            {
                firstHediffOfDef.Severity = 0f;
            }
            else if (CurLevel == 0f)
            {
                pawn.health.AddHediff(DefOfs.HibernationExhaustion);
            }
        }

        public override string GetTipString()
        {
            string text = (base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n";
            if (!Hibernating)
            {
                if (base.CurLevelPercentage > 0.1f)
                {
                    float num = (base.CurLevelPercentage - 0.1f) / (1f / 30f);
                    text += "NextHibernatingNeed".Translate(pawn.Named("PAWN"), "PeriodDays".Translate(num.ToString("F1")).Named("DURATION")).Resolve().CapitalizeFirst();
                }
                else
                {
                    text += "PawnShouldHibernateNow".Translate(pawn.Named("PAWN")).CapitalizeFirst().Colorize(ColorLibrary.RedReadable);
                }
                text += "\n\n";
            }
            return text + def.description;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastHibernationTick, "lastHibernationTick", -999);
        }
    }
}
