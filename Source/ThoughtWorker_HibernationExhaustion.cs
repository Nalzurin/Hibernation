using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Hibernation
{
    public class ThoughtWorker_HibernationExhaustion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!ModsConfig.BiotechActive)
            {
                return ThoughtState.Inactive;
            }
            if (p.needs == null || !p.needs.TryGetNeed(out Need_Hibernation need))
            {
                return ThoughtState.Inactive;
            }
            return need.CurLevel == 0f;
        }
    }
}
