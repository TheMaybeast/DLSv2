using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Linq;
using Rage;

namespace DLSv2.Conditions
{
    using Core;
    using Utils;

    public class DriverCondition : VehicleCondition
    {
        [XmlAttribute]
        public bool HasDriver { get; set; } = true;

        protected override bool Evaluate(ManagedVehicle veh) => veh.Vehicle.HasDriver == HasDriver;
    }

    public class SeatsCondition : VehicleCondition
    {
        [XmlAttribute("Occupied")]
        public bool SeatsOccupied { get; set; } = true;

        [XmlAttribute("All")]
        public bool AllSeats { get; set; } = true;

        [XmlText]
        public string Seats { get; set; } = "";

        [XmlIgnore]
        private List<int> seatIndices;

        [XmlIgnore]
        public int[] SeatIndices
        {
            get
            {
                if (seatIndices == null)
                {
                    seatIndices = new List<int>();
                    foreach (var seat in Seats.Split(','))
                    {
                        if (int.TryParse(seat.Trim(), out int index)) seatIndices.Add(index);
                    }
                }

                return seatIndices.ToArray();
            }

            set => seatIndices = value.ToList();
        }

        public SeatsCondition() : base()
        {
            List<int> indices = new List<int>();
            foreach (var seat in Seats.Split(','))
            {
                if (int.TryParse(seat.Trim(), out int index))
                {
                    indices.Add(index);
                }
            }            
        }

        protected override bool Evaluate(ManagedVehicle veh)
        {
            Vehicle v = veh.Vehicle;
            bool any = false;
            bool all = true;
            foreach (int seat in SeatIndices)
            {
                bool seatOK = (v.IsSeatFree(seat) != SeatsOccupied);
                any = any || seatOK;
                all = all && seatOK;
            }

            return AllSeats ? all : any;
        }
    }

    public class OccupantsCondition : VehicleMinMaxCondition
    {
        [XmlAttribute("Any")]
        public bool HasOccupants { get; set; } = true;

        [XmlIgnore]
        public bool? IsFull
        {
            get => FullValueSpecified ? FullValue : (bool?)null;
            set
            {
                FullValueSpecified = value.HasValue;
                if (value.HasValue) FullValue = value.Value;
                else FullValue = false;
            }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool FullValueSpecified { get; set; }
        [XmlAttribute("Full")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool FullValue { get; set; }

        protected virtual int GetNum(Vehicle v) => v.Occupants.Length;
        protected virtual int GetMaxSeats(Vehicle v) => v.PassengerCapacity + 1;
        protected virtual int GetSeatsLeft(Vehicle v) => v.FreeSeatsCount;

        protected override bool Evaluate(ManagedVehicle veh)
        {
            if (HasOccupants && !Min.HasValue && !Max.HasValue) Min = 1;

            int occs = GetNum(veh.Vehicle);
            bool ok = true;
            if (Min.HasValue) ok = ok && occs >= Min.Value;
            if (Max.HasValue) ok = ok && occs <= Max.Value;
            if (IsFull.HasValue) ok = ok && ((GetSeatsLeft(veh.Vehicle) == 0) == IsFull);
            
            return ok;
        }
    }

    public class PassengersCondition : OccupantsCondition
    {
        protected override int GetNum(Vehicle v) => v.PassengerCount;
        protected override int GetMaxSeats(Vehicle v) => v.PassengerCapacity;
        protected override int GetSeatsLeft(Vehicle v) => v.FreePassengerSeatsCount;
    }

    public class VehicleOwnerCondition : BaseCondition
    {
        public class LastDriverInstance : ConditionInstance
        {
            public LastDriverInstance(VehicleOwnerCondition condition) : base(condition) { }

            public Ped LastDriver { get; set; }
        }

        [XmlIgnore]
        protected static Dictionary<(ManagedVehicle veh, VehicleOwnerCondition cond), LastDriverInstance> instances = new Dictionary<(ManagedVehicle veh, VehicleOwnerCondition cond), LastDriverInstance>();

        [XmlAttribute]
        public bool IsPlayerVehicle { get; set; }

        public override ConditionInstance GetInstance(ManagedVehicle mv)
        {
            if (!instances.TryGetValue((mv, this), out var instance))
            {
                instance = new LastDriverInstance(this);
                instances.Add((mv, this), instance);
            }

            return instance;
        }

        protected override bool Evaluate(ManagedVehicle veh)
        {
            var instance = GetInstance(veh) as LastDriverInstance;
            if (veh.Vehicle.HasDriver)
            {
                instance.LastDriver = veh.Vehicle.Driver;
            } else if (Game.LocalPlayer.Character.LastVehicle == veh.Vehicle)
            {
                instance.LastDriver = Game.LocalPlayer.Character;
            }

            return instance.LastDriver == IsPlayerVehicle;
        }
    }
}
