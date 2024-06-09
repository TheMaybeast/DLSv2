using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq;

namespace DLSv2.Core;

using Utils;

[XmlRoot("Model")]
public class DLSModel
{
    [XmlAttribute("vehicles")]
    public string Vehicles;

    [XmlElement("Audio", IsNullable = true)]
    public AudioSettings AudioSettings = new();

    [XmlElement("PatternSync")]
    public string SyncGroup;

    [XmlElement("SpeedDrift")]
    public float DriftRange = 0f;

    [XmlElement("DefaultMode")]
    public string DefaultModeName;

    [XmlIgnore]
    public LightMode DefaultMode;

    [XmlArray("Modes")]
    [XmlArrayItem("Mode")]
    public List<LightMode> Modes = new();

    [XmlArray("ControlGroups")]
    [XmlArrayItem("ControlGroup")]
    public List<LightControlGroup> ControlGroups = new();
}

public class AudioSettings
{
    [XmlArray("AudioModes")]
    [XmlArrayItem("AudioMode")]
    public List<AudioMode> AudioModes = new();

    [XmlArray("AudioControlGroups")]
    [XmlArrayItem("AudioControlGroup")]
    public List<AudioControlGroup> AudioControlGroups = new();
}

public class AudioMode : BaseMode
{
    [XmlElement("Sound")]
    public Sound Sound;
    
    [XmlElement("Triggers")]
    public AnyCondition Triggers = new();

    [XmlElement("Requirements")]
    public AllCondition Requirements = new();
}

public class Sound
{
    [XmlAttribute("soundbank")] public string SoundBank = null;
    [XmlAttribute("soundset")] public string SoundSet = null;
    [XmlText] public string ScriptName;
}

public class AudioControlGroup : BaseControlGroup<AudioModeSelection>
{
    [XmlArray("AudioModes")]
    [XmlArrayItem("AudioMode")]
    public override List<AudioModeSelection> Modes { get; set; } = new();
}

public class AudioModeSelection : BaseModeSelection
{
    [XmlAttribute("hold")]
    public string Hold;
}

public class LightMode : BaseMode
{
    [XmlAttribute("apply_default_siren_settings")]
    public bool ApplyDefaultSirenSettings;

    [XmlElement("Triggers")]
    public AnyCondition Triggers = new();

    [XmlElement("Requirements")]
    public AllCondition Requirements = new();
        
    [XmlElement("Indicators", IsNullable = true)]
    public string Indicators;

    [XmlArray("Extras", IsNullable = true)]
    [XmlArrayItem("Extra")]
    public List<Extra> Extra = new();

    [XmlArray("ModKits", IsNullable = true)]
    [XmlArrayItem("Kit")]
    public List<ModKit> ModKits = new();

    [XmlElement("Animation", IsNullable = true)]
    public Animation Animation;
        
    [XmlArray("Paints", IsNullable = true)]
    [XmlArrayItem("Paint")]
    public List<PaintJob> PaintJobs = new();

    [XmlElement("SirenSettings", IsNullable = true)]
    public SirenSetting SirenSettings = new();

    [XmlArray("Sequences", IsNullable = true)]
    [XmlArrayItem("Item")]
    public SequenceItem[] Sequences
    {
        get => sequences;
        set
        {
            foreach (SequenceItem item in value)
            {
                if (SirenSettings == null) SirenSettings = new SirenSetting();

                // Parse siren ID into one or more integers
                foreach (string id in item.IDs.Split(','))
                {
                    // If siren ID string is a head/tail light sequencer, set and continue to next item 
                    switch (id)
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

                    if (int.TryParse(id.Trim(), out int ID))
                    {
                        SirenEntry siren = new SirenEntry(ID) { Flashiness = new LightDetailEntry { Sequence = new Sequencer(item.Sequence) } };
                        SirenSettings.SirenList.Add(siren);
                    } else
                    {
                        $"Mode {Name} siren id {id} is invalid".ToLog(LogLevel.ERROR);
                    }
                }
            }

            sequences = value;
        }
    }
    private SequenceItem[] sequences;

    public override string ToString() => Name;
}

public class Yield
{
    [XmlAttribute("enabled")]
    public bool Enabled;
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

public class Animation
{
    [XmlElement("Dict")] public string AnimDict;

    [XmlElement("Name")] public string AnimName;

    [XmlAttribute("blend")] public float BlendDelta = 4.0f;

    [XmlAttribute("loop")] public bool Loop = true;

    [XmlAttribute("stay_in_last_frame")] public bool StayInLastFrame = true;

    [XmlAttribute("start_at")] public float StartPhase = 0f;

    [XmlAttribute("flags")] public int Flags = 0;


    [XmlIgnore]
    public float? Speed
    {
        get => SpeedValueSpecified ? SpeedValue : (float?)null;
        set
        {
            SpeedValueSpecified = value.HasValue;
            if (value.HasValue) SpeedValue = value.Value;
            else SpeedValue = 0;
        }
    }

    [XmlIgnore]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool SpeedValueSpecified { get; set; }

    [XmlAttribute("speed")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public float SpeedValue { get; set; }
}

public class PaintJob
{
    [XmlAttribute("slot")]
    public int PaintSlot;

    [XmlAttribute("color")]
    public int ColorCode;
}

public class SequenceItem
{
    [XmlAttribute("id")]
    public string IDs;

    [XmlAttribute("sequence")]
    public string Sequence;
}

public class LightControlGroup : BaseControlGroup<LightModeSelection>
{
    [XmlArray("Modes")]
    [XmlArrayItem("Mode")]
    public override List<LightModeSelection> Modes { get; set; } = new();
}

public class LightModeSelection : BaseModeSelection { }

public abstract class BaseMode
{
    [XmlAttribute("name")]
    public string Name;
        
    [XmlElement("Yield")]
    public Yield Yield;
}

public abstract class BaseModeSelection
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

public abstract class BaseControlGroup<T> where T : BaseModeSelection
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
    public bool Exclusive = true;

    [XmlIgnore]
    public abstract List<T> Modes { get; set; }
}