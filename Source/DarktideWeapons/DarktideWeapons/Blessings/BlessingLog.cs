using Verse;

namespace DarktideWeapons.Blessings
{
    /// <summary>
    /// Debug logging helper for the Blessing / Perk system.
    /// All output is gated behind Prefs.DevMode so it is silent in normal play
    /// and visible whenever the player enables RimWorld's Developer Mode.
    /// Each message is prefixed with "[DW-Blessing]" for easy log filtering.
    /// </summary>
    internal static class BlessingLog
    {
        private const string Prefix = "[DW-Blessing] ";

        /// <summary>Emits a message when dev mode is active.</summary>
        public static void Dev(string msg)
        {
            if (Prefs.DevMode)
                Log.Message(Prefix + msg);
        }

        /// <summary>Always emits a warning (visible outside dev mode as well).</summary>
        public static void Warn(string msg)
        {
            Log.Warning(Prefix + msg);
        }
    }
}
