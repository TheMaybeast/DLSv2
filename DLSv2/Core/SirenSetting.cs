using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;
using Rage;

namespace DLSv2.Core;

public class SirenSetting
{
    [XmlElement("timeMultiplier", IsNullable = true)]
    public ValueItem<float> TimeMultiplier { get; set; }

    [XmlElement("lightFalloffMax", IsNullable = true)]
    public ValueItem<float> LightFalloffMax { get; set; }

    [XmlElement("lightFalloffExponent", IsNullable = true)]
    public ValueItem<float> LightFalloffExponent { get; set; }

    [XmlElement("lightInnerConeAngle", IsNullable = true)]
    public ValueItem<float> LightInnerConeAngle { get; set; }

    [XmlElement("lightOuterConeAngle", IsNullable = true)]
    public ValueItem<float> LightOuterConeAngle { get; set; }

    [XmlElement("lightOffset", IsNullable = true)]
    public ValueItem<float> LightOffset { get; set; }

    [XmlElement("textureName", IsNullable = true)]
    public string TextureName { get; set; }

    [XmlIgnore]
    public uint? TextureHash
    {
        get => TextureName != null ? Core.TextureHash.StringToHash(TextureName) : (uint?)null;

        set => TextureName = value.HasValue ? Core.TextureHash.HashToString(value.Value) : null;
    }

    [XmlElement("sequencerBpm", IsNullable = true)]
    public ValueItem<uint> SequencerBPM { get; set; }

    [XmlElement("leftHeadLight", IsNullable = true)]
    public SequencerWrapper LeftHeadLightSequencer { get; set; }

    [XmlElement("rightHeadLight", IsNullable = true)]
    public SequencerWrapper RightHeadLightSequencer { get; set; }

    [XmlElement("leftTailLight", IsNullable = true)]
    public SequencerWrapper LeftTailLightSequencer { get; set; }

    [XmlElement("rightTailLight", IsNullable = true)]
    public SequencerWrapper RightTailLightSequencer { get; set; }

    [XmlElement("leftHeadLightMultiples", IsNullable = true)]
    public ValueItem<byte> LeftHeadLightMultiples { get; set; }

    [XmlElement("rightHeadLightMultiples", IsNullable = true)]
    public ValueItem<byte> RightHeadLightMultiples { get; set; }

    [XmlElement("leftTailLightMultiples", IsNullable = true)]
    public ValueItem<byte> LeftTailLightMultiples { get; set; }

    [XmlElement("rightTailLightMultiples", IsNullable = true)]
    public ValueItem<byte> RightTailLightMultiples { get; set; }

    [XmlElement("useRealLights", IsNullable = true)]
    public ValueItem<bool> UseRealLights { get; set; }


    [XmlArray("sirens", IsNullable = true)]
    [XmlArrayItem("Item")]
    // public List<SirenEntry> Sirens
    public SirenEntry[] Sirens
    {
        get => SirenList.ToArray();

        set
        {
            for (int i = 0; i < value.Length; i++)
            {
                SirenEntry entry = value[i];
                if (entry.sirenIDs == null || entry.sirenIDs.Length == 0)
                {
                    entry.sirenIDs = new int[] { i + 1 };
                }
            }
            SirenList = value.ToList();
        }
    }

    [XmlIgnore]
    public List<SirenEntry> SirenList = new List<SirenEntry>();
}

public class SirenEntry
{
    public SirenEntry() { }

    public SirenEntry(params int[] IDs) { sirenIDs = IDs; }

    [XmlAttribute("id")]
    public string IDs
    {
        get => string.Join(",", sirenIDs);

        set
        {
            if (value == "all")
                sirenIDs = Enumerable.Range(1, EmergencyLighting.MaxLights).ToArray();

            else
                sirenIDs = value.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        }
    }

    [XmlIgnore]
    public int[] sirenIDs { get; set; } = new int[] { };

    [XmlElement("rotation", IsNullable = true)]
    public LightDetailEntry Rotation { get; set; } = new LightDetailEntry();

    [XmlElement("flashiness", IsNullable = true)]
    public LightDetailEntry Flashiness { get; set; } = new LightDetailEntry();

    [XmlElement("corona", IsNullable = true)]
    public CoronaEntry Corona { get; set; } = new CoronaEntry();

    [XmlIgnore]
    public Color? LightColor { get; set; }

    [XmlElement("color", IsNullable = true)]
    public ValueItem<string> ColorString
    {
        get => LightColor != null ? $"0x{LightColor?.ToArgb():X8}" : null;
        set => LightColor = Color.FromArgb(Convert.ToInt32(value, 16));
    }

    [XmlElement("intensity", IsNullable = true)]
    public ValueItem<float> Intensity { get; set; }

    [XmlElement("lightGroup", IsNullable = true)]
    public ValueItem<byte> LightGroup { get; set; }

    [XmlElement("rotate", IsNullable = true)]
    public ValueItem<bool> Rotate { get; set; }

    [XmlElement("scale", IsNullable = true)]
    public ValueItem<bool> Scale { get; set; }

    [XmlElement("scaleFactor", IsNullable = true)]
    public ValueItem<byte> ScaleFactor { get; set; }

    [XmlElement("flash", IsNullable = true)]
    public ValueItem<bool> Flash { get; set; }

    [XmlElement("light", IsNullable = true)]
    public ValueItem<bool> Light { get; set; }

    [XmlElement("spotLight", IsNullable = true)]
    public ValueItem<bool> SpotLight { get; set; }

    [XmlElement("castShadows", IsNullable = true)]
    public ValueItem<bool> CastShadows { get; set; }
}

public class CoronaEntry
{
    [XmlElement("intensity", IsNullable = true)]
    public ValueItem<float> CoronaIntensity { get; set; }

    [XmlElement("size", IsNullable = true)]
    public ValueItem<float> CoronaSize { get; set; }

    [XmlElement("pull", IsNullable = true)]
    public ValueItem<float> CoronaPull { get; set; }

    [XmlElement("faceCamera", IsNullable = true)]
    public ValueItem<bool> CoronaFaceCamera { get; set; }
}

public class LightDetailEntry
{
    [XmlElement("delta", IsNullable = true)]
    public ValueItem<float> DeltaRad { get; set; }

    [XmlIgnore]
    public float? DeltaDeg
    {
        get => DeltaRad == null ? (float?)null : Rage.MathHelper.ConvertRadiansToDegrees(DeltaRad);
        set => DeltaRad = value.HasValue ? (float?)null : Rage.MathHelper.ConvertDegreesToRadians(value.Value);
    }

    [XmlElement("start", IsNullable = true)]
    public ValueItem<float> StartRad { get; set; }

    [XmlIgnore]
    public float? StartDeg
    {
        get => StartRad == null ? (float?) null : Rage.MathHelper.ConvertRadiansToDegrees(StartRad);
        set => StartRad = value.HasValue ? (float?)null : Rage.MathHelper.ConvertDegreesToRadians(value.Value);
    }

    [XmlElement("speed", IsNullable = true)]
    public ValueItem<float> Speed { get; set; }

    [XmlElement("sequencer", IsNullable = true)]
    public Sequencer Sequence { get; set; }

    [XmlElement("multiples", IsNullable = true)]
    public ValueItem<byte> Multiples { get; set; }

    [XmlElement("direction", IsNullable = true)]
    public ValueItem<bool> Direction { get; set; }

    [XmlElement("syncToBpm", IsNullable = true)]
    public ValueItem<bool> SyncToBPM { get; set; }

}

[DebuggerDisplay("{Sequence} = {SequenceRaw}")]
public class Sequencer : ValueItem<uint>
{
    public static implicit operator Sequencer(uint value) => new Sequencer(value);
    public static implicit operator Sequencer(string value) => new Sequencer(value);
    public static implicit operator uint(Sequencer item) => item.Value;
    public static implicit operator string(Sequencer item) => Convert.ToString(item.Value, 2);

    public Sequencer(uint value) : base(value) { }
    public Sequencer(string value) : base(Convert.ToUInt32(value, 2)) { }

    public Sequencer() : base() { }

    [XmlIgnore]
    public uint SequenceRaw
    {
        get => Value;
        set => Value = value;
    }

    [XmlIgnore]
    public string Sequence
    {
        get => Convert.ToString(Value, 2);
        set => Value = Convert.ToUInt32(value, 2);
    }
}

[DebuggerDisplay("{Sequencer.Sequence} = {Sequencer.SequenceRaw}")]
public class SequencerWrapper
{
    public static implicit operator SequencerWrapper(uint value) => new SequencerWrapper(value);
    public static implicit operator SequencerWrapper(string value) => new SequencerWrapper(value);
    public static implicit operator uint(SequencerWrapper item) => item.Sequencer.SequenceRaw;
    public static implicit operator string(SequencerWrapper item) => item.Sequencer.Sequence;

    public SequencerWrapper(uint value)
    {
        Sequencer = value;
    }

    public SequencerWrapper(string value)
    {
        Sequencer = value;
    }

    public SequencerWrapper()
    {
        Sequencer = new Sequencer();
    }

    [XmlElement("sequencer")]
    public Sequencer Sequencer { get; set; }
}

[DebuggerDisplay("{Value}")]
public class ValueItem<T>
{
    public static implicit operator ValueItem<T>(T value) => new ValueItem<T>(value);
    public static implicit operator T(ValueItem<T> item) => item.Value;

    public ValueItem() { }

    public ValueItem(T value)
    {
        Value = value;
    }

    [XmlAttribute("value")]
    public virtual T Value { get; set; }
}