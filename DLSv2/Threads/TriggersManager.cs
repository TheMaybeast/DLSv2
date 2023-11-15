using DLSv2.Core;
using Rage;

namespace DLSv2.Threads
{
    internal class TriggersManager
    {
        public static void Process()
        {
            while (true)
            {
                foreach (ManagedVehicle mv in Entrypoint.ManagedVehicles)
                {
                    if (mv.Vehicle) foreach (BaseCondition condition in mv.Conditions) condition.Update(mv);
                }
                GameFiber.Yield();
            }
        }
    }   
}