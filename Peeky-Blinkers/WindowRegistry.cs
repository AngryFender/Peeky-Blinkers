using Microsoft.Win32;
using Peeky_Blinkers.Interface;
using System;

namespace Peeky_Blinkers
{
    internal class WindowRegistry : IWindowRegistry, IDisposable
    {
        private RegistryKey _softwareKey;
        private RegistryKey _appKey;
        public WindowRegistry() 
        {
            _softwareKey= Registry.CurrentUser.OpenSubKey("Software", true);
            _appKey= _softwareKey.CreateSubKey("Peeky-Blinkers", true);
        }

        public void SetValue( string valueName, object value)
        {
            _appKey.SetValue(valueName, value);
        }

        public object GetValue(string valueName, object defaultValue)
        {
            return _appKey.GetValue(valueName, defaultValue);
        }

        public void Dispose()
        {
            _appKey.Close();
            _softwareKey.Close();
        }
    }
}
