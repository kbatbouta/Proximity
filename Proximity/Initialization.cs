using System;
using Verse;

namespace Proximity
{
    [StaticConstructorOnStartup]
    public static class Initialization
    {
        static Initialization()
        {
            Finder.harmony.PatchAll();
        }
    }
}
