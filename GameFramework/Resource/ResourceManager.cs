namespace GameFramework.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        private string m_ApplicableGameVersion;
        private int m_InternalResourceVersion;
        private AssetLoader m_AssetLoader;
        private SceneLoader m_SceneLoader;

        /// <summary>
        /// 初始化资源管理器的新实例。
        /// </summary>
        public ResourceManager()
        {
            m_ApplicableGameVersion = null;
            m_InternalResourceVersion = 0;
            m_AssetLoader = new AssetLoader();
            m_SceneLoader = new SceneLoader();
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority
        {
            get { return 3; }
        }

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion
        {
            get { return m_ApplicableGameVersion; }
        }

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion
        {
            get { return m_InternalResourceVersion; }
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

            m_AssetLoader.SetResourceHelper(resourceHelper);
            m_SceneLoader.SetResourceHelper(resourceHelper);
        }

        /// <summary>
        /// 资源管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_AssetLoader.Update(elapseSeconds, realElapseSeconds);
            m_SceneLoader.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理资源管理器。
        /// </summary>
        internal override void Shutdown()
        {
            if (m_AssetLoader != null)
            {
                m_AssetLoader.Shutdown();
                m_AssetLoader = null;
            }
            
            if (m_SceneLoader != null)
            {
                m_SceneLoader.Shutdown();
                m_SceneLoader = null;
            }
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <returns>异步加载资源句柄</returns>
        public AsyncOperationHandleBase LoadAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            return m_AssetLoader.LoadAsset(assetName);
        }

        /// <summary>
        /// 实例化资源。
        /// </summary>
        /// <param name="asset">要实例化的资源。</param>
        /// <returns>资源实例</returns>
        public object Instantiate(object asset)
        {
            return m_AssetLoader.Instantiate(asset);
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        public void UnloadAsset(object asset)
        {
            if (asset == null)
            {
                throw new GameFrameworkException("Asset is invalid.");
            }

            m_AssetLoader.UnloadAsset(asset);
        }

        /// <summary>
        /// 释放并且销毁实例化资源
        /// </summary>
        /// <param name="instance">资源实例</param>
        /// <param name="asset">原始资源</param>
        public void ReleaseInstance(object instance, object asset)
        {
            m_AssetLoader.ReleaseInstance(instance, asset);
        }
        
        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <returns>异步加载场景句柄</returns>
        public AsyncOperationHandleBase LoadScene(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            return m_SceneLoader.LoadScene(sceneAssetName);
        }

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        public AsyncOperationHandleBase UnloadScene(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }

            AsyncOperationHandleBase op = m_SceneLoader.UnloadScene(sceneAssetName);
            return op;
        }
    }
}