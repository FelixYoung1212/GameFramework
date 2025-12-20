using System;
using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        private sealed class AssetLoader
        {
            private IResourceHelper m_ResourceHelper;

            /// <summary>
            /// 加载中的资源列表
            /// </summary>
            private readonly List<AsyncOperationHandleBase> m_LoadingAssetHandles;

            /// <summary>
            /// 加载中的资源列表,加速查询
            /// </summary>
            private readonly Dictionary<string, AsyncOperationHandleBase> m_LoadingAssetNameToHandleMap;

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
            /// 初始化加载资源器的新实例。
            /// </summary>
            public AssetLoader()
            {
                m_LoadingAssetHandles = new List<AsyncOperationHandleBase>();
                m_LoadingAssetNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadCompletedAssetNames = new List<string>();
                m_LoadedAssetNameToHandleMap = new Dictionary<string, AsyncOperationHandleBase>(StringComparer.Ordinal);
                m_LoadedAssetToHandleMap = new Dictionary<object, AsyncOperationHandleBase>();
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
                for (var i = 0; i < m_LoadingAssetHandles.Count; i++)
                {
                    var handle = m_LoadingAssetHandles[i];
                    if (handle.IsValid)
                    {
                        handle.Update(elapseSeconds, realElapseSeconds);
                    }
                }

                if (m_LoadCompletedAssetNames.Count > 0)
                {
                    foreach (var assetName in m_LoadCompletedAssetNames)
                    {
                        if (!m_LoadingAssetNameToHandleMap.TryGetValue(assetName, out var handle))
                        {
                            continue;
                        }

                        m_LoadingAssetHandles.Remove(handle);
                        m_LoadingAssetNameToHandleMap.Remove(handle.AssetName);
                    }

                    m_LoadCompletedAssetNames.Clear();
                }

                for (var i = m_LoadingAssetHandles.Count - 1; i >= 0; i--)
                {
                    var handle = m_LoadingAssetHandles[i];
                    if (handle.IsValid)
                    {
                        continue;
                    }

                    m_LoadingAssetHandles.RemoveAt(i);
                    m_LoadingAssetNameToHandleMap.Remove(handle.AssetName);
                }
            }

            /// <summary>
            /// 关闭并清理加载资源器。
            /// </summary>
            public void Shutdown()
            {
                m_LoadingAssetHandles.Clear();
                m_LoadingAssetNameToHandleMap.Clear();
                m_LoadCompletedAssetNames.Clear();
                m_LoadedAssetNameToHandleMap.Clear();
                m_LoadedAssetToHandleMap.Clear();
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
                    op.OnSucceeded += handle => handle.IncrementReferenceCount();
                    op.OnSucceeded += _ => m_LoadCompletedAssetNames.Add(assetName);
                    op.Execute();
                    AddHandleToLoading(op);
                    return op;
                }

                if (m_LoadingAssetNameToHandleMap.TryGetValue(assetName, out op))
                {
                    op.OnSucceeded += handle => handle.IncrementReferenceCount();
                    return op;
                }

                try
                {
                    op = m_ResourceHelper.LoadAsset(assetName);
                    op.OnSucceeded += LoadAssetSuccessCallback;
                    op.OnFailed += LoadAssetFailCallback;
                    op.Execute();
                    AddHandleToLoading(op);
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

                op.DecrementReferenceCount();

                if (op.ReferenceCount > 0)
                {
                    return;
                }

                var assetName = op.AssetName;
                try
                {
                    m_ResourceHelper.UnloadAsset(op);
                    m_LoadedAssetNameToHandleMap.Remove(assetName);
                    m_LoadedAssetToHandleMap.Remove(asset);
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not unload asset {0}, error message {1}.", assetName, e.Message));
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
                    throw new GameFrameworkException(Utility.Text.Format("Can not instantiate asset {0} error message {1}.", op.AssetName, e.Message));
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
                    throw new GameFrameworkException(Utility.Text.Format("Can not release instance {0} error message {1}.", op.AssetName, e.Message));
                }
            }

            private void AddHandleToLoading(AsyncOperationHandleBase handle)
            {
                if (m_LoadingAssetNameToHandleMap.TryGetValue(handle.AssetName, out AsyncOperationHandleBase _))
                {
                    return;
                }

                m_LoadingAssetHandles.Add(handle);
                m_LoadingAssetNameToHandleMap.Add(handle.AssetName, handle);
            }

            private void RemoveHandleFromLoading(AsyncOperationHandleBase handle)
            {
                m_LoadingAssetHandles.Remove(handle);
                m_LoadingAssetNameToHandleMap.Remove(handle.AssetName);
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
        }
    }
}