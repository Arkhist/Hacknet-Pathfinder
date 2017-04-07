using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinder
{
    public class PathfinderDaemon : Hacknet.Daemon
    {
        PathfinderDaemon(Hacknet.Computer computer, string serviceName, Hacknet.OS os) : base(computer, serviceName, os)
        {
            
        }
    }
}
