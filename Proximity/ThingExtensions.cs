using System;
using Verse;

namespace Proximity
{
    public static class ThingExtensions
    {
        public static void Notify_PositionChanged(this Thing thing, IntVec3 oldPosition, IntVec3 newPosition)
        {
            if (thing is Pawn pawn)
            {
                var proximityComp = pawn.Map.GetComponent<ProximityComponent>();
                proximityComp.Update(pawn, newPosition, oldPosition);
            }
        }

        public static void Notify_Spawned(this Thing thing)
        {
            if (thing is Pawn pawn)
            {
                var proximityComp = pawn.Map.GetComponent<ProximityComponent>();
                proximityComp.Insert(thing, thing.positionInt);
            }
        }

        public static void Notify_Despawning(this Thing thing, DestroyMode mode)
        {
            if (thing is Pawn pawn)
            {
                var proximityComp = pawn.Map.GetComponent<ProximityComponent>();
                proximityComp.Remove(thing);
            }
        }
    }
}
