using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peeky_Blinkers.Interface
{
    public interface IWindowRegistry : IDisposable
    {
        object GetValue(string valueName, object defaultValue);
        void SetValue(string valueName, object value);
    }
}
