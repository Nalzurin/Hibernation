using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Hibernation
{
    public class Alert_HibernationComplete : Alert
    {
        private List<Pawn> targets = new List<Pawn>();

        public Alert_HibernationComplete()
        {
            defaultLabel = "AlertHibernationComplete".Translate();
            defaultExplanation = "AlertHibernationCompleteDesc".Translate();
            requireBiotech = true;
        }

        public override AlertReport GetReport()
        {
            GetTargets();
            return AlertReport.CulpritsAre(targets);
        }

        public override TaggedString GetExplanation()
        {
            return defaultExplanation + ":\n" + targets.Select((Pawn p) => p.NameShortColored.Resolve()).ToLineList("  - ");
        }

        private void GetTargets()
        {
            targets.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].genes != null && list[j].Deathresting)
                    {
                        Gene_Hibernation firstGeneOfType = list[j].genes.GetFirstGeneOfType<Gene_Hibernation>();
                        if (firstGeneOfType != null && firstGeneOfType.ShowWakeAlert)
                        {
                            targets.Add(list[j]);
                        }
                    }
                }
            }
        }
    }
}
