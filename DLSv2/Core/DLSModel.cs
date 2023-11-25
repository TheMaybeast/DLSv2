using Rage;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace DLSv2.Core
{
    using Utils;

    [XmlRoot("Model")]
    public class DLSModel
    {
        [XmlAttribute("vehicles")]
        public string Vehicles;

        [XmlElement("Audio", IsNullable = true)]
        public AudioSettings AudioSettings = new AudioSettings();

        [XmlArray("Modes")]
        [XmlArrayItem("Mode")]
        public List<Mode> Modes = new List<Mode>();

        [XmlArray("ControlGroups")]
        [XmlArrayItem("ControlGroup")]
        public List<ControlGroup> ControlGroups = new List<ControlGroup>();
    }

    public class AudioSettings
    {
        [XmlArray("AudioModes")]
        [XmlArrayItem("AudioMode")]
        public List<AudioMode> AudioModes = new List<AudioMode>();

        [XmlArray("AudioControlGroups")]
        [XmlArrayItem("AudioControlGroup")]
        public List<AudioControlGroup> AudioControlGroups = new List<AudioControlGroup>();
    }

    public class AudioMode
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("yield")]
        public bool Yield = true;

        [XmlElement("Sound")]
        public string Sound;
    }

    public class AudioControlGroup
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("cycle")]
        public string Cycle;

        [XmlAttribute("rev_cycle")]
        public string ReverseCycle;

        [XmlAttribute("toggle")]
        public string Toggle;

        [XmlAttribute("exclusive")]
        public bool Exclusive;

        [XmlArray("AudioModes")]
        [XmlArrayItem("AudioMode")]
        public List<AudioModeSelection> Modes;
    }

    public class AudioModeSelection
    {
        [XmlAttribute("toggle")]
        public string Toggle;

        [XmlAttribute("hold")]
        public string Hold;

        [XmlText]
        public string ModesRaw
        {
            get => modesRaw;
            set
            {
                Modes = value.Split(',').Select(s => s.Trim()).ToList();
                modesRaw = value;
            }
        }
        private string modesRaw;

        [XmlIgnore]
        public List<string> Modes;
    }

    public class Mode
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("apply_default_siren_settings")]
        public bool ApplyDefaultSirenSettings;

        [XmlElement("Yield", IsNullable = true)]
        public Yield Yield = new Yield();

        [XmlElement("Triggers")]
        public AnyCondition Triggers = new AnyCondition();

        [XmlElement("Requirements")]
        public AllCondition Requirements = new AllCondition();
        
        [XmlElement("Indicators", IsNullable = true)]
        public string Indicators;

        [XmlArray("Extras", IsNullable = true)]
        [XmlArrayItem("Extra")]
        public List<Extra> Extra = new List<Extra>();

        [XmlArray("ModKits", IsNullable = true)]
        [XmlArrayItem("Kit")]
        public List<ModKit> ModKits = new List<ModKit>();

        [XmlElement("SirenSettings", IsNullable = true)]
        public SirenSetting SirenSettings = new SirenSetting();

        [XmlArray("Sequences", IsNullable = true)]
        [XmlArrayItem("Item")]
        public SequenceItem[] Sequences
        {
            get => sequences;
            set
            {
                List<SirenEntry> sequenceSirens = new List<SirenEntry>();

                foreach (SequenceItem item in value)
                {
                    if (SirenSettings == null) SirenSettings = new SirenSetting();

                    switch (item.ID)
                    {
                        case "leftHeadLight":
                            SirenSettings.LeftHeadLightSequencer = new SequencerWrapper(item.Sequence);
                            continue;
                        case "rightHeadLight":
                            SirenSettings.RightHeadLightSequencer = new SequencerWrapper(item.Sequence);
                            continue;
                        case "leftTailLight":
                            SirenSettings.LeftTailLightSequencer = new SequencerWrapper(item.Sequence);
                            continue;
                        case "rightTailLight":
                            SirenSettings.RightTailLightSequencer = new SequencerWrapper(item.Sequence);
                            continue;
                    }

                    SirenEntry previousSiren = null;

                    if (SirenSettings.Sirens != null)
                        previousSiren = SirenSettings.Sirens.FirstOrDefault(x => x?.ID == int.Parse(item.ID));

                    if (previousSiren?.Flashiness != null)
                    {
                        previousSiren.Flashiness.Sequence = new Sequencer(item.Sequence);
                        sequenceSirens.Add(previousSiren);
                    }
                    else
                    {
                        sequenceSirens.Add(new SirenEntry
                        {
                            ID = int.Parse(item.ID),
                            Flashiness = new LightDetailEntry
                            {
                                Sequence = new Sequencer(item.Sequence)
                            }
                        });
                    }
                }

                SirenSettings.Sirens = sequenceSirens.ToArray();
                sequences = value;
            }
        }
        private SequenceItem[] sequences;

        public static Mode GetEmpty(Vehicle veh)
        {
            return new Mode()
            {
                Name = "Empty",
                Yield = new Yield()
                {
                    Enabled = false
                },
                Extra = new List<Extra>(),
                SirenSettings = new SirenSetting()
                {
                    TimeMultiplier = veh.DefaultEmergencyLighting.TimeMultiplier,
                    LightFalloffMax = veh.DefaultEmergencyLighting.LightFalloffMax,
                    LightFalloffExponent = veh.DefaultEmergencyLighting.LightFalloffExponent,
                    LightInnerConeAngle = veh.DefaultEmergencyLighting.LightInnerConeAngle,
                    LightOuterConeAngle = veh.DefaultEmergencyLighting.LightOuterConeAngle,
                    LightOffset = veh.DefaultEmergencyLighting.LightOffset,
                    TextureHash = veh.DefaultEmergencyLighting.TextureHash,
                    SequencerBPM = veh.DefaultEmergencyLighting.SequencerBpm,
                    UseRealLights = veh.DefaultEmergencyLighting.UseRealLights,
                    LeftHeadLightSequencer = new SequencerWrapper("00000000000000000000000000000000"),
                    LeftHeadLightMultiples = veh.DefaultEmergencyLighting.LeftHeadLightMultiples,
                    RightHeadLightSequencer = new SequencerWrapper("00000000000000000000000000000000"),
                    RightHeadLightMultiples = veh.DefaultEmergencyLighting.RightHeadLightMultiples,
                    LeftTailLightSequencer = new SequencerWrapper("00000000000000000000000000000000"),
                    LeftTailLightMultiples = veh.DefaultEmergencyLighting.LeftTailLightMultiples,
                    RightTailLightSequencer = new SequencerWrapper("00000000000000000000000000000000"),
                    RightTailLightMultiples = veh.DefaultEmergencyLighting.RightTailLightMultiples,
                    Sirens = Enumerable.Range(0, 32).Select(i => new SirenEntry
                    {
                        Flashiness = new LightDetailEntry
                        {
                            Sequence = new Sequencer("00000000000000000000000000000000"),
                            SyncToBPM = veh.DefaultEmergencyLighting.Lights[i].FlashinessSynchronizeToBpm,
                        }
                    }).ToArray()
                }
            };
        }

        public override string ToString() => Name;
    }

    public class Yield
    {
        [XmlAttribute("enabled")]
        public bool Enabled;
    }

    public class TriggerRaw
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlText]
        public string Argument;
    }

    public class Siren
    {
        [XmlAttribute("manual")]
        public string ManualEnabled;

        [XmlAttribute("full")]
        public string FullSirenEnabled;
    }

    public class Extra
    {
        [XmlAttribute("id")]
        public int ID;

        [XmlAttribute("enabled")]
        public bool Enabled;
    }

    public class ModKit
    {
        [XmlAttribute("type")]
        public ModKitType Type;

        [XmlAttribute("index")]
        public int Index;
    }

    public class SequenceItem
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("sequence")]
        public string Sequence;
    }

    public class ControlGroup
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("cycle")]
        public string Cycle;

        [XmlAttribute("rev_cycle")]
        public string ReverseCycle;

        [XmlAttribute("toggle")]
        public string Toggle;

        [XmlAttribute("exclusive")]
        public bool Exclusive;

        [XmlArray("Modes")]
        [XmlArrayItem("Mode")]
        public List<ModeSelection> Modes;
    }

    public class ModeSelection
    {
        [XmlAttribute("toggle")]
        public string Toggle;

        [XmlText]
        public string ModesRaw
        {
            get => string.Join(",", Modes);
            set
            {
                Modes = value.Split(',').Select(s => s.Trim()).ToList();
            }
        }

        [XmlIgnore]
        public List<string> Modes;
    }
}
