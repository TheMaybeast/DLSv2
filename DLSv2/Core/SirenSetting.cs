using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace DLSv2.Core
{
    public class SirenSetting
    {
        [XmlElement("timeMultiplier", IsNullable = true)]
        public ValueItem<float> TimeMultiplier { get; set; } = null;

        [XmlElement("lightFalloffMax", IsNullable = true)]
        public ValueItem<float> LightFalloffMax { get; set; } = null;

        [XmlElement("lightFalloffExponent", IsNullable = true)]
        public ValueItem<float> LightFalloffExponent { get; set; } = null;

        [XmlElement("lightInnerConeAngle", IsNullable = true)]
        public ValueItem<float> LightInnerConeAngle { get; set; } = null;

        [XmlElement("lightOuterConeAngle", IsNullable = true)]
        public ValueItem<float> LightOuterConeAngle { get; set; } = null;

        [XmlElement("lightOffset", IsNullable = true)]
        public ValueItem<float> LightOffset { get; set; } = null;

        [XmlElement("textureName", IsNullable = true)]
        public string TextureName { get; set; } = null;

        [XmlIgnore]
        public uint? TextureHash
        {
            get => TextureName != null ? Core.TextureHash.StringToHash(TextureName) : (uint?)null;

            set => TextureName = value.HasValue ? Core.TextureHash.HashToString(value.Value) : null;
        }

        [XmlElement("sequencerBpm", IsNullable = true)]
        public ValueItem<uint> SequencerBPM { get; set; } = null;

        [XmlElement("leftHeadLight", IsNullable = true)]
        public SequencerWrapper LeftHeadLightSequencer { get; set; } = null;

        [XmlElement("rightHeadLight", IsNullable = true)]
        public SequencerWrapper RightHeadLightSequencer { get; set; } = null;

        [XmlElement("leftTailLight", IsNullable = true)]
        public SequencerWrapper LeftTailLightSequencer { get; set; } = null;

        [XmlElement("rightTailLight", IsNullable = true)]
        public SequencerWrapper RightTailLightSequencer { get; set; } = null;

        [XmlElement("leftHeadLightMultiples", IsNullable = true)]
        public ValueItem<byte> LeftHeadLightMultiples { get; set; } = null;

        [XmlElement("rightHeadLightMultiples", IsNullable = true)]
        public ValueItem<byte> RightHeadLightMultiples { get; set; } = null;

        [XmlElement("leftTailLightMultiples", IsNullable = true)]
        public ValueItem<byte> LeftTailLightMultiples { get; set; } = null;

        [XmlElement("rightTailLightMultiples", IsNullable = true)]
        public ValueItem<byte> RightTailLightMultiples { get; set; } = null;

        [XmlElement("useRealLights", IsNullable = true)]
        public ValueItem<bool> UseRealLights { get; set; } = null;


        [XmlArray("sirens", IsNullable = true)]
        [XmlArrayItem("Item")]
        public SirenEntry[] Sirens
        {
            get => sirenList.ToArray();
            set
            {
                for (int i = 0; i < value.Count(); i++)
                {
                    SirenEntry siren = value[i];
                    int sirenID = siren.ID;
                    if (sirenID == 0)
                        siren.ID = (i + 1);
                    sirenList[siren.ID - 1] = siren;
                }
            }
        }

        [XmlIgnore]
        private SirenEntry[] sirenList = new SirenEntry[32];
    }

    public class SirenEntry
    {
        [XmlAttribute("ID")]
        public int ID { get; set; } = 0;

        [XmlElement("rotation", IsNullable = true)]
        public LightDetailEntry Rotation { get; set; } = new LightDetailEntry();

        [XmlElement("flashiness", IsNullable = true)]
        public LightDetailEntry Flashiness { get; set; } = new LightDetailEntry();

        [XmlElement("corona", IsNullable = true)]
        public CoronaEntry Corona { get; set; } = new CoronaEntry();

        [XmlIgnore]
        public Color? LightColor { get; set; } = null;

        [XmlElement("color", IsNullable = true)]
        public ValueItem<string> ColorString
        {
            get => LightColor != null ? string.Format("0x{0:X8}", LightColor?.ToArgb()) : null;
            set
            {
                LightColor = Color.FromArgb(Convert.ToInt32(value, 16));
            }
        }

        [XmlElement("intensity", IsNullable = true)]
        public ValueItem<float> Intensity { get; set; } = null;

        [XmlElement("lightGroup", IsNullable = true)]
        public ValueItem<byte> LightGroup { get; set; } = null;

        [XmlElement("rotate", IsNullable = true)]
        public ValueItem<bool> Rotate { get; set; } = null;

        [XmlElement("scale", IsNullable = true)]
        public ValueItem<bool> Scale { get; set; } = null;

        [XmlElement("scaleFactor", IsNullable = true)]
        public ValueItem<byte> ScaleFactor { get; set; } = null;

        [XmlElement("flash", IsNullable = true)]
        public ValueItem<bool> Flash { get; set; } = null;

        [XmlElement("light", IsNullable = true)]
        public ValueItem<bool> Light { get; set; } = null;

        [XmlElement("spotLight", IsNullable = true)]
        public ValueItem<bool> SpotLight { get; set; } = null;

        [XmlElement("castShadows", IsNullable = true)]
        public ValueItem<bool> CastShadows { get; set; } = null;
    }

    public class CoronaEntry
    {
        [XmlElement("intensity", IsNullable = true)]
        public ValueItem<float> CoronaIntensity { get; set; } = null;

        [XmlElement("size", IsNullable = true)]
        public ValueItem<float> CoronaSize { get; set; } = null;

        [XmlElement("pull", IsNullable = true)]
        public ValueItem<float> CoronaPull { get; set; } = null;

        [XmlElement("faceCamera", IsNullable = true)]
        public ValueItem<bool> CoronaFaceCamera { get; set; } = null;
    }

    public class LightDetailEntry
    {
        [XmlElement("delta", IsNullable = true)]
        public ValueItem<float> DeltaRad { get; set; } = null;

        [XmlIgnore]
        public float? DeltaDeg
        {
            get => DeltaRad == null ? (float?)null : Rage.MathHelper.ConvertRadiansToDegrees(DeltaRad);
            set => DeltaRad = value.HasValue ? (float?)null : Rage.MathHelper.ConvertDegreesToRadians(value.Value);
        }

        [XmlElement("start", IsNullable = true)]
        public ValueItem<float> StartRad { get; set; } = null;

        [XmlIgnore]
        public float? StartDeg
        {
            get => StartRad == null ? (float?) null : Rage.MathHelper.ConvertRadiansToDegrees(StartRad);
            set => StartRad = value.HasValue ? (float?)null : Rage.MathHelper.ConvertDegreesToRadians(value.Value);
        }

        [XmlElement("speed", IsNullable = true)]
        public ValueItem<float> Speed { get; set; } = null;

        [XmlElement("sequencer", IsNullable = true)]
        public Sequencer Sequence { get; set; } = null;

        [XmlElement("multiples", IsNullable = true)]
        public ValueItem<byte> Multiples { get; set; } = null;

        [XmlElement("direction", IsNullable = true)]
        public ValueItem<bool> Direction { get; set; } = null;

        [XmlElement("syncToBpm", IsNullable = true)]
        public ValueItem<bool> SyncToBPM { get; set; } = null;

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
            this.Value = value;
        }

        [XmlAttribute("value")]
        public virtual T Value { get; set; }
    }
}