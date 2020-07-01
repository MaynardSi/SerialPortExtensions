using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SickODValueHelper
{
    public interface IHeightSensorController : IController
    {
        double ReadHeight();
    }
}