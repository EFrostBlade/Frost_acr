using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AEAssist.Helper;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

namespace Frost.IPC
{
    /// <summary>
    /// NavmeshIpc 类用于与 Dalamud 插件的 IPC 通信，提供导航网格相关的功能。
    /// </summary>
    internal sealed class NavmeshIpc
    {
        private readonly ICallGateSubscriber<bool> _isNavReady;
        private readonly ICallGateSubscriber<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>> _navPathfind;
        private readonly ICallGateSubscriber<List<Vector3>, bool, object> _pathMoveTo;
        private readonly ICallGateSubscriber<object> _pathStop;
        private readonly ICallGateSubscriber<bool> _pathIsRunning;
        private readonly ICallGateSubscriber<List<Vector3>> _pathListWaypoints;
        private readonly ICallGateSubscriber<float, object> _pathSetTolerance;
        private readonly ICallGateSubscriber<Vector3, bool, float, Vector3?> _queryPointOnFloor;
        private readonly ICallGateSubscriber<float> _buildProgress;

        /// <summary>
        /// 构造函数，初始化 IPC 通信订阅者。
        /// </summary>
        /// <param name="pluginInterface">Dalamud 插件接口。</param>
        public NavmeshIpc(IDalamudPluginInterface pluginInterface)
        {
            _isNavReady = pluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
            _navPathfind = pluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>>("vnavmesh.Nav.PathfindCancelable");
            _pathMoveTo = pluginInterface.GetIpcSubscriber<List<Vector3>, bool, object>("vnavmesh.Path.MoveTo");
            _pathStop = pluginInterface.GetIpcSubscriber<object>("vnavmesh.Path.Stop");
            _pathIsRunning = pluginInterface.GetIpcSubscriber<bool>("vnavmesh.Path.IsRunning");
            _pathListWaypoints = pluginInterface.GetIpcSubscriber<List<Vector3>>("vnavmesh.Path.ListWaypoints");
            _pathSetTolerance = pluginInterface.GetIpcSubscriber<float, object>("vnavmesh.Path.SetTolerance");
            _queryPointOnFloor = pluginInterface.GetIpcSubscriber<Vector3, bool, float, Vector3?>("vnavmesh.Query.Mesh.PointOnFloor");
            _buildProgress = pluginInterface.GetIpcSubscriber<float>("vnavmesh.Nav.BuildProgress");
        }

        /// <summary>
        /// 检查导航网格是否准备就绪。
        /// </summary>
        public bool IsReady
        {
            get
            {
                try
                {
                    return _isNavReady.InvokeFunc();
                }
                catch (IpcError)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 检查当前路径是否正在运行。
        /// </summary>
        public bool IsPathRunning
        {
            get
            {
                try
                {
                    return _pathIsRunning.InvokeFunc();
                }
                catch (IpcError)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 停止当前导航路径。
        /// </summary>
        public void Stop()
        {
            try
            {
                _pathStop.InvokeAction();
            }
            catch (IpcError e)
            {
                LogHelper.Print($"[Warning] 无法通过导航网格停止导航: {e.Message}");
            }
        }

        /// <summary>
        /// 计算从当前位置到目标位置的路径。
        /// </summary>
        /// <param name="localPlayerPosition">玩家当前位置。</param>
        /// <param name="targetPosition">目标位置。</param>
        /// <param name="fly">是否允许飞行。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>路径点列表的任务。</returns>
        public Task<List<Vector3>> Pathfind(Vector3 localPlayerPosition, Vector3 targetPosition, bool fly, CancellationToken cancellationToken)
        {
            try
            {
                _pathSetTolerance.InvokeAction(0.25f);
                return _navPathfind.InvokeFunc(localPlayerPosition, targetPosition, fly, cancellationToken);
            }
            catch (IpcError e)
            {
                LogHelper.Print($"[Warning] 无法通过导航网格计算路径: {e.Message}");
                return Task.FromException<List<Vector3>>(e);
            }
        }

        /// <summary>
        /// 移动到指定位置。
        /// </summary>
        /// <param name="position">目标位置列表。</param>
        /// <param name="fly">是否允许飞行。</param>
        public void MoveTo(List<Vector3> position, bool fly)
        {
            Stop();
            try
            {
                _pathMoveTo.InvokeAction(position, fly);
            }
            catch (IpcError e)
            {
                LogHelper.Print($"[Warning] 无法通过导航网格移动: {e.Message}");
            }
        }

        /// <summary>
        /// 获取指定位置的地面点。
        /// </summary>
        /// <param name="position">目标位置。</param>
        /// <param name="unlandable">是否允许不可降落区域。</param>
        /// <returns>地面点的坐标。</returns>
        public Vector3? GetPointOnFloor(Vector3 position, bool unlandable)
        {
            try
            {
                return _queryPointOnFloor.InvokeFunc(position, unlandable, 0.2f);
            }
            catch (IpcError)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取当前路径的所有路径点。
        /// </summary>
        /// <returns>路径点列表。</returns>
        public List<Vector3> GetWaypoints()
        {
            if (IsPathRunning)
            {
                try
                {
                    return _pathListWaypoints.InvokeFunc();
                }
                catch (IpcError)
                {
                    return new List<Vector3>();
                }
            }
            return new List<Vector3>();
        }

        /// <summary>
        /// 获取导航网格的构建进度。
        /// </summary>
        /// <returns>构建进度的百分比。</returns>
        public int GetBuildProgress()
        {
            try
            {
                float progress = _buildProgress.InvokeFunc();
                return progress < 0f ? 100 : (int)(progress * 100f);
            }
            catch (IpcError)
            {
                return 0;
            }
        }
    }
}
