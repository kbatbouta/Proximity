using System;
using HarmonyLib;

namespace Proximity
{
    public static class Finder
    {
        public static string packageID = "krkr.proximity";

        public static Harmony harmony = new Harmony(packageID);

        public static bool debug = true;
    }
}
