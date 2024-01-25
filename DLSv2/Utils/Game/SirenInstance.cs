using System;
using System.Runtime.InteropServices;
using Rage;
using Rage.Attributes;

namespace DLSv2.Utils
{
    public unsafe struct SirenInstance
    {
        public static int CVehicle_SirensDataOffset;
        public static void MemoryInit()
        {
            var address = Game.FindPattern("48 89 B7 ?? ?? ?? ?? 48 8B 0B");
            if (Memory.AssertAddress(address, nameof(CVehicle_SirensDataOffset)))
            {
                CVehicle_SirensDataOffset = Marshal.ReadInt16(address + 3);
                $"  CVehicle_SirensDataOffset = {CVehicle_SirensDataOffset}".ToLog(LogLevel.DEBUG);
            }
        }
        
        public SirenInstance(Vehicle vehicle)
        {
            this.Vehicle = vehicle;
        }

        public Vehicle Vehicle { get; }
        private sirenInstanceData* data => *(sirenInstanceData**)(Vehicle.MemoryAddress + CVehicle_SirensDataOffset);

        public uint SirenOnTime => data->sirenOnTime;
        public float SirenTimeDelta => data->sirenTimeDelta;
        public int TotalSirenBeats => data->lastSirenBeat;
        public int CurrentSirenBeat => data->lastSirenBeat % 32;
        public int GameTimeOffset => TotalSirenBeats < 0 ? 0 : (int)(Math.Round(SirenOnTime + SirenTimeDelta, 0) - CachedGameTime.GameTime);

        public void SetSirenOnTime(uint gameTime, uint threshold = 10, bool createFiberIfJustToggled = true)
        {
            if (TotalSirenBeats >= 0)
            {
                int offset = GameTimeOffset;
                uint onTime = (uint)(gameTime + offset);
                uint currentDiff = (uint)Math.Abs(SirenOnTime - onTime);

                // Siren processing breaks if on time is in the future
                // To prevent constantly resetting slightly (due to rounding error in time delta), 
                // only change if difference from current siren on time exceeds threshold
                if (onTime > CachedGameTime.GameTime || currentDiff < threshold) return;
                
                data->sirenOnTime = onTime;
                $"Reset siren on time for 0x{Vehicle.Handle.Value.ToString("X")} to {gameTime} + {offset} = {onTime}".ToLog(LogLevel.DEBUG);
            } else if (Vehicle.IsSirenOn)
            {
                $"Siren is on but beats is <0. Siren may have just been toggled. Yielding one tick and trying again.".ToLog(LogLevel.DEBUG);
                var s = this;
                GameFiber.StartNew(() => { 
                    GameFiber.Yield();
                    if (s.Vehicle) s.SetSirenOnTime(gameTime, threshold, false);
                });
            }
        }

        public void SetSirenOnTime()
        {
            uint newOnTime = (uint)(CachedGameTime.GameTime - (32 * SirenTimeDelta / TotalSirenBeats));
            SetSirenOnTime(newOnTime);
        }
        
        [StructLayout(LayoutKind.Explicit)]
        internal struct sirenInstanceData
        {
            [FieldOffset(0x000)] public uint sirenOnTime;
            [FieldOffset(0x004)] public float sirenTimeDelta;
            [FieldOffset(0x008)] public int lastSirenBeat;
        };

#if DEBUG
        [ConsoleCommand]
        private static void DebugSirenBeats()
        {
            Vehicle v = Game.LocalPlayer.Character.LastVehicle;
            SirenInstance s = new SirenInstance(v);

            while (v && Game.LocalPlayer.Character.LastVehicle == v)
            {
                string info = "";
                info += $"On Time: {s.SirenOnTime}\n";
                info += $"Time Delta: {s.SirenTimeDelta}\n";
                info += $"Total Beats: {s.TotalSirenBeats}\n";
                info += $"Last Beat: {s.CurrentSirenBeat}\n";
                info += $"Time Offset: {s.GameTimeOffset}\n";
                info += $"Game Time: {CachedGameTime.GameTime}\n";
                Game.DisplaySubtitle(info, 10);
                GameFiber.Yield();
            }
        }

        [ConsoleCommand(Name = "ResetSirenOnTime")]
        private static void Command_ResetSirenOnTime()
        {
            while (Game.Console.IsOpen) GameFiber.Yield();

            Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
            if (!v) return;

            var s = new SirenInstance(v);
            s.SetSirenOnTime();
            Game.DisplayNotification($"Reset siren on time to {s.SirenOnTime}");
        }

        [ConsoleCommand(Name = "SetSirenOnTime")]
        private static void Command_SetSirenOnTime(uint time)
        {
            while (Game.Console.IsOpen) GameFiber.Yield();

            Vehicle v = Game.LocalPlayer.Character.CurrentVehicle;
            if (!v) return;

            var s = new SirenInstance(v);
            s.SetSirenOnTime(time);
            Game.DisplayNotification($"Reset siren on time to {time}");
        }
#endif
    }
}
