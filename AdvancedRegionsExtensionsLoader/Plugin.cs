using System;
using System.Collections.Generic;
using System.Reflection;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using SDG.NetTransport;
using SDG.Unturned;
using UnityEngine;
using Environment = Rocket.Core.Environment;
using Logger = Rocket.Core.Logging.Logger;

namespace AdvancedRegionsExtensionsLoader
{
    public class Plugin : RocketPlugin
    {
        protected override void Load()
        {
            Logger.Log("AdvancedRegionsExtensionsLoader has been loaded");
            Level.onLevelLoaded += OnLevelLoaded;
        }

        protected override void Unload()
        {
            Logger.Log("AdvancedRegionsExtensionsLoader has been unloaded");
            Level.onLevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(int level)
        {    
            var type = typeof(RocketPluginManager);
            var assembliesInfo = type.GetField("pluginAssemblies", BindingFlags.NonPublic | BindingFlags.Static);
            if (assembliesInfo == null || assembliesInfo.GetValue(null) is not List<Assembly> pluginAssemblies)
            {
                Logger.Log("Plugin assemblies not found");
                return;
            }

            var pluginsInfo = type.GetField("plugins", BindingFlags.NonPublic | BindingFlags.Static);
            if (pluginsInfo == null || pluginsInfo.GetValue(null) is not List<GameObject> plugins)
            {
                Logger.Log("Plugins not found");
                return;
            }
            
            Logger.Log("Loading all extensions");
            var assemblies = RocketPluginManager.LoadAssembliesFromDirectory(Environment.PluginsDirectory, "*.ext");
            Logger.Log($"Found {assemblies.Count} extensions");
            if (assemblies.Count == 0)
                return;
            
            pluginAssemblies.AddRange(assemblies);
            var pluginImplemenations = RocketHelper.GetTypesFromInterface(assemblies, "IRocketPlugin");
            foreach (var pluginType in pluginImplemenations)
            {
                var plugin = new GameObject(pluginType.Name, pluginType);
                DontDestroyOnLoad(plugin);
                plugins.Add(plugin);
            }
        }
    }
}