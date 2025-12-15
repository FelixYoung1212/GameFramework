using System;

namespace GameFramework.Resource
{
    /// <summary>
    /// 资源辅助器接口。
    /// </summary>
    public interface IResourceHelper
    {
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <returns>异步加载资源句柄</returns>
        AsyncOperationHandleBase LoadAsset(string assetName);

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="handle">要卸载的资源加载句柄。</param>
        void UnloadAsset(AsyncOperationHandleBase handle);

        /// <summary>
        /// 实例化资源。
        /// </summary>
        /// <param name="asset">要实例化的资源。</param>
        /// <returns>资源实例</returns>
        object Instantiate(object asset);

        /// <summary>
        /// 释放并且销毁实例化资源
        /// </summary>
        /// <param name="instance">资源实例</param>
        /// <param name="asset">原始资源</param>
        public void ReleaseInstance(object instance, object asset);

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <returns>异步加载场景句柄</returns>
        AsyncOperationHandleBase LoadScene(string sceneAssetName);

        /// <summary>
        /// 异步卸载场景。
        /// </summary>
        /// <param name="handle">要卸载场景加载句柄。</param>
        /// <returns>异步卸载场景句柄</returns>
        AsyncOperationHandleBase UnloadScene(AsyncOperationHandleBase handle);
    }
}