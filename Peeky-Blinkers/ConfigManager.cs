using Microsoft.Win32;
using Peeky_Blinkers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peeky_Blinkers
{
    public class ConfigManager: IDisposable 
    {
        private readonly IWindowRegistry _registry;
        private bool _disposed = false;

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

        internal int GetAnimationFrameCount()
        {
            object defaultValue = 3;
            object obj = _registry.GetValue("animation_frame_count", defaultValue);
            if(obj == null)
            {
                return 3;
            }

            if (obj is int result)
            {
                return result;
            }
            else if (obj is string resultStr)
            {
                return int.Parse(resultStr);
            }
            else
            {
                return 3;
            }
        }

        internal void SetAnimationState(bool value)
        {
            _registry.SetValue("animation_enabled", value);
        }

        internal void setAnimationFrameCount(int frameCount) 
        {
            _registry.SetValue("animation_frame_count", frameCount);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _registry.Dispose();
                }
                // no unmanaged memory to free here
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
