namespace GameFramework.Resource
{
    /// <summary>
    /// 资源加载器接口。
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadAsset(string assetName, LoadAssetCallbacks loadAssetCallbacks, object userData);
        
        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源。</param>
        void UnloadAsset(object asset);

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadScene(string sceneAssetName, LoadSceneCallbacks loadSceneCallbacks, object userData);

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData);

        // /// <summary>
        // /// 资源加载器轮询。
        // /// </summary>
        // /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        // /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        // void Update(float elapseSeconds, float realElapseSeconds);
    }
}