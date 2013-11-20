// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;
using Microsoft.Win32;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.TestSupport
{
    public class ConfigurationHelper
    {
        public static string GetSetting(string settingName)
        {
            string value;
            using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\EntLib") ?? Registry.CurrentUser)
            {
                value = (string)subKey.GetValue(settingName);
            }
            if (string.IsNullOrEmpty(value))
            {
                value = ConfigurationManager.AppSettings[settingName];
            }
            return value;
        }
    }
}
