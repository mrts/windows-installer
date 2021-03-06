using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;

namespace FirefoxAction
{
    public class FirefoxActions
    {
        [CustomAction]
        public static ActionResult ExtensionSettingsInstall(Session session)
        {
            try
            {
                session.Log("Begin ExtensionSettingsInstall " + session["EXTENSIONSETTINGS_UUID"]);
                using (RegistryKey firefox = Utils.FirefoxKey())
                {
                    string[] value = (string[])firefox.GetValue("ExtensionSettings", new string[] { "{}" });
                    JObject json = JObject.Parse(string.Join("\n", value));
                    json[session["EXTENSIONSETTINGS_UUID"]] = new JObject
                    {
                        ["installation_mode"] = "normal_installed",
                        ["install_url"] = session["EXTENSIONSETTINGS_URL"]
                    };
                    firefox.SetValue("ExtensionSettings", json.ToString().Split('\n'));
                    return ActionResult.Success;
                }
            }
            catch (Exception e)
            {
                session.Log("ExtensionSettingsInstall " + session["EXTENSIONSETTINGS_UUID"] + " failed: " + e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult ExtensionSettingsRemove(Session session)
        {
            try
            {
                session.Log("Begin ExtensionSettingsRemove " + session["EXTENSIONSETTINGS_UUID"]);
                using (RegistryKey firefox = Utils.FirefoxKey())
                {
                    string[] value = (string[])firefox.GetValue("ExtensionSettings");
                    if (value != null)
                    {
                        JObject json = JObject.Parse(string.Join("\n", value));
                        json[session["EXTENSIONSETTINGS_UUID"]] = new JObject
                        {
                            ["installation_mode"] = "blocked"
                        };
                        firefox.SetValue("ExtensionSettings", json.ToString().Split('\n'));
                    }
                    return ActionResult.Success;
                }
            }
            catch (Exception e)
            {
                session.Log("ExtensionSettingsRemove " + session["EXTENSIONSETTINGS_UUID"] + " failed: " + e.Message);
                return ActionResult.Failure;
            }
        }
    }

    internal static class Utils
    {
        internal static RegistryKey FirefoxKey()
        {
            using (RegistryKey mozilla = Registry.LocalMachine.OpenOrCreateSubKey(@"Software\Policies\Mozilla", true))
                return mozilla.OpenOrCreateSubKey(@"Firefox", true);
        }

        internal static RegistryKey OpenOrCreateSubKey(this RegistryKey registryKey, string name, bool writable = false)
        {
            return registryKey.OpenSubKey(name, writable) ?? registryKey.CreateSubKey(name);
        }
    }
}
