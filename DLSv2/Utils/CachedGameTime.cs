using System;
using Rage;

namespace DLSv2.Utils
{
    internal static class CachedGameTime
    {
        private static ulong lastTick = Game.TickCount;
        private static uint lastGameTime = Game.GameTime;

        public static uint GameTime
        {
            get
            {
                ulong tick = Game.TickCount;
                if (tick > lastTick)
                {
                    lastTick = tick;
                    lastGameTime = Game.GameTime;
                }

                return lastGameTime;
            }
        }
    }
}
