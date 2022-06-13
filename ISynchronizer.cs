using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinth
{
    using Bolt;

    public interface ISynchronizer<T> : IIdentifier<T>
    {
        Write Sending { get; }
        Read Recieving { get; }
    }
}
