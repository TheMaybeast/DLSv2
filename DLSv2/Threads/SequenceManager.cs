using DLSv2.Core;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSv2.Threads
{
    internal class SequenceManager
    {
        internal static void Process()
        {
            while (true)
            {
                foreach (ManagedVehicle mv in Entrypoint.ManagedVehicles.Values)
                {
                    if (mv.Vehicle) mv.ProcessSequences();
                }
                GameFiber.Yield();
            }
        }
    }
}
