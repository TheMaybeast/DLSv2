using System.Collections.Generic;
using System.Linq;
using Rage;

namespace DLSv2.Utils;

using Core;

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

    public static LightMode GetEmptyMode(this Vehicle veh)
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

        return new LightMode()
        {
            Name = "DLS_EMPTY_MODE",
            SirenSettings = defaultSirenSetting
        };
    }

    private static EmergencyLighting blankEL = EmergencyLighting.GetByName("DLS_EMPTY_LIGHTING_DO_NOT_EDIT") ?? new EmergencyLighting() { Name = "DLS_EMPTY_LIGHTING_DO_NOT_EDIT" };

    internal static SirenSetting GetDefaultSirenSetting(this Vehicle veh)
    {
        // Use a blank EmergencyLighting instance if the vehicle does not have a default
        var defaultEl = veh.DefaultEmergencyLighting ?? blankEL;

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

            Sirens = Enumerable.Range(0, EmergencyLighting.MaxLights).Select(i => {
                
                // Copy settings from default for all sirens that exist on the setting, and pull in 
                // blank entries for any that don't (which can happen for built-in siren settings that 
                // have only 20 entries even if SSLA is installed)
                var light = (i < defaultEl.Lights.Length) ? defaultEl.Lights[i] : blankEL.Lights[0];
                
                return new SirenEntry(i + 1)
                {
                    // Main Light Settings
                    LightColor = light.Color,
                    Intensity = light.Intensity,
                    LightGroup = light.LightGroup,
                    Rotate = light.Rotate,
                    Scale = light.Scale,
                    ScaleFactor = light.ScaleFactor,
                    Flash = light.Flash,
                    SpotLight = light.SpotLight,
                    CastShadows = light.CastShadows,
                    Light = light.Light,

                    // Rotation Settings
                    Rotation = new LightDetailEntry()
                    {
                        DeltaDeg = light.RotationDelta,
                        StartDeg = light.RotationStart,
                        Speed = light.RotationSpeed,
                        Sequence = new Sequencer(light.RotationSequence),
                        Multiples = light.RotationMultiples,
                        Direction = light.RotationDirection,
                        SyncToBPM = light.RotationSynchronizeToBpm
                    },

                    // Flashiness Settings
                    Flashiness = new LightDetailEntry()
                    {
                        DeltaDeg = light.FlashinessDelta,
                        StartDeg = light.FlashinessStart,
                        Speed = light.FlashinessSpeed,
                        Sequence = new Sequencer(light.FlashinessSequence),
                        Multiples = light.FlashinessMultiples,
                        Direction = light.FlashinessDirection,
                        SyncToBPM = light.FlashinessSynchronizeToBpm
                    },

                    // Corona Settings
                    Corona = new CoronaEntry()
                    {
                        CoronaIntensity = light.CoronaIntensity,
                        CoronaSize = light.CoronaSize,
                        CoronaPull = light.CoronaPull,
                        CoronaFaceCamera = light.CoronaFaceCamera,
                    }
                };
            }).ToArray()
        };
    }

    public static EmergencyLighting GetDLSEmergencyLighting(this Vehicle vehicle)
    {
        // Safety checks
        if (!vehicle) return null;

        EmergencyLighting eL;
        uint key = vehicle.Handle.Value;
        string name = "DLS_" + key.ToString("X");

        if (Entrypoint.ELUsedPool.TryGetValue(key, out var elFromPool))
        {
            eL = elFromPool;
        }
        else if (Entrypoint.ELAvailablePool.Count > 0)
        {
            eL = Entrypoint.ELAvailablePool[0];
            Entrypoint.ELAvailablePool.Remove(eL);
            eL.Name = name;
            ("Allocated \"" + eL.Name + "\" (now \"" + key + "\") EL from Available Pool").ToLog(LogLevel.DEBUG);
        }
        else if (EmergencyLighting.GetByName(name) != null)
        {
            eL = EmergencyLighting.GetByName(name);
            ("Allocated \"" + eL.Name + "\" EL from Game Memory").ToLog(LogLevel.DEBUG);
        }
        else
        {
            eL = new EmergencyLighting();
            eL.Name = name;
            ("Created \"" + eL.Name + "\" EL").ToLog(LogLevel.DEBUG);
        }

        if (!Entrypoint.ELUsedPool.ContainsKey(key))
            Entrypoint.ELUsedPool.Add(key, eL);

        return eL;
    }

    public static void ApplyLightModes(this ManagedVehicle managedVehicle, List<LightMode> modes)
    {
        // Safety checks
        if (managedVehicle == null) return;
        var vehicle = managedVehicle.Vehicle;
        if (!vehicle) return;

        managedVehicle.extendedSequences.Clear();

        EmergencyLighting eL = managedVehicle.eL;

        SirenApply.ApplySirenSettingsToEmergencyLighting(managedVehicle.EmptyMode.SirenSettings, eL);

        var shouldYield = false;
        var extras = new Dictionary<int, bool>();
        Animation anim = null;
        var paints = new Dictionary<int, int>();
        var sequences = new Dictionary<int, string>();

        foreach (var mode in modes)
        {
            // apply siren settings including regular sequences
            if (mode.ApplyDefaultSirenSettings && vehicle.DefaultEmergencyLighting != null)
                eL.Copy(vehicle.DefaultEmergencyLighting);

            
            if (mode.SirenSettings != null)
            {
                // remove any overridden sequences from extended sequence list
                foreach (var siren in mode.SirenSettings.Sirens)
                    if (siren.Flashiness?.Sequence?.Sequence != null)
                        foreach (int sirenID in siren.sirenIDs)
                            managedVehicle.extendedSequences.Remove(sirenID);

                // apply siren settings if specified
                SirenApply.ApplySirenSettingsToEmergencyLighting(mode.SirenSettings, eL);
            }

            // add regular sequences
            foreach (var seq in mode.StandardSequences)
            {
                int i = seq.Key - 1;
                if (i < eL.Lights.Length) eL.Lights[i].FlashinessSequence = seq.Value;
                managedVehicle.extendedSequences.Remove(seq.Key);
            }

            // add extended sequences            
            foreach (var seq in mode.ExtendedSequences)
                managedVehicle.extendedSequences[seq.Key] = seq.Value;

            // Sets the extras for the specific mode
            foreach (var extra in mode.Extra)
                extras[extra.ID] = extra.Enabled;

            // Sets modkits for the specific mode
            foreach (var kit in mode.ModKits)
                if (vehicle.HasModkitMod(kit.Type) && vehicle.GetModkitModCount(kit.Type) > kit.Index)
                    vehicle.SetModkitModIndex(kit.Type, kit.Index);

            // Set most recent mode's animation as the active animation, if present
            if (mode.Animation != null) anim = mode.Animation;
              
            // Sets vehicle paints
            foreach (var paint in mode.PaintJobs)
            {
                paints[paint.PaintSlot] = paint.ColorCode;
            }

            // Sets the yield setting
            if (mode.Yield != null) shouldYield = mode.Yield.Enabled;

            // Sets the indicators
            if (mode.Indicators != null)
            {
                switch (mode.Indicators.ToLower())
                {
                    case "off":
                        managedVehicle.IndStatus = VehicleIndicatorLightsStatus.Off;
                        break;
                    case "rightonly":
                        managedVehicle.IndStatus = VehicleIndicatorLightsStatus.RightOnly;
                        break;
                    case "leftonly":
                        managedVehicle.IndStatus = VehicleIndicatorLightsStatus.LeftOnly;
                        break;
                    case "both":
                        managedVehicle.IndStatus = VehicleIndicatorLightsStatus.Both;
                        break;
                }

                managedVehicle.Vehicle.IndicatorLightsStatus = managedVehicle.IndStatus;
            }

            managedVehicle.LightModes[mode.Name].Enabled = true;
        }

        // Adjust time multiplier if a drift is configured
        var newTimeMultiplier = SyncManager.GetAdjustedMultiplier(vehicle, eL.TimeMultiplier);
        if (newTimeMultiplier.HasValue) eL.TimeMultiplier = newTimeMultiplier.Value;

        // Set enabled extras first, then disabled extras second, because <extraIncludes> in vehicles.meta 
        // can cause enabling one extra to enable other linked extras. By disabling second, we turn back off 
        // any extras that are explicitly set to be turned off.
        foreach (var extra in extras.OrderByDescending(e => e.Value))
        {
            if (!vehicle.HasExtra(extra.Key)) continue;
            if (!managedVehicle.ManagedExtras.ContainsKey(extra.Key)) managedVehicle.ManagedExtras[extra.Key] = vehicle.IsExtraEnabled(extra.Key);
            vehicle.SetExtra(extra.Key, extra.Value);
        }

        // Reset any extras not specified by the current mode back to their previous setting before they were set by any mode
        foreach (var extra in managedVehicle.ManagedExtras.Keys.ToArray())
        {
            if (extras.ContainsKey(extra)) continue;

            if (vehicle.HasExtra(extra)) vehicle.SetExtra(extra, managedVehicle.ManagedExtras[extra]);
            managedVehicle.ManagedExtras.Remove(extra);
        }

        // Update animations
        if (managedVehicle.ActiveAnim != anim)
        {
            // Stop current animation if exists
            if (managedVehicle.ActiveAnim != null) vehicle.StopAnim(managedVehicle.ActiveAnim);
            // Start new animation if specified. Use a fiber because loading an
            // animation dictionary can require yielding the fiber it is called in
            if (anim != null) GameFiber.StartNew(() => vehicle.LoadAndPlayAnim(anim));
            // Save new animation
            managedVehicle.ActiveAnim = anim;
        }

        // Set new paint colors and record original values of any paint settings
        foreach (var paint in paints)
        {
            if (!managedVehicle.ManagedPaint.ContainsKey(paint.Key)) managedVehicle.ManagedPaint[paint.Key] = vehicle.GetPaint(paint.Key);
            vehicle.SetPaint(paint.Key, paint.Value);
        }

        // Reset any paint not specified by current mode back to previous setting
        foreach (var paint in managedVehicle.ManagedPaint.Keys.ToArray())
        {
            if (paints.ContainsKey(paint)) continue;

            vehicle.SetPaint(paint, managedVehicle.ManagedPaint[paint]);
            managedVehicle.ManagedPaint.Remove(paint);
        }

        // Update extended sequences
        if (managedVehicle.extendedSequences.Count > 0)
        {
            // Game.LogTrivialDebug("force update extended sequence");
            managedVehicle.ProcessExtendedSequences(true);
        }

        vehicle.ShouldVehiclesYieldToThisVehicle = shouldYield;

        managedVehicle.Vehicle.EmergencyLightingOverride = eL;
    }

    internal static void DebugCurrentModes(this Vehicle vehicle, bool showConditions = true)
    {
        if (vehicle == null)
        {
            ("Vehicle is null").ToLog(LogLevel.ERROR);
            return;
        }

        var managedVehicle = vehicle.GetManagedVehicle();

        if (managedVehicle == null)
        {
            ("Vehicle is not a managed vehicle").ToLog(LogLevel.ERROR);
            return;
        }

        ("").ToLog(LogLevel.DEVMODE);
        ("--------------------------------------------------------------------------------").ToLog(LogLevel.DEVMODE);
        ($"Active modes for managed DLS vehicle {managedVehicle.Vehicle.Model.Name} - {managedVehicle.VehicleHandle}").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);

        ("").ToLog(LogLevel.DEVMODE);
        ($"Is Player Vehicle: {managedVehicle.Vehicle.IsPlayerVehicle()}").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);

        ("Light Control Groups:").ToLog(LogLevel.DEVMODE);
        foreach (var cg in managedVehicle.LightControlGroups)
        {
            string modes = "";
            foreach (var modeIndex in cg.Value.ActiveIndexes)
            {
                modes += string.Join(" + ", cg.Value.BaseControlGroup.Modes[modeIndex].Modes);
            }
            ($"  {boolToCheck(cg.Value.Enabled)}\t{cg.Key}: ({string.Join(" + ", cg.Value.ActiveIndexes)}) = {modes}").ToLog(LogLevel.DEVMODE);
        }

        ("").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);
        ("Vanilla Settings:").ToLog(LogLevel.DEVMODE);
        ($"  {boolToCheck(vehicle.IsSirenOn)}  IsSirenOn").ToLog(LogLevel.DEVMODE);
        ($"  {boolToCheck(vehicle.IsSirenSilent)}  IsSirenSilent").ToLog(LogLevel.DEVMODE);
        ($"  {boolToCheck(vehicle.ShouldVehiclesYieldToThisVehicle)}  ShouldYield").ToLog(LogLevel.DEVMODE);

        ("").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);
        ("Light Modes:").ToLog(LogLevel.DEVMODE);
        foreach (var slm in managedVehicle.LightModes)
        {
            string modeName = slm.Key;
            bool enabled = slm.Value.Enabled || slm.Value.EnabledByTrigger;
            var mode = slm.Value.BaseMode;
            ($"  {boolToCheck(enabled)}  {modeName}").ToLog(LogLevel.DEVMODE);

            if (showConditions && mode.Triggers != null && mode.Triggers.NestedConditions.Count > 0)
            {
                bool triggers = mode.Triggers.GetInstance(managedVehicle).LastTriggered;
                ($"       {boolToCheck(triggers)}  Triggers:").ToLog(LogLevel.DEVMODE);
                logNestedConditions(managedVehicle, mode.Triggers, 5);
            }

            if (showConditions && mode.Requirements != null && mode.Requirements.NestedConditions.Count > 0)
            {
                bool reqs = mode.Requirements.GetInstance(managedVehicle).LastTriggered;
                ($"       {boolToCheck(reqs)}  Requirements:").ToLog(LogLevel.DEVMODE);
                logNestedConditions(managedVehicle, mode.Requirements, 5);
            }
        }

        ("").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);
        ("Active Light Modes:").ToLog(LogLevel.DEVMODE);
        foreach (var mode in managedVehicle.LightModes.Where(x => x.Value.Enabled))
        {
            ($"  {mode.Key}").ToLog(LogLevel.DEVMODE);
        }

        ("").ToLog(LogLevel.DEVMODE);
        ("--------------------------------------------------------------------------------").ToLog(LogLevel.DEVMODE);
        ("").ToLog(LogLevel.DEVMODE);
    }

    private static string boolToCheck(bool state) => state ? "[x]" : "[ ]";

    private static void logNestedConditions(ManagedVehicle mv, GroupConditions group, int level = 0)
    {
        string indent = new string(' ', 2 * level);
        foreach (var condition in group.NestedConditions)
        {
            var inst = condition.GetInstance(mv);
            string updateInfo = inst.TimeSinceUpdate == CachedGameTime.GameTime ? "never" : $"{inst.TimeSinceUpdate} ms ago";
            ($"{indent} - {boolToCheck(inst.LastTriggered)} {condition.GetType().Name} ({updateInfo})").ToLog(LogLevel.DEVMODE);
            if (condition is GroupConditions subGroup)
            {
                logNestedConditions(mv, subGroup, level + 1);
            }
        }
    }
}