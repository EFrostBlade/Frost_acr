using AEAssist;
using AEAssist.Helper;
using AEAssist.Verify;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Frost.HOOK
{
    // Hook补丁类型枚举
    public enum PatchType
    {
        NoActionMove,       // 突进无位移
        SkillPostActionMove, // 技能后摇可移动
        ActionRange         // 技能范围修改
    }

    internal class Hook
    {
        internal static IGameInteropProvider GameInteropProvider => ECHelper.Hook;
        public static ISigScanner SigScanner => ECHelper.SigScanner;

        // 委托类型定义
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate ulong NoActionMoveDelegate(
            ulong arg1,
            byte arg2,
            ulong arg3,
            float arg4,
            nint arg5
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate long SkillPostActionMoveDelegate(long arg1);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate float ActionRangeDelegate(uint actionId);


        // 存储各类型Hook的字典
        private static readonly Dictionary<PatchType, IDisposable> _activeHooks = new();

        // NoActionMove Hook
        private static Hook<NoActionMoveDelegate>? _noActionMoveHook;
        private static readonly NoActionMoveDelegate _noActionMoveDetour = NoActionMoveDetour;

        // SkillPostActionMove Hook
        private static Hook<SkillPostActionMoveDelegate>? _skillPostActionMoveHook;
        private static readonly SkillPostActionMoveDelegate _skillPostActionMoveDetour = SkillPostActionMoveDetour;

        // ActionRange Hook
        private static Hook<ActionRangeDelegate>? _actionRangeHook;
        private static readonly ActionRangeDelegate _actionRangeDetour = ActionRangeDetour;

        // 补丁类型的元数据
        private class PatchMeta
        {
            public string Name { get; }
            public string MemorySignature { get; }
            public VIPLevel RequiredVIPLevel { get; }

            public PatchMeta(string name, string memorySignature, VIPLevel requiredVIPLevel = VIPLevel.Normal)
            {
                Name = name;
                MemorySignature = memorySignature;
                RequiredVIPLevel = requiredVIPLevel;
            }
        }

        // 补丁类型与元数据的映射
        private static readonly Dictionary<PatchType, PatchMeta> _patchMetas = new()
        {
            {
                PatchType.NoActionMove,
                new PatchMeta(
                    "突进无位移",
                    "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F1 0F 29 74 24 ?? 48 8B 89 ?? ?? ?? ?? 0F 28 F3",
                    VIPLevel.VIP1
                )
            },
            {
                PatchType.SkillPostActionMove,
                new PatchMeta(
                    "技能后摇可移动",
                    "E8 ?? ?? ?? ?? C6 83 ?? ?? ?? ?? ?? E9 96 00 ?? ??",
                    VIPLevel.VIP1
                )
            },
            {
                PatchType.ActionRange,
                new PatchMeta(
                    "技能距离增强",
                    "48 89 5C 24 ?? 57 48 ?? ?? ?? 48 ?? ?? ?? ?? ?? ?? 8B ?? 0F 29 74 24 20",
                    VIPLevel.VIP1
                )
            }
        };

        // 通用的启用补丁方法
        public static void EnablePatch(PatchType patchType)
        {
            // 检查补丁是否已启用
            if (_activeHooks.ContainsKey(patchType))
                return;

            var meta = _patchMetas[patchType];

            // 对于需要VIP权限的补丁进行判断
            if (meta.RequiredVIPLevel != VIPLevel.Normal && Share.VIP.Level < meta.RequiredVIPLevel)
            {
                LogHelper.Print($"{meta.Name}需要{meta.RequiredVIPLevel}权限");
                return;
            }

            try
            {
                // 搜索内存地址
                var address = SigScanner.ScanText(meta.MemorySignature);
                if (address == IntPtr.Zero)
                {
                    LogHelper.Print($"无法找到方法 {meta.Name} 的内存地址");
                    return;
                }

                // 根据补丁类型创建特定的Hook
                switch (patchType)
                {
                    case PatchType.NoActionMove:
                        _noActionMoveHook = GameInteropProvider.HookFromAddress<NoActionMoveDelegate>(address, _noActionMoveDetour);
                        _noActionMoveHook.Enable();
                        _activeHooks[patchType] = _noActionMoveHook;
                        break;

                    case PatchType.SkillPostActionMove:
                        _skillPostActionMoveHook = GameInteropProvider.HookFromAddress<SkillPostActionMoveDelegate>(address, _skillPostActionMoveDetour);
                        _skillPostActionMoveHook.Enable();
                        _activeHooks[patchType] = _skillPostActionMoveHook;
                        break;
                    case PatchType.ActionRange:
                        _actionRangeHook = GameInteropProvider.HookFromAddress<ActionRangeDelegate>(address, _actionRangeDetour);
                        _actionRangeHook.Enable();
                        _activeHooks[patchType] = _actionRangeHook;
                        break;
                }

                LogHelper.Print($"{meta.Name}已开启");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"启用{meta.Name}时发生错误: {ex.Message}");
            }
        }

        // 通用的禁用补丁方法
        public static void DisablePatch(PatchType patchType)
        {
            if (!_activeHooks.TryGetValue(patchType, out var hook))
                return;

            try
            {
                hook.Dispose();
                _activeHooks.Remove(patchType);

                // 清空相应的Hook引用
                switch (patchType)
                {
                    case PatchType.NoActionMove:
                        _noActionMoveHook = null;
                        break;

                    case PatchType.SkillPostActionMove:
                        _skillPostActionMoveHook = null;
                        break;
                    case PatchType.ActionRange:
                        _actionRangeHook = null;
                        break;
                }

                LogHelper.Print($"{_patchMetas[patchType].Name}已关闭");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"禁用{_patchMetas[patchType].Name}时发生错误: {ex.Message}");
            }
        }

        // 补丁方法实现
        private static unsafe ulong NoActionMoveDetour(ulong arg1, byte arg2, ulong arg3, float arg4, nint arg5)
        {
            return 0;
        }

        private static unsafe long SkillPostActionMoveDetour(long arg1)
        {
            return arg1;
        }
        
        private static unsafe float ActionRangeDetour(uint actionId)
        {

            // 获取原始范围值并添加修改值
            float originalRange = _actionRangeHook!.Original(actionId);
            float modifiedRange = originalRange + 3.0f;

            return modifiedRange;
        }
    }
}