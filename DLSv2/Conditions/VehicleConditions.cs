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
}
