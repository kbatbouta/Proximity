using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Proximity
{
    public class Patches
    {
        [HarmonyPatch(typeof(Thing), nameof(Thing.Position), MethodType.Setter)]
        internal static class Thing_Position_Set
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = instructions.ToList();
                var finished = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!finished)
                    {
                        if (codes[i].opcode == OpCodes.Ret)
                        {
                            finished = true;
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>(new[] { codes[i + 1].labels[0] }) };
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(Thing), "positionInt"));
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call,
                                AccessTools.Method("Harmony_Thing_Position:OnPositionChanged"));
                            continue;
                        }
                    }
                    yield return codes[i];
                }
            }

            // TODO: Rewrite in pure IL        
            static void OnPositionChanged(Thing thing,
               IntVec3 oldPos,
               IntVec3 newPos)
            {
                if (newPos.IsValid == false) return;
                // Limiting the number of checks by limiting the resolution
                if (oldPos.IsValid && Mathf.Abs(newPos.x - oldPos.x) < 3f) return;

                if (thing.Destroyed || !thing.Spawned) return;
                if (thing.Map == null) return;
                if (thing.positionInt == null) return;

                thing.Notify_PositionChanged(oldPos, newPos);
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
        internal static class Thing_SpawnSetup
        {
            public static void Postfix(Thing __instance) => __instance.Notify_Spawned();
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.DeSpawn))]
        internal static class Thing_Despawn
        {
            public static void Prefix(Thing __instance, DestroyMode mode) => __instance.Notify_Despawning(mode);
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        internal static class Pawn_GetGizmos
        {
            public static void Postfix(Pawn __instance)
            {
                if (!Finder.debug) return;

                List<Thing> others = __instance.Map.GetComponent<ProximityComponent>().GetThingsInRange(__instance.positionInt, 10).ToList();

                if (others == null)
                    return;

                var selPos = __instance.DrawPos.MapToUIPosition();
                var offset = new Vector2(12, 12);
                GameFont textSize = Text.Font;
                TextAnchor anchor = Text.Anchor;

                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;

                foreach (Thing thing in others)
                {
                    if (thing == null || thing == __instance)
                        continue;
                    var drawPos = thing.DrawPos.MapToUIPosition();


                    var distance = Vector2.Distance(drawPos, selPos);
                    if (distance < 24)
                        continue;

                    var midPoint = (drawPos + selPos) / 2;
                    var rect = new Rect(midPoint - offset, offset * 2);
                    var realDistance = Mathf.RoundToInt(thing.Position.DistanceTo(__instance.Position));
                    if (realDistance <= 10)
                    {
                        var color = new Color(0.1f, 0.5f, 0.1f);
                        Widgets.DrawLine(drawPos, selPos, color, 1);
                        Widgets.DrawWindowBackgroundTutor(rect);
                        Widgets.Label(rect, "" + Mathf.RoundToInt(thing.Position.DistanceTo(__instance.Position)));
                    }
                    else
                    {
                        var color = new Color(1f, 0.1f, 0.1f);
                        Widgets.DrawLine(drawPos, selPos, color, 1);
                        Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f));
                        Widgets.Label(rect, "" + Mathf.RoundToInt(thing.Position.DistanceTo(__instance.Position)));
                    }
                }

                Text.Font = textSize;
                Text.Anchor = anchor;
            }
        }
    }
}
