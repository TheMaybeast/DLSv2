using DLSv2.Utils;
using Rage;
using System;

namespace DLSv2.Core
{
    internal static class SirenApply
    {
        public static void ApplySirenSettingsToEmergencyLighting(SirenSetting setting, EmergencyLighting els)
        {
            els.TimeMultiplier = setting.TimeMultiplier != null ? setting.TimeMultiplier.Value : els.TimeMultiplier;
            els.LightFalloffMax = setting.LightFalloffMax != null ? setting.LightFalloffMax.Value : els.LightFalloffMax;
            els.LightFalloffExponent = setting.LightFalloffExponent != null ? setting.LightFalloffExponent.Value : els.LightFalloffExponent;
            els.LightInnerConeAngle = setting.LightInnerConeAngle != null ? setting.LightInnerConeAngle.Value : els.LightInnerConeAngle;
            els.LightOuterConeAngle = setting.LightOuterConeAngle != null ? setting.LightOuterConeAngle.Value : els.LightOuterConeAngle;
            els.LightOffset = setting.LightOffset != null ? setting.LightOffset.Value : els.LightOffset;
            els.TextureHash = setting.TextureHash != 0 ? setting.TextureHash : els.TextureHash;
            els.SequencerBpm = setting.SequencerBPM != null ? setting.SequencerBPM.Value : els.SequencerBpm;
            els.UseRealLights = setting.UseRealLights != null ? setting.UseRealLights.Value :els.UseRealLights;
            els.LeftHeadLightSequence = setting.LeftHeadLightSequencer != null ? setting.LeftHeadLightSequencer : new SequencerWrapper(els.LeftHeadLightSequence);
            els.LeftHeadLightMultiples = setting.LeftHeadLightMultiples != null ? setting.LeftHeadLightMultiples.Value : els.LeftHeadLightMultiples;
            els.RightHeadLightSequence = setting.RightHeadLightSequencer != null ? setting.RightHeadLightSequencer : new SequencerWrapper(els.RightHeadLightSequence);
            els.RightHeadLightMultiples = setting.RightHeadLightMultiples != null ? setting.RightHeadLightMultiples.Value : els.RightHeadLightMultiples;
            els.LeftTailLightSequence = setting.LeftTailLightSequencer != null ? setting.LeftTailLightSequencer : new SequencerWrapper(els.LeftTailLightSequence);
            els.LeftTailLightMultiples = setting.LeftTailLightMultiples != null ? setting.LeftTailLightMultiples.Value : els.LeftTailLightMultiples;
            els.RightTailLightSequence = setting.RightTailLightSequencer != null ? setting.RightTailLightSequencer : new SequencerWrapper(els.RightTailLightSequence);
            els.RightTailLightMultiples = setting.RightTailLightMultiples != null ? setting.RightTailLightMultiples.Value : els.RightTailLightMultiples;

            foreach (SirenEntry siren in setting.Sirens)
            {
                if (siren == null) continue;

                SirenEntry entry = siren;
                EmergencyLight light = els.Lights[siren.ID.ToInt32() - 1];

                // Main light settings
                light.Color = entry.LightColor != null ? entry.LightColor.Value : light.Color;
                light.Intensity = entry.Intensity != null ? entry.Intensity : new ValueItem<float>(light.Intensity);
                light.LightGroup = entry.LightGroup != null ? entry.LightGroup : new ValueItem<byte>(light.LightGroup);
                light.Rotate = entry.Rotate != null ? entry.Rotate : new ValueItem<bool>(light.Rotate);
                light.Scale = entry.Scale != null ? entry.Scale : new ValueItem<bool>(light.Scale);
                light.ScaleFactor = entry.ScaleFactor != null ? entry.ScaleFactor : new ValueItem<byte>(light.ScaleFactor);
                light.Flash = entry.Flash != null ? entry.Flash : new ValueItem<bool>(light.Flash);
                light.SpotLight = entry.SpotLight != null ? entry.SpotLight : new ValueItem<bool>(light.SpotLight);
                light.CastShadows = entry.CastShadows != null ? entry.CastShadows : new ValueItem<bool>(light.CastShadows);
                light.Light = entry.Light != null ? entry.Light : new ValueItem<bool>(light.Light);

                // Corona settings
                light.CoronaIntensity = entry.Corona.CoronaIntensity != null ? entry.Corona.CoronaIntensity : new ValueItem<float>(light.CoronaIntensity);
                light.CoronaSize = entry.Corona.CoronaSize != null ? entry.Corona.CoronaSize : new ValueItem<float>(light.CoronaSize);
                light.CoronaPull = entry.Corona.CoronaPull != null ? entry.Corona.CoronaPull : new ValueItem<float>(light.CoronaPull);
                light.CoronaFaceCamera = entry.Corona.CoronaFaceCamera != null ? entry.Corona.CoronaFaceCamera : new ValueItem<bool>(light.CoronaFaceCamera);

                // Rotation settings
                light.RotationDelta = entry.Rotation.DeltaDeg != 999 ? entry.Rotation.DeltaDeg : light.RotationDelta;
                light.RotationStart = entry.Rotation.StartDeg != 999 ? entry.Rotation.StartDeg : light.RotationStart;
                light.RotationSpeed = entry.Rotation.Speed != null ? entry.Rotation.Speed : new ValueItem<float>(light.RotationSpeed);
                light.RotationSequence = entry.Rotation.Sequence != null ? entry.Rotation.Sequence : new Sequencer(light.RotationSequence);
                light.RotationMultiples = entry.Rotation.Multiples != null ? entry.Rotation.Multiples : new ValueItem<byte>(light.RotationMultiples);
                light.RotationDirection = entry.Rotation.Direction != null ? entry.Rotation.Direction : new ValueItem<bool>(light.RotationDirection);
                light.RotationSynchronizeToBpm = entry.Rotation.SyncToBPM != null ? entry.Rotation.SyncToBPM : new ValueItem<bool>(light.RotationSynchronizeToBpm);

                // Flash settings
                light.FlashinessDelta = entry.Flashiness.DeltaDeg != 999 ? entry.Flashiness.DeltaDeg : light.FlashinessDelta;
                light.FlashinessStart = entry.Flashiness.StartDeg != 999 ? entry.Flashiness.StartDeg : light.FlashinessStart;
                light.FlashinessSpeed = entry.Flashiness.Speed != null ? entry.Flashiness.Speed : new ValueItem<float>(light.FlashinessSpeed);
                light.FlashinessSequenceRaw = entry.Flashiness.Sequence != null ? entry.Flashiness.Sequence : new Sequencer(light.FlashinessSequenceRaw);
                light.FlashinessMultiples = entry.Flashiness.Multiples != null ? entry.Flashiness.Multiples : new ValueItem<byte>(light.FlashinessMultiples);
                light.FlashinessDirection = entry.Flashiness.Direction != null ? entry.Flashiness.Direction : new ValueItem<bool>(light.FlashinessDirection);
                light.FlashinessSynchronizeToBpm = entry.Flashiness.SyncToBPM != null ? entry.Flashiness.SyncToBPM : new ValueItem<bool>(light.FlashinessSynchronizeToBpm);
            }
        }
    }
}
