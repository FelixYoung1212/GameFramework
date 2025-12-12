namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 资源加载器
        /// </summary>
        private sealed partial class ResourceLoader
        {
            private readonly ResourceManager m_ResourceManager;

            /// <summary>
            /// 初始化加载资源器的新实例。
            /// </summary>
            /// <param name="resourceManager">资源管理器。</param>
            public ResourceLoader(ResourceManager resourceManager)
            {
                m_ResourceManager = resourceManager;
            }

            /// <summary>
            /// 异步加载资源。
            /// </summary>
            /// <param name="assetName">要加载资源的名称。</param>
            /// <returns>异步加载资源句柄</returns>
            public AsyncOperationHandleBase LoadAsset(string assetName)
            {
                return null;
            }

            /// <summary>
            /// 卸载资源。
            /// </summary>
            /// <param name="asset">要卸载的资源。</param>
            public void UnloadAsset(object asset)
            {
            }

            /// <summary>
            /// 实例化资源。
            /// </summary>
            /// <param name="asset">要实例化的资源。</param>
            public void Instantiate(object asset)
            {
            }

            /// <summary>
            /// 释放并且销毁实例化资源
            /// </summary>
            /// <param name="instance"></param>
            public void ReleaseInstance(object instance)
            {
            }

            /// <summary>
            /// 异步加载场景。
            /// </summary>
            /// <param name="sceneAssetName">要加载场景资源的名称。</param>
            /// <returns>异步加载场景句柄</returns>
            public AsyncOperationHandleBase LoadScene(string sceneAssetName)
            {
                return null;
            }

            /// <summary>
            /// 异步卸载场景。
            /// </summary>
            /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
            /// <returns>异步卸载场景句柄</returns>
            public AsyncOperationHandleBase UnloadScene(string sceneAssetName)
            {
                return null;
            }
        }
    }
}