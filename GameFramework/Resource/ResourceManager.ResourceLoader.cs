using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        private sealed partial class ResourceLoader
        {
            private IResourceHelper m_ResourceHelper;
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadedAssetNameToHandleMap;
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadedSceneNameToHandleMap;
            private readonly Dictionary<string, AsyncOperationHandleBase> m_UnloadSceneNameToHandleMap;

            /// <summary>
            /// 初始化加载资源器的新实例。
            /// </summary>
            public ResourceLoader()
            {
                m_LoadedAssetNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>();
                m_LoadedSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_UnloadSceneNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
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
            /// 加载资源器轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                foreach (var kvp in m_LoadedAssetNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_LoadedSceneNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }

                foreach (var kvp in m_UnloadSceneNameToHandleMap)
                {
                    kvp.Value.Update(elapseSeconds, realElapseSeconds);
                }
            }

            /// <summary>
            /// 关闭并清理加载资源器。
            /// </summary>
            public void Shutdown()
            {
                m_LoadedAssetNameToHandleMap.Clear();
                m_LoadedSceneNameToHandleMap.Clear();
                m_UnloadSceneNameToHandleMap.Clear();
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

                if (!m_LoadedAssetNameToHandleMap.TryGetValue(assetName, out AsyncOperationHandleBase op))
                {
                    op = m_ResourceHelper.LoadAsset(assetName);
                    op.OnFailed += handle => LoadAssetFailCallback(assetName, handle.ErrorMessage);
                    m_LoadedAssetNameToHandleMap.Add(assetName, op);
                }

                op.Start();
                return op;
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

                m_ResourceHelper.UnloadAsset(asset);
            }

            /// <summary>
            /// 实例化资源。
            /// </summary>
            /// <param name="asset">要实例化的资源。</param>
            public void Instantiate(object asset)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                m_ResourceHelper.Instantiate(asset);
            }

            /// <summary>
            /// 释放并且销毁实例化资源
            /// </summary>
            /// <param name="instance"></param>
            public void ReleaseInstance(object instance)
            {
                if (m_ResourceHelper == null)
                {
                    throw new GameFrameworkException("You must set resource helper first.");
                }

                m_ResourceHelper.ReleaseInstance(instance);
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

                if (!m_LoadedSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase op))
                {
                    op = m_ResourceHelper.LoadScene(sceneAssetName);
                    op.OnFailed += handle => LoadSceneFailCallback(sceneAssetName, handle.ErrorMessage);
                    m_LoadedSceneNameToHandleMap.Add(sceneAssetName, op);
                }

                op.Start();
                return op;
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

                if (!m_UnloadSceneNameToHandleMap.TryGetValue(sceneAssetName, out AsyncOperationHandleBase op))
                {
                    op = m_ResourceHelper.UnloadScene(sceneAssetName);
                    op.OnSucceeded += handle => UnloadSceneSuccessCallback(sceneAssetName);
                    op.OnFailed += handle => UnloadSceneFailureCallback(sceneAssetName, handle.ErrorMessage);
                    m_UnloadSceneNameToHandleMap.Add(sceneAssetName, op);
                }

                op.Start();
                return op;
            }

            private void LoadAssetFailCallback(string assetName, string errorMessage)
            {
                m_LoadedAssetNameToHandleMap.Remove(assetName);
                throw new GameFrameworkException(Utility.Text.Format("Load asset failure, asset name '{0}', error message '{1}'.", assetName, errorMessage));
            }

            private void LoadSceneFailCallback(string sceneAssetName, string errorMessage)
            {
                m_LoadedSceneNameToHandleMap.Remove(sceneAssetName);
                throw new GameFrameworkException(Utility.Text.Format("Load scene failure, scene asset name '{0}', error message '{1}'.", sceneAssetName, errorMessage));
            }

            private void UnloadSceneSuccessCallback(string sceneAssetName)
            {
                m_UnloadSceneNameToHandleMap.Remove(sceneAssetName);
                m_LoadedSceneNameToHandleMap.Remove(sceneAssetName);
            }

            private void UnloadSceneFailureCallback(string sceneAssetName, string errorMessage)
            {
                m_UnloadSceneNameToHandleMap.Remove(sceneAssetName);
                throw new GameFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}', error message '{1}'.", sceneAssetName, errorMessage));
            }
        }
    }
}