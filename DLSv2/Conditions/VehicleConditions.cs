using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Rage;

namespace DLSv2.Conditions
{
    using Core;
    using Utils;

    public class EngineStateCondition : VehicleCondition
    {
        [XmlAttribute]
        public bool EngineOn { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.IsEngineOn == EngineOn;
    }

    public class IndicatorLightsCondition : VehicleCondition
    {
        [XmlAttribute("Status")]
        public VehicleIndicatorLightsStatus DesiredStatus { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.GetIndicatorStatus() == DesiredStatus;
    }

    public class DoorsCondition : VehicleCondition
    {
        [XmlAttribute("Door")]
        public DoorList DoorIndex { get; set; }

        [XmlAttribute("State")]
        public DoorState State { get; set; } = DoorState.Open;

        protected override bool Evaluate(ManagedVehicle veh)
        {
            var door = veh.Vehicle.Doors[(int)DoorIndex];
            switch (State)
            {
                case DoorState.FullyOpen:
                    return door.IsFullyOpen;
                case DoorState.Closed:
                    return !door.IsOpen;
                case DoorState.Damaged:
                    return door.IsDamaged;
                case DoorState.Open:
                default:
                    return door.IsOpen;
            }
        }

        public enum DoorState
        {
            Open,
            FullyOpen,
            Closed,
            Damaged
        }

        public enum DoorList
        {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 2,
            RearRight = 3,
            Hood = 4,
            Trunk = 5,
            Bonnet = Hood,
            Boot = Trunk
        }
    }

    public class SpeedCondition : VehicleMinMaxCondition
    {
        [XmlAttribute("Units")]
        public SpeedUnits Units { get; set; } = SpeedUnits.mph;

        [XmlAttribute("Inclusive")]
        public bool Inclusive { get; set; } = true;

        [XmlAttribute("Abs")]
        public bool AbsValue { get; set; } = true;

        [XmlAttribute("Round")]
        public int RoundToDecimalPlaces { get; set; } = 2;

        public float ConvertToSpecifiedUnits(float speed)
        {
            float ratio = 1.0f;
            switch (Units)
            {
                    
                case SpeedUnits.mph:
                    ratio = 2.237f;
                    break;
                case SpeedUnits.kmh:
                    ratio = 0.6214f;
                    break;
                case SpeedUnits.ftps:
                    ratio = 0.6818f;
                    break;
                default:
                case SpeedUnits.mps:
                    break;
            }

            // speed [m/s] * ratio [units]/[m/s]
            return speed * ratio;
        }

        protected override bool Evaluate(ManagedVehicle veh)
        {
            float speed = ConvertToSpecifiedUnits(veh.Vehicle.GetForwardSpeed());
            speed = (float)Math.Round(AbsValue ? Math.Abs(speed) : speed, RoundToDecimalPlaces);
                        
            bool ok = true;
            if (Min.HasValue) ok = ok && (Inclusive ? speed >= Min.Value : speed > Min.Value);
            if (Max.HasValue) ok = ok && (Inclusive ? speed <= Max.Value : speed < Max.Value);
            return ok;
        }

        public enum SpeedUnits
        {
            mps,
            mph,
            kmh,
            ftps,
        }
    }

    public class AccelerationCondition : VehicleCondition<AccelerationCondition.AccelInstance>
    {
        protected override bool Evaluate(ManagedVehicle veh)
        {
            var instance = GetInstance(veh) as AccelInstance;
            float speed = Math.Abs(veh.Vehicle.GetForwardSpeed());
            float time = Game.GameTime;

            float accel = (speed - instance.LastSpeed) / (time - instance.LastTime) * 1000;
            instance.LastSpeed = speed;
            instance.LastTime = Game.GameTime;

            bool ok = true;
            if (Min.HasValue) ok = ok && accel > Min.Value;
            if (Max.HasValue) ok = ok && accel < Max.Value;
            return ok;
        }

        public class AccelInstance : ConditionInstance
        {
            public AccelInstance(AccelerationCondition condition) : base(condition) { }
            public AccelInstance() : base() { }

            public float LastSpeed;
            public uint LastTime;
        }

        [XmlIgnore]
        public float? Min
        {
            get => MinValueSpecified ? MinValue : (float?)null;
            set
            {
                MinValueSpecified = value.HasValue;
                if (value.HasValue) MinValue = value.Value;
                else MinValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MinValueSpecified { get; set; }
        [XmlAttribute("Min")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MinValue { get; set; }


        [XmlIgnore]
        public float? Max
        {
            get => MaxValueSpecified ? MaxValue : (float?)null;
            set
            {
                MaxValueSpecified = value.HasValue;
                if (value.HasValue) MaxValue = value.Value;
                else MaxValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MaxValueSpecified { get; set; }
        [XmlAttribute("Max")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MaxValue { get; set; }

    }
}
