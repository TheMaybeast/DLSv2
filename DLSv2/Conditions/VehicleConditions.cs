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

    public class SpeedCondition : VehicleCondition
    {
        [XmlIgnore]
        public float? MinSpeed
        {
            get => MinSpeedValueSpecified ? MinSpeedValue : (float?)null;
            set
            {
                MinSpeedValueSpecified = value.HasValue;
                if (value.HasValue) MinSpeedValue = value.Value;
                else MinSpeedValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MinSpeedValueSpecified { get; set; }
        [XmlAttribute("Min")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MinSpeedValue { get; set; }


        [XmlIgnore]
        public float? MaxSpeed
        {
            get => MaxSpeedValueSpecified ? MaxSpeedValue : (float?)null;
            set
            {
                MaxSpeedValueSpecified = value.HasValue;
                if (value.HasValue) MaxSpeedValue = value.Value;
                else MaxSpeedValue = 0;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MaxSpeedValueSpecified { get; set; }
        [XmlAttribute("Max")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MaxSpeedValue { get; set; }


        [XmlAttribute("Units")]
        public SpeedUnits Units { get; set; } = SpeedUnits.mph;

        [XmlAttribute("Inclusive")]
        public bool Inclusive { get; set; } = true;

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
            float speed = ConvertToSpecifiedUnits(veh.Vehicle.Speed);
            
            bool ok = true;
            if (MinSpeed.HasValue) ok = ok && (Inclusive ? speed >= MinSpeed.Value : speed > MinSpeed.Value);
            if (MaxSpeed.HasValue) ok = ok && (Inclusive ? speed <= MaxSpeed.Value : speed < MaxSpeed.Value);
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
}
