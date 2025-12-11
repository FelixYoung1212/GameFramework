namespace GameFramework.Resource
{
    /// <summary>
    /// 异步加载资源接口
    /// </summary>
    public interface IAsyncOperation
    {
        /// <summary>
        /// 异步加载资源进度
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 异步加载资源是否结束
        /// </summary>
        bool IsDone { get; }
    }
}