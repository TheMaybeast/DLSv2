using DLSv2.Core;
using Rage;
using Rage.Native;

namespace DLSv2.Utils;

public static class Animations
{
    public static void SetAnimSpeed(this Vehicle vehicle, string animDict, string animName, float speed) =>
        NativeFunction.Natives.SET_ENTITY_ANIM_SPEED(vehicle, animDict, animName, speed);

    public static bool PlayAnim(this Vehicle vehicle, string animDict, string animName, float blend, bool loop, bool keepLastFrame, float startPos = 0f, int flags = 0) =>
        NativeFunction.Natives.PLAY_ENTITY_ANIM<bool>(vehicle, animName, animDict, blend, loop, keepLastFrame, false, startPos, flags);

    public static bool LoadAndPlayAnim(this Vehicle vehicle, AnimationDictionary animDict, string animName, float blend, bool loop, bool keepLastFrame, float startPos = 0f, int flags = 0)
    {
        if (!animDict.IsLoaded)
        {
            if (!NativeFunction.Natives.DOES_ANIM_DICT_EXIST<bool>(animDict.Name)) return false;

            animDict.LoadAndWait();
        }
            
        return PlayAnim(vehicle, animDict.Name, animName, blend, loop, keepLastFrame, startPos, flags);
    }

    public static bool LoadAndPlayAnim(this Vehicle vehicle, Animation anim)
    {
        bool result = LoadAndPlayAnim(vehicle, anim.AnimDict, anim.AnimName, anim.BlendDelta, anim.Loop, anim.StayInLastFrame, anim.StartPhase, anim.Flags);
        if (anim.Speed.HasValue) GameFiber.StartNew(() => SetAnimSpeed(vehicle, anim.AnimDict, anim.AnimName, anim.Speed.Value));
        return result;
    }

    public static bool StopAnim(this Vehicle vehicle, string animDict, string animName, float blend = 4f) =>
        NativeFunction.Natives.STOP_ENTITY_ANIM<bool>(vehicle, animName, animDict, blend);

    public static bool StopAnim(this Vehicle vehicle, Animation anim) =>
        StopAnim(vehicle, anim.AnimDict, anim.AnimName, anim.BlendDelta);

    public static bool IsAnimEventActive(this Vehicle vehicle, uint eventHash) =>
        NativeFunction.Natives.HAS_ANIM_EVENT_FIRED<bool>(vehicle, eventHash);

    public static bool IsAnimEventActive(this Vehicle vehicle, string eventName) =>
        IsAnimEventActive(vehicle, Game.GetHashKey(eventName));
}