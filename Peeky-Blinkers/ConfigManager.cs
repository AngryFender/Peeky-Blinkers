using Microsoft.Win32;
using Peeky_Blinkers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peeky_Blinkers
{
    internal class ConfigManager
    {
        private readonly IWindowRegistry _registry;

        internal ConfigManager(IWindowRegistry registry) => _registry = registry;

        internal bool GetAnimationState()
        {
            object defaultValue = false;
            object obj = _registry.GetValue("animation_enabled", defaultValue);
            if (obj == null)
            {
                return false;
            }

            if(obj is bool result)
            {
                return result;
            } 
            else if (obj is string resultStr)
            {
                return bool.Parse(resultStr);
            }
            else
            {
                return false;
            }
        }

        internal void SetAnimationState(bool value)
        {
            _registry.SetValue("animation_enabled", value);
        }
    }
}
