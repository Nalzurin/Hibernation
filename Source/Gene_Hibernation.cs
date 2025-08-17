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
    public class Gene_Hibernation : Gene
    {
        public int hibernationTicks;

        public bool autoWake;

        private bool notifiedWakeOK;

        [Unsaved(false)]
        private Need_Hibernation cachedHibernationNeed;

        public const int BaseHibernationTicksWithoutInterruptedHediff = 240000;

        public const float PresencePercentRequiredToApply = 0.75f;

        private static readonly CachedTexture WakeCommandTex = new CachedTexture("UI/Gizmos/Wake");

        private static readonly CachedTexture AutoWakeCommandTex = new CachedTexture("UI/Gizmos/DeathrestAutoWake");
        public Need_Hibernation HibernationNeed
        {
            get
            {
                if (cachedHibernationNeed == null)
                {
                    pawn.needs.TryGetNeed(out cachedHibernationNeed);
                }
                return cachedHibernationNeed;
            }
        }

        public int MinHibernationTicks => BaseHibernationTicksWithoutInterruptedHediff;

        public float HibernationPercent => Mathf.Clamp01((float)hibernationTicks / (float)MinHibernationTicks);

        public bool ShowWakeAlert
        {
            get
            {
                if (HibernationPercent >= 1f)
                {
                    return !autoWake;
                }
                return false;
            }
        }

        public override void PostAdd()
        {
            if (ModLister.CheckBiotech("Hibernation"))
            {
                base.PostAdd();
                Reset();
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(DefOfs.Hibernation);
            if (firstHediffOfDef != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef);
            }
            Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(DefOfs.HibernationExhaustion);
            if (firstHediffOfDef2 != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef2);
            }
            Reset();
        }

        public void TickHibernating(int delta)
        {
            if (HibernationNeed != null)
            {
                HibernationNeed.lastHibernationTick = Find.TickManager.TicksGame;
            }
            hibernationTicks += delta;
            if (HibernationPercent >= 1f && !notifiedWakeOK)
            {
                notifiedWakeOK = true;
                if (autoWake)
                {
                    Wake();
                    return;
                }
                if (PawnUtility.ShouldSendNotificationAbout(pawn))
                {
                    Messages.Message("MessageHibernatingPawnCanWakeSafely".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }
        public void Notify_HibernationStarted()
        {
            notifiedWakeOK = false;
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            cachedHibernationNeed = null;
        }

        public void Wake()
        {
            if (HibernationPercent < 1f)
            {
                pawn.health.AddHediff(DefOfs.InterruptedHibernation);
            }
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(DefOfs.Hibernation);
            if (firstHediffOfDef != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef);
            }
            hibernationTicks = 0;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Active)
            {
                yield break;
            }
            if (pawn.Hibernating())
            {
                if (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony)
                {
                    string text = "HibernationWakeDesc".Translate(pawn.Named("PAWN"), hibernationTicks.ToStringTicksToPeriod().Named("DURATION")).Resolve() + "\n\n";
                    text = ((!(HibernationPercent < 1f)) ? (text + "HibernationWakeExtraDesc_Safe".Translate(pawn.Named("PAWN")).Resolve()) : (text + "HibernationWakeExtraDesc_Exhaustion".Translate(pawn.Named("PAWN"), MinHibernationTicks.ToStringTicksToPeriod().Named("TOTAL")).Resolve()));
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Wake".Translate().CapitalizeFirst(),
                        defaultDesc = text,
                        icon = WakeCommandTex.Texture,
                        action = delegate
                        {
                            if (HibernationPercent < 1f)
                            {
                                Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("WarningWakingInterruptsHibernation".Translate(pawn.Named("PAWN"), MinHibernationTicks.ToStringTicksToPeriod().Named("MINDURATION"), hibernationTicks.ToStringTicksToPeriod().Named("CURDURATION")), Wake, destructive: true);
                                Find.WindowStack.Add(window);
                            }
                            else
                            {
                                Wake();
                            }
                        }
                    };
                    yield return command_Action;
                    if (HibernationPercent < 1f)
                    {
                        yield return new Command_Toggle
                        {
                            defaultLabel = "AutoWake".Translate().CapitalizeFirst(),
                            defaultDesc = "HibernationAutoWakeDesc".Translate(pawn.Named("PAWN")).Resolve(),
                            icon = AutoWakeCommandTex.Texture,
                            isActive = () => autoWake,
                            toggleAction = delegate
                            {
                                autoWake = !autoWake;
                            }
                        };
                    }
                }
                if (DebugSettings.ShowDevGizmos)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Wake up",
                        action = delegate
                        {
                            HibernationNeed.SetInitialLevel();
                            hibernationTicks = MinHibernationTicks + 100000;
                            Wake();
                        }
                    };
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hibernationTicks, "hibernationTicks", 0);
            Scribe_Values.Look(ref autoWake, "autoWake", defaultValue: false);
            Scribe_Values.Look(ref notifiedWakeOK, "notifiedWakeOK", defaultValue: false);
        }
    }
}
