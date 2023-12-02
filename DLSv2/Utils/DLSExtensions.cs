using Rage;

namespace DLSv2.Utils
{
    using Core;
    using System.Collections.Generic;
    using System.Linq;

    internal static class DLSExtensions
    {
        internal static ManagedVehicle GetManagedVehicle(this Vehicle veh)
        {
            if (!veh) return null;
            if (Entrypoint.ManagedVehicles.TryGetValue(veh, out var managedVehicle))
                return managedVehicle;

            managedVehicle = new ManagedVehicle(veh);
            Entrypoint.ManagedVehicles.Add(veh, managedVehicle);
            return managedVehicle;
        }

        internal static DLSModel GetDLS(this Vehicle veh)
        {
            if (!veh) return null;
            return Entrypoint.DLSModels.TryGetValue(veh.Model, out var dlsModel) ? dlsModel : null;
        }

        public static Mode GetEmptyMode(this Vehicle veh)
        {
            var defaultSirenSetting = veh.GetDefaultSirenSetting();

            defaultSirenSetting.LeftTailLightSequencer = new SequencerWrapper("00000000000000000000000000000000");
            defaultSirenSetting.RightHeadLightSequencer = new SequencerWrapper("00000000000000000000000000000000");
            defaultSirenSetting.LeftTailLightSequencer = new SequencerWrapper("00000000000000000000000000000000");
            defaultSirenSetting.RightTailLightSequencer = new SequencerWrapper("00000000000000000000000000000000");
            foreach (var sirenEntry in defaultSirenSetting.Sirens)
            {
                sirenEntry.Rotation.Sequence = new Sequencer("00000000000000000000000000000000");
                sirenEntry.Flashiness.Sequence = new Sequencer("00000000000000000000000000000000");
            }

            return new Mode()
            {
                Name = "DLS_EMPTY_MODE",
                SirenSettings = defaultSirenSetting

            };
        }

        internal static SirenSetting GetDefaultSirenSetting(this Vehicle veh)
        {
            var defaultEl = veh.DefaultEmergencyLighting;
            return new SirenSetting()
            {
                TimeMultiplier = defaultEl.TimeMultiplier,
                LightFalloffMax = defaultEl.LightFalloffMax,
                LightFalloffExponent = defaultEl.LightFalloffExponent,
                LightInnerConeAngle = defaultEl.LightInnerConeAngle,
                LightOuterConeAngle = defaultEl.LightOuterConeAngle,
                LightOffset = defaultEl.LightOffset,
                TextureHash = defaultEl.TextureHash,
                SequencerBPM = defaultEl.SequencerBpm,
                UseRealLights = defaultEl.UseRealLights,
                LeftHeadLightSequencer = new SequencerWrapper(defaultEl.LeftHeadLightSequence),
                LeftHeadLightMultiples = defaultEl.LeftHeadLightMultiples,
                RightHeadLightSequencer = new SequencerWrapper(defaultEl.RightHeadLightSequence),
                RightHeadLightMultiples = defaultEl.RightHeadLightMultiples,
                LeftTailLightSequencer = new SequencerWrapper(defaultEl.LeftTailLightSequence),
                LeftTailLightMultiples = defaultEl.LeftTailLightMultiples,
                RightTailLightSequencer = new SequencerWrapper(defaultEl.RightTailLightSequence),
                RightTailLightMultiples = defaultEl.RightTailLightMultiples,

                Sirens = Enumerable.Range(0, EmergencyLighting.MaxLights).Select(i => new SirenEntry(i + 1)
                {
                    // Main Light Settings
                    LightColor = defaultEl.Lights[i].Color,
                    Intensity = defaultEl.Lights[i].Intensity,
                    LightGroup = defaultEl.Lights[i].LightGroup,
                    Rotate = defaultEl.Lights[i].Rotate,
                    Scale = defaultEl.Lights[i].Scale,
                    ScaleFactor = defaultEl.Lights[i].ScaleFactor,
                    Flash = defaultEl.Lights[i].Flash,
                    SpotLight = defaultEl.Lights[i].SpotLight,
                    CastShadows = defaultEl.Lights[i].CastShadows,
                    Light = defaultEl.Lights[i].Light,

                    // Rotation Settings
                    Rotation = new LightDetailEntry()
                    {
                        DeltaDeg = defaultEl.Lights[i].RotationDelta,
                        StartDeg = defaultEl.Lights[i].RotationStart,
                        Speed = defaultEl.Lights[i].RotationSpeed,
                        Sequence = new Sequencer(defaultEl.Lights[i].RotationSequence),
                        Multiples = defaultEl.Lights[i].RotationMultiples,
                        Direction = defaultEl.Lights[i].RotationDirection,
                        SyncToBPM = defaultEl.Lights[i].RotationSynchronizeToBpm
                    },

                    // Flashiness Settings
                    Flashiness = new LightDetailEntry
                    {
                        DeltaDeg = defaultEl.Lights[i].FlashinessDelta,
                        StartDeg = defaultEl.Lights[i].FlashinessStart,
                        Speed = defaultEl.Lights[i].FlashinessSpeed,
                        Sequence = new Sequencer(defaultEl.Lights[i].FlashinessSequence),
                        Multiples = defaultEl.Lights[i].FlashinessMultiples,
                        Direction = defaultEl.Lights[i].FlashinessDirection,
                        SyncToBPM = defaultEl.Lights[i].FlashinessSynchronizeToBpm
                    },

                    // Corona Settings
                    Corona = new CoronaEntry()
                    {
                        CoronaIntensity = defaultEl.Lights[i].CoronaIntensity,
                        CoronaSize = defaultEl.Lights[i].CoronaSize,
                        CoronaPull = defaultEl.Lights[i].CoronaPull,
                        CoronaFaceCamera = defaultEl.Lights[i].CoronaFaceCamera,
                    }
                }).ToArray()
            };
        }
    }
}