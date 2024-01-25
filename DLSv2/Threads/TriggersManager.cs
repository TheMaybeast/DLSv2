using Rage;

namespace DLSv2.Threads;

using Core;

internal class TriggersManager
{
    public static void Process()
    {
        while (true)
        {
            foreach (ManagedVehicle mv in Entrypoint.ManagedVehicles.Values)
            {
                if (mv.Vehicle) foreach (BaseCondition condition in mv.Conditions) condition.Update(mv);
            }
            GameFiber.Yield();
        }
    }
}