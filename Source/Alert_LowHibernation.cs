using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Hibernation
{
    internal class Alert_LowHibernation : Alert
    {
        private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

        private List<string> targetLabels = new List<string>();

        public Alert_LowHibernation()
        {
            requireBiotech = true;
        }

        public override string GetLabel()
        {
            if (targets.Count == 1)
            {
                return "AlertLowHibernationPawn".Translate(targetLabels[0].Named("PAWN"));
            }
            return "AlertLowHibernationPawns".Translate(targetLabels.Count.ToStringCached().Named("NUMCULPRITS"));
        }

        private void CalculateTargets()
        {
            targets.Clear();
            targetLabels.Clear();
            foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned)
            {
                if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer && item.needs != null && item.needs.TryGetNeed(out Need_Hibernation need) && need.CurLevel <= 0.1f && !item.Hibernating())
                {
                    targets.Add(item);
                    targetLabels.Add(item.NameShortColored.Resolve());
                }
            }
        }

        public override TaggedString GetExplanation()
        {
            return "AlertLowHibernationDesc".Translate(targetLabels.ToLineList("  - ").Named("CULPRITS"));
        }

        public override AlertReport GetReport()
        {
            CalculateTargets();
            return AlertReport.CulpritsAre(targets);
        }
    }
}
