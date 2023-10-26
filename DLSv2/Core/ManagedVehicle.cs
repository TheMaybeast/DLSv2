using Rage;

namespace DLSv2
{
    public class ManagedVehicle
    {
        public ManagedVehicle(Vehicle vehicle, bool lightsOn = false)
        {
            Vehicle = vehicle;
            LightsOn = lightsOn;
            SirenStage = SirenStage.Off;

            if (Entrypoint.SirenSets.ContainsKey(vehicle.Model.Name.ToLower()))
                SoundSet = Entrypoint.SirenSets[vehicle.Model.Name.ToLower()];
            else
                SoundSet = null;

            if (vehicle)
            {
                bool temp = vehicle.IsSirenOn;
                vehicle.IsSirenOn = false;
                vehicle.IsSirenOn = temp;
            }
        }

        // General
        public Vehicle Vehicle { get; set; }
        

        // Lights
        public bool LightsOn { get; set; } = false;
        public bool Blackout { get; set; } = false;
        public bool InteriorLight { get; set; } = false;
        public IndStatus IndStatus { get; set; } = IndStatus.Off;

        // Sirens
        public SoundSet SoundSet { get; set; }
        public SirenStage SirenStage { get; set; }
        public bool AuxOn { get; set; } = false;
        public int AuxID { get; set; } = 999;
        public int SoundId { get; set; } = 999;
        public int? AirManuState { get; set; } = null;
        public int? AirManuID { get; set; } = null;
    }

    public enum SirenStage
    {
        Horn = -1,
        Off,
        One,
        Two,
        Warning,
        Warning2        
    }

    public enum IndStatus
    {
        Left,
        Right,
        Both,
        Off
    }
}
