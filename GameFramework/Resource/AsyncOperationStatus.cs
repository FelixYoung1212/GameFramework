namespace GameFramework.Resource
{
    /// <summary>
    /// 异步加载资源状态
    /// </summary>
    public enum AsyncOperationStatus
    {
        /// <summary>
        /// 加载中
        /// </summary>
        None,

        /// <summary>
        /// 加载成功
        /// </summary>
        Succeeded,

        /// <summary>
        /// 加载失败
        /// </summary>
        Failed
    }
}