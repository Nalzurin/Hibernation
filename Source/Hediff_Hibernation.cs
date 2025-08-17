using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Hibernation
{
    public class Hediff_Hibernation : HediffWithComps
    {
        private Gene_Hibernation cachedGene;
        private Gene_Hibernation HibernationGene => cachedGene ?? (cachedGene = pawn.genes?.GetFirstGeneOfType<Gene_Hibernation>());

        public override string LabelInBrackets
        {
            get
            {
                return HibernationGene.HibernationPercent.ToStringPercent("F0");
            }
        }
        public override bool ShouldRemove
        {
            get
            {
                if (HibernationGene == null)
                {
                    return true;
                }
                return base.ShouldRemove;
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            HibernationGene?.Notify_HibernationStarted();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (pawn.Spawned && pawn.CurJobDef == DefOfs.Hibernate)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            HibernationGene?.TickHibernating(delta);
        }
    }
}
