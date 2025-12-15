using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        private sealed class ResourceLoader
        {
            private IResourceHelper m_ResourceHelper;

            /// <summary>
            /// 加载中的资源列表
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadingAssetNameToHandlesMap;

            /// <summary>
            /// 加载完成的资源列表，临时列表
            /// </summary>
            private readonly List<string> m_LoadCompletedAssetNames;

            /// <summary>
            /// 加载成功的资源列表
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadedAssetNameToHandleMap;

            /// <summary>
            /// 加载成功的资源列表
            /// </summary>
            private readonly Dictionary<object, AsyncOperationHandleBase> m_LoadedAssetToHandleMap;

            /// <summary>
            /// 加载中的场景列表
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadingSceneNameToHandlesMap;

            /// <summary>
            /// 加载完成的场景列表，临时列表
            /// </summary>
            private readonly List<string> m_LoadCompletedSceneNames;

            /// <summary>
            /// 加载成功的场景字典
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadedSceneNameToHandleMap;

            /// <summary>
            /// 卸载中的场景字典
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_UnloadingSceneNameToHandleMap;

            /// <summary>
            /// 卸载完成的场景列表，临时列表
            /// </summary>
            private readonly List<string> m_UnloadCompletedSceneNames;

            /// <summary>
            /// 初始化加载资源器的新实例。
            /// </summary>
            public ResourceLoader()
            {
                m_LoadingAssetNameToHandlesMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadCompletedAssetNames = new List<string>();
                m_LoadedAssetNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadedAssetToHandleMap = new Dictionary<object, AsyncOperationHandleBase>();
                m_LoadingSceneNameToHandlesMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadCompletedSceneNames = new List<string>();
                m_LoadedSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
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
            /// 资源加载器轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                foreach (var kvp in m_LoadingAssetNameToHandlesMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_LoadingSceneNameToHandlesMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_UnloadingSceneNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_LoadedAssetNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_LoadedSceneNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                if (m_LoadCompletedAssetNames.Count > 0)
                {
                    foreach (var assetName in m_LoadCompletedAssetNames)
                    {
                        m_LoadingAssetNameToHandlesMap.Remove(assetName);
                    }

                    m_LoadCompletedAssetNames.Clear();
                }

                if (m_LoadCompletedSceneNames.Count > 0)
                {
                    foreach (var sceneName in m_LoadCompletedSceneNames)
                    {
                        m_LoadingSceneNameToHandlesMap.Remove(sceneName);
                    }

                    m_LoadCompletedSceneNames.Clear();
                }

                if (m_UnloadCompletedSceneNames.Count > 0)
                {
                    foreach (var sceneName in m_UnloadCompletedSceneNames)
                    {
                        m_UnloadingSceneNameToHandleMap.Remove(sceneName);
                    }

                    m_UnloadCompletedSceneNames.Clear();
                }
            }

            /// <summary>
            /// 关闭并清理加载资源器。
            /// </summary>
            public void Shutdown()
            {
                m_LoadingAssetNameToHandlesMap.Clear();
                m_LoadCompletedAssetNames.Clear();
                m_LoadedAssetNameToHandleMap.Clear();
                m_LoadedAssetToHandleMap.Clear();
                m_LoadingSceneNameToHandlesMap.Clear();
                m_LoadCompletedSceneNames.Clear();
                m_LoadedSceneNameToHandleMap.Clear();
                m_UnloadingSceneNameToHandleMap.Clear();
                m_UnloadCompletedSceneNames.Clear();
            }

            /// <summary>
            /// 异步加载资源。
            /// </summary>
            /// <param name="assetName">要加载资源的名称。</param>
            /// <returns>异步加载资源句柄</returns>
            public AsyncOperationHandleBase LoadAsset(string assetName)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (m_LoadedAssetNameToHandleMap.TryGetValue(assetName, out AsyncOperationHandleBase op))
                {
                    op.Start();
                    return op;
                }

                if (m_LoadingAssetNameToHandlesMap.TryGetValue(assetName, out op))
                {
                    return op;
                }

                try
                {
                    op = m_ResourceHelper.LoadAsset(assetName);
                    op.OnSucceeded += LoadAssetSuccessCallback;
                    op.OnFailed += LoadAssetFailCallback;
                    op.Start();
                    m_LoadingAssetNameToHandlesMap.Add(assetName, op);
                    return op;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("load asset failed asset name: {0} error message: {1}.", assetName, e.Message));
                }
            }

            /// <summary>
            /// 卸载资源。
            /// </summary>
            /// <param name="asset">要卸载的资源。</param>
            public void UnloadAsset(object asset)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (!m_LoadedAssetToHandleMap.TryGetValue(asset, out AsyncOperationHandleBase op))
                {
                    throw new GameFrameworkException(Utility.Text.Format("asset {0} is not loaded.", asset.ToString()));
                }

                if (op.ReferenceCount > 1)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not unload asset {0}, reference count {1}.", asset.ToString(), op.ReferenceCount));
                }

                op.DecrementReferenceCount();

                try
                {
                    m_ResourceHelper.UnloadAsset(op);
                    var handle = m_LoadedAssetToHandleMap[asset];
                    m_LoadedAssetNameToHandleMap.Remove(handle.AssetName);
                    m_LoadedAssetToHandleMap.Remove(handle.Result);
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not unload asset {0}, error message {1}.", asset.ToString(), e.Message));
                }
            }

            /// <summary>
            /// 实例化资源。
            /// </summary>
            /// <param name="asset">要实例化的资源。</param>
            /// <returns>资源实例</returns>
            public object Instantiate(object asset)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (!m_LoadedAssetToHandleMap.TryGetValue(asset, out AsyncOperationHandleBase op))
                {
                    throw new GameFrameworkException(Utility.Text.Format("asset {0} is not loaded.", asset.ToString()));
                }

                try
                {
                    object instance = m_ResourceHelper.Instantiate(asset);
                    op.IncrementReferenceCount();
                    return instance;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not instantiate asset {0} error message {1}.", asset.ToString(), e.Message));
                }
            }

            /// <summary>
            /// 释放并且销毁实例化资源
            /// </summary>
            /// <param name="instance">资源实例</param>
            /// <param name="asset">原始资源</param>
            public void ReleaseInstance(object instance, object asset)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                if (!m_LoadedAssetToHandleMap.TryGetValue(asset, out AsyncOperationHandleBase op))
                {
                    throw new GameFrameworkException(Utility.Text.Format("asset {0} is not loaded.", asset.ToString()));
                }

                try
                {
                    m_ResourceHelper.ReleaseInstance(instance, asset);
                    op.DecrementReferenceCount();
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not release instance {0} error message {1}.", instance.ToString(), e.Message));
                }
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
                    op.Start();
                    return op;
                }
                
                if (m_LoadingSceneNameToHandlesMap.TryGetValue(sceneAssetName, out op))
                {
                    return op;
                }

                try
                {
                    op = m_ResourceHelper.LoadScene(sceneAssetName);
                    op.OnSucceeded += LoadSceneSuccessCallback;
                    op.OnFailed += LoadSceneFailCallback;
                    op.Start();
                    m_LoadingSceneNameToHandlesMap.Add(sceneAssetName, op);
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
                    unloadOp.Start();
                    m_UnloadingSceneNameToHandleMap.Add(sceneAssetName, unloadOp);
                    return unloadOp;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("unload scene {0} failed error message {1}.", sceneAssetName, e.Message));
                }
            }

            private void LoadAssetSuccessCallback(AsyncOperationHandleBase handle)
            {
                m_LoadedAssetNameToHandleMap[handle.AssetName] = handle;
                m_LoadedAssetToHandleMap[handle.Result] = handle;
                m_LoadCompletedAssetNames.Add(handle.AssetName);
            }

            private void LoadAssetFailCallback(AsyncOperationHandleBase handle)
            {
                m_LoadCompletedAssetNames.Add(handle.AssetName);
                GameFrameworkLog.Error(Utility.Text.Format("Load asset failure, asset name '{0}', error message '{1}'.", handle.AssetName, handle.ErrorMessage));
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