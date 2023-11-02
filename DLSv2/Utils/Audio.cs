using DLSv2.Core;
using Rage.Native;

namespace DLSv2.Utils
{
    internal class Audio
    {
        public static void PlayMode(ManagedVehicle mV, AudioMode mode)
        {
            if (mV.SoundIds.ContainsKey(mode.Name))
            {
                NativeFunction.Natives.STOP_SOUND(mV.SoundIds[mode.Name]);
                NativeFunction.Natives.RELEASE_SOUND_ID(mV.SoundIds[mode.Name]);
                Entrypoint.UsedSoundIDs.Remove(mV.SoundIds[mode.Name]);
                mV.SoundIds.Remove(mode.Name);
            }
            int newID = NativeFunction.Natives.GET_SOUND_ID<int>();
            mV.SoundIds[mode.Name] = newID;
            Entrypoint.UsedSoundIDs.Add(newID);
            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(newID, mode.Sound, mV.Vehicle, 0, 0, 0);
        }

        public static void StopMode(ManagedVehicle mV, string mode)
        {
            if (mV.SoundIds.ContainsKey(mode))
            {
                NativeFunction.Natives.STOP_SOUND(mV.SoundIds[mode]);
                NativeFunction.Natives.RELEASE_SOUND_ID(mV.SoundIds[mode]);
                Entrypoint.UsedSoundIDs.Remove(mV.SoundIds[mode]);
                mV.SoundIds.Remove(mode);
            }
        }
    }
}
