using System;
using System.Runtime.InteropServices;
using Rage;
using Rage.Attributes;

namespace DLSv2.Utils
{
    public unsafe struct SirenInstance
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct sirenInstanceData
        {
            [FieldOffset(0x000)] public uint sirenOnTime;
            [FieldOffset(0x004)] public float sirenTimeDelta;
            [FieldOffset(0x008)] public int lastSirenBeat;
        };

        private static short sirenDataOffset;

        static SirenInstance()
        {
            GetSirenDataOffset();
        }

        public SirenInstance(Vehicle vehicle)
        {
            this.Vehicle = vehicle;
        }

        public Vehicle Vehicle { get; }
        private sirenInstanceData* data => *(sirenInstanceData**)(Vehicle.MemoryAddress + sirenDataOffset);

        public uint SirenOnTime => data->sirenOnTime;
        public float SirenTimeDelta => data->sirenTimeDelta;
        public int TotalSirenBeats => data->lastSirenBeat;
        public int CurrentSirenBeat => data->lastSirenBeat % 32;
        public int GameTimeOffset => TotalSirenBeats < 0 ? 0 : (int)(Math.Round(SirenOnTime + SirenTimeDelta, 0) - Game.GameTime);

        public void SetSirenOnTime(uint gameTime)
        {
            if (TotalSirenBeats >= 0)
                data->sirenOnTime = (uint)(gameTime + GameTimeOffset);
        }

        public void SetSirenOnTime() => SetSirenOnTime(Game.GameTime);

        private static void GetSirenDataOffset()
        {
            IntPtr addr = Game.FindPattern("48 89 B7 ?? ?? ?? ?? 48 8B 0B");
            if (addr == IntPtr.Zero) throw new Exception("Can't find pattern for the siren data");
            addr = addr + 3;
            sirenDataOffset = *(short*)addr;
        }

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
            Game.DisplayNotification($"Reset siren on time to {Game.GameTime}");
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
