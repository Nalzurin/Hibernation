using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace Hibernation
{
    public class FloatMenuOptionProvider_CarryToHibernate : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return ModsConfig.BiotechActive;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!clickedPawn.Hibernating() || clickedPawn.InBed())
            {
                return null;
            }
            if (!clickedPawn.IsColonist && !clickedPawn.IsPrisonerOfColony)
            {
                return null;
            }
            if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return new FloatMenuOption("CannotCarry".Translate(clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }
            var bestBedOrCasket = RestUtility.FindBedFor(clickedPawn, context.FirstSelectedPawn, checkSocialProperness: false);
            if (bestBedOrCasket == null)
            {
                return new FloatMenuOption("CannotCarry".Translate(clickedPawn) + ": " + "NoBed".Translate(), null);
            }
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToSpecificThing".Translate(bestBedOrCasket), delegate
            {
                Job job = JobMaker.MakeJob(JobDefOf.DeliverToBed, clickedPawn, bestBedOrCasket);
                job.count = 1;
                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }, MenuOptionPriority.RescueOrCapture, null, clickedPawn), context.FirstSelectedPawn, clickedPawn);
        }
    }
}
