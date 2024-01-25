using System.Collections.Generic;
using Rage;
using Rage.Native;

namespace DLSv2.Utils;

public static class Audio
{
    private static List<int> usedSoundIds = new();

    public static int PlaySoundFromEntity(this Entity entity, string audioName, string audioRef)
    {
        int newID = NativeFunction.Natives.GET_SOUND_ID<int>();
        ("Allocated Sound ID ["+newID+"]").ToLog(LogLevel.DEBUG);
        usedSoundIds.Add(newID);
        if (string.IsNullOrEmpty(audioRef))
            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(newID, audioName, entity, 0, 0, 0);
        else
            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY(newID, audioName, entity, audioRef, 0, 0);
        return newID;
    }

    public static void StopSound(int soundId)
    {
        NativeFunction.Natives.STOP_SOUND(soundId);
        NativeFunction.Natives.RELEASE_SOUND_ID(soundId);
        ("Released Sound ID [" + soundId + "]").ToLog(LogLevel.DEBUG);
        usedSoundIds.Remove(soundId);
    }
}