using DLSv2.Utils;
using Rage;

namespace DLSv2.Core;

internal static class SirenApply
{
    public static void ApplySirenSettingsToEmergencyLighting(SirenSetting setting, EmergencyLighting els)
    {
        if (setting.TimeMultiplier != null) els.TimeMultiplier = setting.TimeMultiplier.Value;
        if (setting.LightFalloffMax != null) els.LightFalloffMax = setting.LightFalloffMax.Value;
        if (setting.LightFalloffExponent != null) els.LightFalloffExponent = setting.LightFalloffExponent.Value;
        if (setting.LightInnerConeAngle != null) els.LightInnerConeAngle = setting.LightInnerConeAngle.Value;
        if (setting.LightOuterConeAngle != null) els.LightOuterConeAngle = setting.LightOuterConeAngle.Value;
        if (setting.LightOffset != null) els.LightOffset = setting.LightOffset.Value;
        if (setting.TextureHash != null) els.TextureHash = setting.TextureHash.Value;
        if (setting.SequencerBPM != null) els.SequencerBpm = setting.SequencerBPM.Value;
        if (setting.UseRealLights != null) els.UseRealLights = setting.UseRealLights.Value;
        if (setting.LeftHeadLightSequencer != null) els.LeftHeadLightSequenceRaw = setting.LeftHeadLightSequencer.Sequencer.Value;
        if (setting.LeftHeadLightMultiples != null) els.LeftHeadLightMultiples = setting.LeftHeadLightMultiples.Value;
        if (setting.RightHeadLightSequencer != null) els.RightHeadLightSequenceRaw = setting.RightHeadLightSequencer.Sequencer.Value;
        if (setting.RightHeadLightMultiples != null) els.RightHeadLightMultiples = setting.RightHeadLightMultiples.Value;
        if (setting.LeftTailLightSequencer != null) els.LeftTailLightSequenceRaw = setting.LeftTailLightSequencer.Sequencer.Value;
        if (setting.LeftTailLightMultiples != null) els.LeftTailLightMultiples = setting.LeftTailLightMultiples.Value;
        if (setting.RightTailLightSequencer != null) els.RightTailLightSequenceRaw = setting.RightTailLightSequencer.Sequencer.Value;
        if (setting.RightTailLightMultiples != null) els.RightTailLightMultiples = setting.RightTailLightMultiples.Value;

        foreach (SirenEntry entry in setting.Sirens)
        {
            if (entry == null) continue;

            foreach (int id in entry.sirenIDs)
            {
                if (id > EmergencyLighting.MaxLights)
                {
                    ($"Attempting to set unavailable siren, tried setting {id} when max is {EmergencyLighting.MaxLights}").ToLog(LogLevel.INFO);
                    continue;
                }
                
                EmergencyLight light = els.Lights[id - 1];

                // Main light settings
                light.Color = entry.LightColor ?? light.Color;
                if (entry.Intensity != null) light.Intensity = entry.Intensity.Value;
                if (entry.LightGroup != null) light.LightGroup = entry.LightGroup.Value;
                if (entry.Rotate != null) light.Rotate = entry.Rotate.Value;
                if (entry.Scale != null) light.Scale = entry.Scale.Value;
                if (entry.ScaleFactor != null) light.ScaleFactor = entry.ScaleFactor.Value;
                if (entry.Flash != null) light.Flash = entry.Flash.Value;
                if (entry.SpotLight != null) light.SpotLight = entry.SpotLight.Value;
                if (entry.CastShadows != null) light.CastShadows = entry.CastShadows.Value;
                if (entry.Light != null) light.Light = entry.Light.Value;

                // Corona settings
                if (entry.Corona.CoronaIntensity != null) light.CoronaIntensity = entry.Corona.CoronaIntensity.Value;
                if (entry.Corona.CoronaSize != null) light.CoronaSize = entry.Corona.CoronaSize.Value;
                if (entry.Corona.CoronaPull != null) light.CoronaPull = entry.Corona.CoronaPull.Value;
                if (entry.Corona.CoronaFaceCamera != null) light.CoronaFaceCamera = entry.Corona.CoronaFaceCamera.Value;

                // Rotation settings
                if (entry.Rotation.DeltaDeg.HasValue) light.RotationDelta = entry.Rotation.DeltaDeg.Value;
                if (entry.Rotation.StartDeg.HasValue) light.RotationStart = entry.Rotation.StartDeg.Value;
                if (entry.Rotation.Speed != null) light.RotationSpeed = entry.Rotation.Speed.Value;
                if (entry.Rotation.Sequence != null) light.RotationSequenceRaw = entry.Rotation.Sequence;
                if (entry.Rotation.Multiples != null) light.RotationMultiples = entry.Rotation.Multiples.Value;
                if (entry.Rotation.Direction != null) light.RotationDirection = entry.Rotation.Direction.Value;
                if (entry.Rotation.SyncToBPM != null) light.RotationSynchronizeToBpm = entry.Rotation.SyncToBPM.Value;

                // Flash settings
                if (entry.Flashiness.DeltaDeg.HasValue) light.FlashinessDelta = entry.Flashiness.DeltaDeg.Value;
                if (entry.Flashiness.StartDeg.HasValue) light.FlashinessStart = entry.Flashiness.StartDeg.Value;
                if (entry.Flashiness.Speed != null) light.FlashinessSpeed = entry.Flashiness.Speed.Value;
                if (entry.Flashiness.Sequence != null) light.FlashinessSequenceRaw = entry.Flashiness.Sequence;
                if (entry.Flashiness.Multiples != null) light.FlashinessMultiples = entry.Flashiness.Multiples.Value;
                if (entry.Flashiness.Direction != null) light.FlashinessDirection = entry.Flashiness.Direction.Value;
                if (entry.Flashiness.SyncToBPM != null) light.FlashinessSynchronizeToBpm = entry.Flashiness.SyncToBPM.Value;
            }
        }
    }

    public static void Copy(this EmergencyLighting target, EmergencyLighting source)
    {
        target.TimeMultiplier = source.TimeMultiplier;
        target.LightFalloffMax = source.LightFalloffMax;
        target.LightFalloffExponent = source.LightFalloffExponent;
        target.LightInnerConeAngle = source.LightInnerConeAngle;
        target.LightOuterConeAngle = source.LightOuterConeAngle;
        target.LightOffset = source.LightOffset;
        target.TextureHash = source.TextureHash;
        target.SequencerBpm = source.SequencerBpm;
        target.UseRealLights = source.UseRealLights;
        target.LeftHeadLightSequenceRaw = source.LeftHeadLightSequenceRaw;
        target.LeftHeadLightMultiples = source.LeftHeadLightMultiples;
        target.RightHeadLightSequenceRaw = source.RightHeadLightSequenceRaw;
        target.RightHeadLightMultiples = source.RightHeadLightMultiples;
        target.LeftTailLightSequenceRaw = source.LeftTailLightSequenceRaw;
        target.LeftTailLightMultiples = source.LeftTailLightMultiples;
        target.RightTailLightSequenceRaw = source.RightTailLightSequenceRaw;
        target.RightTailLightMultiples = source.RightTailLightMultiples;

        for (var i = 0; i < source.Lights.Length; i++)
        {
            var sourceLight = source.Lights[i];
            var targetLight = target.Lights[i];

            // Main light settings
            targetLight.Color = sourceLight.Color;
            targetLight.Intensity = sourceLight.Intensity;
            targetLight.LightGroup = sourceLight.LightGroup;
            targetLight.Rotate = sourceLight.Rotate;
            targetLight.Scale = sourceLight.Scale;
            targetLight.ScaleFactor = sourceLight.ScaleFactor;
            targetLight.Flash = sourceLight.Flash;
            targetLight.SpotLight = sourceLight.SpotLight;
            targetLight.CastShadows = sourceLight.CastShadows;
            targetLight.Light = sourceLight.Light;

            // Corona settings
            targetLight.CoronaIntensity = sourceLight.CoronaIntensity;
            targetLight.CoronaSize = sourceLight.CoronaSize;
            targetLight.CoronaPull = sourceLight.CoronaPull;
            targetLight.CoronaFaceCamera = sourceLight.CoronaFaceCamera;

            // Rotation settings
            targetLight.RotationDelta = sourceLight.RotationDelta;
            targetLight.RotationStart = sourceLight.RotationStart;
            targetLight.RotationSpeed = sourceLight.RotationSpeed;
            targetLight.RotationSequenceRaw = sourceLight.RotationSequenceRaw;
            targetLight.RotationMultiples = sourceLight.RotationMultiples;
            targetLight.RotationDirection = sourceLight.RotationDirection;
            targetLight.RotationSynchronizeToBpm = sourceLight.RotationSynchronizeToBpm;

            // Flash settings
            targetLight.FlashinessDelta = sourceLight.FlashinessDelta;
            targetLight.FlashinessStart = sourceLight.FlashinessStart;
            targetLight.FlashinessSpeed = sourceLight.FlashinessSpeed;
            targetLight.FlashinessSequenceRaw = sourceLight.FlashinessSequenceRaw;
            targetLight.FlashinessMultiples = sourceLight.FlashinessMultiples;
            targetLight.FlashinessDirection = sourceLight.FlashinessDirection;
            targetLight.FlashinessSynchronizeToBpm = sourceLight.FlashinessSynchronizeToBpm;
        }
    }
}