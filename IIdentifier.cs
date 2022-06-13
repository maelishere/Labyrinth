using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinth
{
    public interface IIdentifier<T>
    {
        T Value { get; }
    }
}
