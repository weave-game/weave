using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace weave.InputHandlers;

public interface IController
{
    bool IsTurningLeft();
    bool IsTurningRight();
}
