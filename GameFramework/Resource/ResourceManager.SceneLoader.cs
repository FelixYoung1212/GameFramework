using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 场景加载器
        /// </summary>
        private sealed class SceneLoader
        {
            private IResourceHelper m_ResourceHelper;

            /// <summary>
            /// 加载中的场景句柄列表
            /// </summary>
            private readonly List<AsyncOperationHandleBase> m_LoadingSceneHandles;

            /// <summary>
            /// 加载中的场景名字对句柄字典，加速查询
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadingSceneNameToHandleMap;

            /// <summary>
            /// 加载完成的场景名字列表，临时列表
            /// </summary>
            private readonly List<string> m_LoadCompletedSceneNames;

            /// <summary>
            /// 加载成功的场景字典
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadedSceneNameToHandleMap;

            /// <summary>
            /// 卸载中的场景句柄列表
            /// </summary>
            private readonly List<AsyncOperationHandleBase> m_UnLoadingSceneHandles;

            /// <summary>
            /// 卸载中的场景字典
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_UnloadingSceneNameToHandleMap;

            /// <summary>
            /// 卸载完成的场景列表，临时列表
            /// </summary>
            private readonly List<string> m_UnloadCompletedSceneNames;

            /// <summary>
            /// 初始化场景加载器的新实例。
            /// </summary>
            public SceneLoader()
            {
                m_LoadingSceneHandles = new List<AsyncOperationHandleBase>();
                m_LoadingSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadCompletedSceneNames = new List<string>();
                m_LoadedSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_UnLoadingSceneHandles = new List<AsyncOperationHandleBase>();
                m_UnloadingSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_UnloadCompletedSceneNames = new List<string>();
            }

            /// <summary>
            /// 设置资源辅助器。
            /// </summary>
            /// <param name="resourceHelper">资源辅助器。</param>
            public void SetResourceHelper(IResourceHelper resourceHelper)
            {
                if (resourceHelper == null)
                {
                    throw new GameFrameworkException("Resource helper is invalid.");
                }

                m_ResourceHelper = resourceHelper;
            }

            /// <summary>
            /// 场景加载器轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                AsyncOperationHandleUtility.UpdateHandles(m_LoadingSceneHandles, elapseSeconds, realElapseSeconds);
                AsyncOperationHandleUtility.RemoveHandlesByName(m_LoadCompletedSceneNames, m_LoadingSceneHandles, m_LoadingSceneNameToHandleMap);
                AsyncOperationHandleUtility.RemoveInvalidHandles(m_LoadingSceneHandles, m_LoadingSceneNameToHandleMap);
                AsyncOperationHandleUtility.UpdateHandles(m_UnLoadingSceneHandles, elapseSeconds, realElapseSeconds);
                AsyncOperationHandleUtility.RemoveHandlesByName(m_UnloadCompletedSceneNames, m_UnLoadingSceneHandles, m_UnloadingSceneNameToHandleMap);
                AsyncOperationHandleUtility.RemoveInvalidHandles(m_UnLoadingSceneHandles, m_UnloadingSceneNameToHandleMap);
            }

            /// <summary>
            /// 关闭并清理场景加载器。
            /// </summary>
            public void Shutdown()
            {
                m_LoadingSceneHandles.Clear();
                m_LoadingSceneNameToHandleMap.Clear();
                m_LoadCompletedSceneNames.Clear();
                m_LoadedSceneNameToHandleMap.Clear();
                m_UnLoadingSceneHandles.Clear();
                m_UnloadingSceneNameToHandleMap.Clear();
                m_UnloadCompletedSceneNames.Clear();
            }

            /// <summary>
            /// 异步加载场景。
            /// </summary>
            /// <param name="sceneAssetName">要加载场景资源的名称。</param>
            /// <returns>异步加载场景句柄</returns>
            public AsyncOperationHandleBase LoadScene(string sceneAssetName)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (m_LoadedSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase op))
                {
                    if (m_LoadingSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase _))
                    {
                        return op;
                    }

                    op.OnSucceeded += _ => m_LoadCompletedSceneNames.Add(sceneAssetName);
                    op.Execute();
                    m_LoadingSceneHandles.Add(op);
                    m_LoadingSceneNameToHandleMap.Add(sceneAssetName, op);
                    return op;
                }

                if (m_LoadingSceneNameToHandleMap.TryGetValue(sceneAssetName, out op))
                {
                    return op;
                }

                try
                {
                    op = m_ResourceHelper.LoadScene(sceneAssetName);
                    op.OnSucceeded += LoadSceneSuccessCallback;
                    op.OnFailed += LoadSceneFailCallback;
                    op.Execute();
                    m_LoadingSceneHandles.Add(op);
                    m_LoadedSceneNameToHandleMap.Add(sceneAssetName, op);
                    return op;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("load scene failed scene name: {0} error message: {1}.", sceneAssetName, e.Message));
                }
            }

            /// <summary>
            /// 异步卸载场景。
            /// </summary>
            /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
            /// <returns>异步卸载场景句柄</returns>
            public AsyncOperationHandleBase UnloadScene(string sceneAssetName)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (!m_LoadedSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase loadOp))
                {
                    throw new GameFrameworkException(Utility.Text.Format("scene {0} is not loaded.", sceneAssetName));
                }

                if (m_UnloadingSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase unloadOp))
                {
                    return unloadOp;
                }

                try
                {
                    unloadOp = m_ResourceHelper.UnloadScene(loadOp);
                    unloadOp.OnSucceeded += UnloadSceneSuccessCallback;
                    unloadOp.OnFailed += UnloadSceneFailureCallback;
                    unloadOp.Execute();
                    m_UnLoadingSceneHandles.Add(unloadOp);
                    m_UnloadingSceneNameToHandleMap.Add(sceneAssetName, unloadOp);
                    return unloadOp;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("unload scene {0} failed error message {1}.", sceneAssetName, e.Message));
                }
            }

            private void LoadSceneSuccessCallback(AsyncOperationHandleBase handle)
            {
                m_LoadedSceneNameToHandleMap[handle.AssetName] = handle;
                m_LoadCompletedSceneNames.Add(handle.AssetName);
            }

            private void LoadSceneFailCallback(AsyncOperationHandleBase handle)
            {
                m_LoadCompletedSceneNames.Add(handle.AssetName);
                GameFrameworkLog.Error(Utility.Text.Format("Load scene failure, scene asset name '{0}', error message '{1}'.", handle.AssetName, handle.ErrorMessage));
            }

            private void UnloadSceneSuccessCallback(AsyncOperationHandleBase handle)
            {
                m_LoadedSceneNameToHandleMap.Remove(handle.AssetName);
                m_UnloadCompletedSceneNames.Add(handle.AssetName);
            }

            private void UnloadSceneFailureCallback(AsyncOperationHandleBase handle)
            {
                m_UnloadCompletedSceneNames.Add(handle.AssetName);
                GameFrameworkLog.Error(Utility.Text.Format("Unload scene failure, scene asset name '{0}', error message '{1}'.", handle.AssetName, handle.ErrorMessage));
            }
        }
    }
}