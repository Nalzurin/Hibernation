using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Hibernation
{
    public static class Helper
    {
        public static bool Hibernating(this Pawn pawn)
        {
            if (ModsConfig.BiotechActive)
            {
                return pawn.health.hediffSet.HasHediff(DefOfs.Hibernation);
            }
            return false;
        }
        public static bool CanHibernate(this Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
            {
                return false;
            }
            return pawn.genes.GetFirstGeneOfType<Gene_Hibernation>() != null;
        }
        public static string HibernationJobReport(Pawn pawn)
        {

            Gene_Hibernation firstGeneOfType = pawn.genes.GetFirstGeneOfType<Gene_Hibernation>();
            TaggedString taggedString = "Hibernating".Translate().CapitalizeFirst() + ": ";
            float hibernatingPercent = firstGeneOfType.HibernationPercent;
            if (hibernatingPercent < 1f)
            {
                taggedString += Mathf.Min(hibernatingPercent, 0.99f).ToStringPercent("F0");
            }
            else
            {
                taggedString += string.Format("{0} - {1}", "Complete".Translate().CapitalizeFirst(), "HibernationCanWakeSafely".Translate());
            }

            if (hibernatingPercent < 1f)
            {
                taggedString += ", " + "DurationLeft".Translate((firstGeneOfType.MinHibernationTicks - firstGeneOfType.hibernationTicks).ToStringTicksToPeriod());
            }

            return taggedString.Resolve();
        }
    }
}
