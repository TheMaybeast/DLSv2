using DLSv2.Utils;
using Rage;

namespace DLSv2.Core;

internal static class SirenApply
{
    public static void ApplySirenSettingsToEmergencyLighting(SirenSetting setting, EmergencyLighting els)
    {
        els.TimeMultiplier = setting.TimeMultiplier?.Value ?? els.TimeMultiplier;
        els.LightFalloffMax = setting.LightFalloffMax?.Value ?? els.LightFalloffMax;
        els.LightFalloffExponent = setting.LightFalloffExponent?.Value ?? els.LightFalloffExponent;
        els.LightInnerConeAngle = setting.LightInnerConeAngle?.Value ?? els.LightInnerConeAngle;
        els.LightOuterConeAngle = setting.LightOuterConeAngle?.Value ?? els.LightOuterConeAngle;
        els.LightOffset = setting.LightOffset?.Value ?? els.LightOffset;
        els.TextureHash = setting.TextureHash ?? els.TextureHash;
        els.SequencerBpm = setting.SequencerBPM?.Value ?? els.SequencerBpm;
        els.UseRealLights = setting.UseRealLights?.Value ?? els.UseRealLights;
        els.LeftHeadLightSequenceRaw = setting.LeftHeadLightSequencer?.Sequencer?.SequenceRaw ?? els.LeftHeadLightSequenceRaw;
        els.LeftHeadLightMultiples = setting.LeftHeadLightMultiples?.Value ?? els.LeftHeadLightMultiples;
        els.RightHeadLightSequenceRaw = setting.RightHeadLightSequencer?.Sequencer?.SequenceRaw ?? els.RightHeadLightSequenceRaw;
        els.RightHeadLightMultiples = setting.RightHeadLightMultiples?.Value ?? els.RightHeadLightMultiples;
        els.LeftTailLightSequenceRaw = setting.LeftTailLightSequencer?.Sequencer?.Value ?? els.LeftTailLightSequenceRaw;
        els.LeftTailLightMultiples = setting.LeftTailLightMultiples?.Value ?? els.LeftTailLightMultiples;
        els.RightTailLightSequenceRaw = setting.RightTailLightSequencer?.Sequencer?.Value ?? els.RightTailLightSequenceRaw;
        els.RightTailLightMultiples = setting.RightTailLightMultiples?.Value ?? els.RightTailLightMultiples;

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
                light.Intensity = entry.Intensity?.Value ?? light.Intensity;
                light.LightGroup = entry.LightGroup?.Value ?? light.LightGroup;
                light.Rotate = entry.Rotate?.Value ?? light.Rotate;
                light.Scale = entry.Scale?.Value ?? light.Scale;
                light.ScaleFactor = entry.ScaleFactor?.Value ?? light.ScaleFactor;
                light.Flash = entry.Flash?.Value ?? light.Flash;
                light.SpotLight = entry.SpotLight?.Value ?? light.SpotLight;
                light.CastShadows = entry.CastShadows?.Value ?? light.CastShadows;
                light.Light = entry.Light?.Value ?? light.Light;

                // Corona settings
                light.CoronaIntensity = entry.Corona.CoronaIntensity?.Value ?? light.CoronaIntensity;
                light.CoronaSize = entry.Corona.CoronaSize?.Value ?? light.CoronaSize;
                light.CoronaPull = entry.Corona.CoronaPull?.Value ?? light.CoronaPull;
                light.CoronaFaceCamera = entry.Corona.CoronaFaceCamera?.Value ?? light.CoronaFaceCamera;

                // Rotation settings
                light.RotationDelta = entry.Rotation.DeltaDeg ?? light.RotationDelta;
                light.RotationStart = entry.Rotation.StartDeg ?? light.RotationStart;
                light.RotationSpeed = entry.Rotation.Speed?.Value ?? light.RotationSpeed;
                light.RotationSequenceRaw = entry.Rotation.Sequence ?? light.RotationSequenceRaw;
                light.RotationMultiples = entry.Rotation.Multiples?.Value ?? light.RotationMultiples;
                light.RotationDirection = entry.Rotation.Direction?.Value ?? light.RotationDirection;
                light.RotationSynchronizeToBpm = entry.Rotation.SyncToBPM?.Value ?? light.RotationSynchronizeToBpm;

                // Flash settings
                light.FlashinessDelta = entry.Flashiness.DeltaDeg ?? light.FlashinessDelta;
                light.FlashinessStart = entry.Flashiness.StartDeg ?? light.FlashinessStart;
                light.FlashinessSpeed = entry.Flashiness.Speed?.Value ?? light.FlashinessSpeed;
                light.FlashinessSequenceRaw = entry.Flashiness.Sequence ?? light.FlashinessSequenceRaw;
                light.FlashinessMultiples = entry.Flashiness.Multiples?.Value ?? light.FlashinessMultiples;
                light.FlashinessDirection = entry.Flashiness.Direction?.Value ?? light.FlashinessDirection;
                light.FlashinessSynchronizeToBpm = entry.Flashiness.SyncToBPM?.Value ?? light.FlashinessSynchronizeToBpm;
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