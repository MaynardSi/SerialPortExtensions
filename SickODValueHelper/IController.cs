using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SickODValueHelper
{
    public interface IController
    {
        bool Startup();

        bool Shutdown();

        bool Reset();
    }
}