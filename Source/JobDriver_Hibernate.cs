using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace Hibernation
{
    internal class JobDriver_Hibernate : JobDriver
    {
        private const TargetIndex BedIndex = TargetIndex.A;
        private const int MoteIntervalTicks = 160;

        private Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

        public override bool PlayerInterruptable => !base.OnLastToil;

        public override string GetReport()
        {
            return ReportStringProcessed(Helper.HibernationJobReport(pawn));
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (Bed != null)
            {
                return pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
            }
            return true;
        }
        public override bool CanBeginNowWhileLyingDown()
        {
            return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool hasBed = Bed != null;
            if (hasBed)
            {
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
                yield return Toils_Bed.GotoBed(TargetIndex.A);
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }
            Toil toil2 = Toils_LayDown.LayDown(TargetIndex.A, hasBed, lookForOtherJobs: false, canSleep: true, gainRestAndHealth: true, PawnPosture.LayingOnGroundNormal);
            toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
            {
                if (pawn.Drafted)
                {
                    pawn.drafter.Drafted = false;
                }
                if (!pawn.health.hediffSet.HasHediff(DefOfs.Hibernation))
                {
                    pawn.health.AddHediff(DefOfs.Hibernation);
                }
            });
            toil2.tickIntervalAction = (Action<int>)Delegate.Combine(toil2.tickIntervalAction, (Action<int>)delegate (int delta)
            {
                if (pawn.IsHashIntervalTick(160, delta))
                {
                    MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_Deathresting, new Vector3(0f, pawn.story.bodyType.bedOffset).RotatedBy(pawn.Rotation));
                }
            });
            yield return toil2;
        }
    }

}
