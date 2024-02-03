using Microsoft.Win32;
using Peeky_Blinkers.Interface;
using System;

namespace Peeky_Blinkers
{
    internal class WindowRegistry : IWindowRegistry
    {
        private RegistryKey _softwareKey;
        private RegistryKey _appKey;
        private bool _disposed = false;

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _appKey.Close();
                _softwareKey.Close();
                _disposed = true;
            }
        }
    }
}
