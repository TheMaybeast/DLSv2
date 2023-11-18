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
        [XmlAttribute("engine_on")]
        public bool EngineOn { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.IsEngineOn == EngineOn;
    }

    public class IndicatorLightsCondition : VehicleCondition
    {
        [XmlAttribute("status")]
        public VehicleIndicatorLightsStatus DesiredStatus { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.GetIndicatorStatus() == DesiredStatus;
    }

    public class DoorsCondition : VehicleCondition
    {
        [XmlAttribute("door")]
        public DoorList DoorIndex { get; set; }

        [XmlAttribute("state")]
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
        [XmlAttribute("units")]
        public SpeedUnits Units { get; set; } = SpeedUnits.mph;

        [XmlAttribute("inclusive")]
        public bool Inclusive { get; set; } = true;

        [XmlAttribute("abs")]
        public bool AbsValue { get; set; } = true;

        [XmlAttribute("round")]
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
        [XmlAttribute("min")]
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
        [XmlAttribute("max")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float MaxValue { get; set; }

    }

    public class LiveryCondition : VehicleCondition
    {
        [XmlAttribute("id")]
        public int LiveryId { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.GetLivery() == LiveryId;
    }

    public class TowingCondition : VehicleCondition
    {
        [XmlAttribute("attached")]
        public bool IsTowing { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.TowedVehicle.Exists() == IsTowing;
    }

    public class TrailerCondition : VehicleCondition
    {
        [XmlAttribute("attached")]
        public bool HasTrailer { get; set; } = true;

        [XmlAttribute("model")]
        public string TrailerModel { get; set; } = null;

        protected override bool Evaluate(ManagedVehicle veh)
        {
            //. If no model is specified, just return whether it has a trailer
            if (TrailerModel == null) return veh.Vehicle.HasTrailer == HasTrailer;

            // If a model is specified but there is no trailer present, return true if 
            // a specific trailer model is prohibited (Attached = false), 
            // and return false if a specific trailer model is desired (Attached = true)
            if (!veh.Vehicle.HasTrailer) return !HasTrailer;

            // If a model is specified and a trailer is attached, check that the model matches
            return veh.Vehicle.Trailer.Model == new Model(TrailerModel);
        }
    }

    public class ExtraCondition : VehicleCondition
    {
        [XmlAttribute("id")]
        public int ExtraID { get; set; }

        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.HasExtra(ExtraID) && (veh.Vehicle.IsExtraEnabled(ExtraID) == Enabled);
    }

    public class LightEmissiveCondition : VehicleCondition
    {
        [XmlAttribute("id")]
        public VehicleExtensions.LightID ID { get; set; }

        [XmlAttribute("status")]
        public bool Status { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.GetLightEmissiveStatus(ID) == Status;
    }

    public class BrakingCondition : VehicleCondition
    {
        [XmlAttribute("status")]
        public bool Status { get; set; }

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.GetBrakePressure(0) > 0 == Status;
    }
}
