using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace DontBanMyPlugin
{
    public sealed class Plugin : IDalamudPlugin, IDisposable
    {
        [PluginService]
        internal static IDalamudPluginInterface PluginInterface { get; private set; }
        [PluginService]
        internal static IClientState ClientState { get; private set; }
        [PluginService]
        internal static IPluginLog Log { get; private set; }
        [PluginService]
        internal static IAddonLifecycle AddonLifecycle { get; private set; }
        [PluginService]
        internal static ICommandManager Command { get; private set; }

        private readonly Assembly dalamudAssembly;
        private readonly object pm;

        public Plugin()
        {
            this.dalamudAssembly = PluginInterface.GetType().Assembly;
            this.pm = Reflect.GetService(this.dalamudAssembly,
                                         "Dalamud.Service`1",
                                         "Dalamud.Plugin.Internal.PluginManager",
                                         BindingFlag.Default);

            // 如果处于开发模式则不执行后续操作
            if (PluginInterface.IsDev)
            {
                return;
            }

            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Logo", new IAddonLifecycle.AddonEventDelegate(OnAddon));
            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "_TitleMenu", new IAddonLifecycle.AddonEventDelegate(OnAddon));
            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "_CharaSelectListMenu", new IAddonLifecycle.AddonEventDelegate(OnAddon));

            ClientState.Login += Unban;
            PluginInterface.UiBuilder.OpenMainUi += Unban;
            PluginInterface.UiBuilder.OpenConfigUi += Unban;

            Command.AddHandler("/unban", new CommandInfo(OnCommand)
            {
                HelpMessage = "unban",
                ShowInHelp = true
            });

            ResetBannedList();
            StopSendMeasurement();
            Unban();
        }

        private void OnAddon(AddonEvent type, AddonArgs args)
        {
            Unban();
        }

        private void OnCommand(string command, string args)
        {
            Unban();
        }

        /// <summary>
        /// 解除所有已封禁的插件
        /// </summary>
        private async void Unban()
        {
            Log.Debug("Start Unban", Array.Empty<object>());
            try
            {
                // 获取已安装插件列表
                FieldInfo installedPluginsField = pm.GetType().GetField("installedPluginsList", BindingFlag.All);
                if (installedPluginsField == null)
                {
                    Log.Error("无法获取字段：installedPluginsList");
                    return;
                }
                IEnumerable installedPlugins = installedPluginsField.GetValue(pm) as IEnumerable;
                if (installedPlugins == null)
                {
                    Log.Error("installedPluginsList 为空");
                    return;
                }

                foreach (object plugin in installedPlugins)
                {
                    Type pluginType = plugin.GetType();

                    // 获取插件名称
                    PropertyInfo nameProp = pluginType.GetProperty("Name", BindingFlag.PubIns);
                    string pluginName = nameProp != null ? (string)nameProp.GetValue(plugin) : string.Empty;

                    // 获取插件状态
                    PropertyInfo stateProp = pluginType.GetProperty("State", BindingFlag.PubIns);
                    object state = stateProp?.GetValue(plugin);

                    // 检查是否被封禁，通过反射获取 IsBanned
                    PropertyInfo bannedProp = pluginType.GetProperty("IsBanned", BindingFlag.PubPriIns);
                    bool isBanned = bannedProp != null && bannedProp.GetValue(plugin) is bool b && b;

                    // 如果插件未封禁，则执行解除封禁操作
                    if (!isBanned)
                    {
                        // 对于 LocalDevPlugin，从基类获取后备字段
                        if (pluginType.Name == "LocalDevPlugin")
                        {
                            FieldInfo backingField = Reflect.GetBackingField(pluginType.BaseType, "IsBanned", BindingFlag.PubPriIns);
                            backingField?.SetValue(plugin, false);
                        }
                        else
                        {
                            FieldInfo backingField = Reflect.GetBackingField(pluginType, "IsBanned", BindingFlag.PubPriIns);
                            backingField?.SetValue(plugin, false);
                        }

                        // 如果插件状态为 "LoadError"，则尝试重置状态并调用 LoadAsync 重载该插件
                        if (state?.ToString() == "LoadError")
                        {
                            stateProp?.SetValue(plugin, 0);
                            MethodInfo loadAsyncMethod = pluginType.GetMethod("LoadAsync");
                            if (loadAsyncMethod != null)
                            {
                                object loadResult = loadAsyncMethod.Invoke(plugin, new object[] { 3, false });
                                if (loadResult is Task task)
                                {
                                    await task;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString(), Array.Empty<object>());
            }
        }

        /// <summary>
        /// 重置插件管理器中封禁列表为空数组
        /// </summary>
        private void ResetBannedList()
        {
            try
            {
                FieldInfo bannedPluginsField = pm.GetType().GetField("bannedPlugins", BindingFlag.PriIns);
                object bannedPluginsObj = bannedPluginsField?.GetValue(pm);
                if (bannedPluginsObj != null)
                {
                    Type elementType = bannedPluginsObj.GetType().GetElementType();
                    int length = (int)bannedPluginsObj.GetType().GetProperty("Length").GetValue(bannedPluginsObj);
                    Array newBannedArray = Array.CreateInstance(elementType, length);
                    bannedPluginsField.SetValue(pm, newBannedArray);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString(), Array.Empty<object>());
            }
        }

        /// <summary>
        /// 阻止发送测量数据
        /// </summary>
        private void StopSendMeasurement()
        {
            try
            {
                object chatHandler = Reflect.GetService(dalamudAssembly,
                                                          "Dalamud.Service`1",
                                                          "Dalamud.Game.ChatHandlers",
                                                          BindingFlags.Default);
                FieldInfo measurementField = chatHandler.GetType().GetField("hasSendMeasurement", BindingFlag.PriIns);
                measurementField?.SetValue(chatHandler, true);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString(), Array.Empty<object>());
            }
        }

        public void Dispose()
        {
            AddonLifecycle.UnregisterListener(new IAddonLifecycle.AddonEventDelegate[] { OnAddon });
            PluginInterface.UiBuilder.OpenMainUi -= Unban;
            PluginInterface.UiBuilder.OpenConfigUi -= Unban;
            ClientState.Login -= Unban;
            Command.RemoveHandler("/unban");
        }

        // 辅助内部的 BindingFlags 定义
        public static class BindingFlag
        {
            public static BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            public static BindingFlags PubStc = BindingFlags.Instance | BindingFlags.NonPublic;
            public static BindingFlags PubIns = BindingFlags.Instance | BindingFlags.Public;
            public static BindingFlags PriIns = BindingFlags.Instance | BindingFlags.NonPublic;
            public static BindingFlags PubPriIns = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            public static BindingFlags Default = BindingFlags.Default;
        }

        // 辅助静态类：封装反射操作
        public static class Reflect
        {
            public static FieldInfo GetBackingField(Type type, string propertyName, BindingFlags flags)
            {
                return type?.GetField($"<{propertyName}>k__BackingField", flags);
            }

            public static object GetService(Assembly assembly, string serviceName, string typeName, BindingFlags bind = BindingFlags.Default)
            {
                Type serviceType = assembly.GetType(serviceName, true);
                Type targetType = assembly.GetType(typeName, true);
                Type genericType = serviceType.MakeGenericType(new[] { targetType });
                MethodInfo getMethod = genericType.GetMethod("Get");
                return getMethod.Invoke(null, new object[] { });
            }
        }
    }
}
