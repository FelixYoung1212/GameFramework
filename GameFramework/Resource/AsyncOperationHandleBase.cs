using System;

namespace GameFramework.Resource
{
    /// <summary>
    /// 异步加载资源句柄基类
    /// </summary>
    public abstract class AsyncOperationHandleBase : IReference
    {
        /// <summary>
        /// 异步加载资源进度
        /// </summary>
        public abstract float Progress { get; protected set; }

        /// <summary>
        /// 异步加载资源状态
        /// </summary>
        public abstract AsyncOperationStatus Status { get; protected set; }

        /// <summary>
        /// 异步资源加载结果
        /// </summary>
        public abstract object Result { get; protected set; }

        /// <summary>
        /// 异步加载资源进度更新事件
        /// </summary>
        public event Action<AsyncOperationHandleBase> OnProgress;

        /// <summary>
        /// 异步加载资源成功事件
        /// </summary>
        public event Action<AsyncOperationHandleBase> OnSucceeded;

        /// <summary>
        /// 异步加载资源失败事件
        /// </summary>
        public event Action<AsyncOperationHandleBase> OnFailed;

        private int m_ReferenceCount = 1;

        private bool m_IsRunning;

        /// <summary>
        /// 减少资源引用计数
        /// </summary>
        internal void DecrementReferenceCount()
        {
            if (m_ReferenceCount <= 0)
            {
                return;
            }

            m_ReferenceCount--;
        }

        /// <summary>
        /// 增加资源引用计数
        /// </summary>
        internal void IncrementReferenceCount()
        {
            m_ReferenceCount++;
        }

        /// <summary>
        /// 资源引用计数
        /// </summary>
        internal int ReferenceCount => m_ReferenceCount;

        /// <summary>
        /// 开始异步加载
        /// </summary>
        internal void Start()
        {
            m_IsRunning = true;
        }

        /// <summary>
        /// 异步加载轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!m_IsRunning)
            {
                return;
            }

            switch (Status)
            {
                case AsyncOperationStatus.None:
                    OnProgress?.Invoke(this);
                    break;
                case AsyncOperationStatus.Succeeded:
                    OnProgress?.Invoke(this);
                    OnSucceeded?.Invoke(this);
                    ClearEvents();
                    m_IsRunning = false;
                    break;
                case AsyncOperationStatus.Failed:
                    OnProgress?.Invoke(this);
                    OnFailed?.Invoke(this);
                    ClearEvents();
                    m_IsRunning = false;
                    break;
                default:
                    throw new GameFrameworkException(Utility.Text.Format("Not supported status '{0}'.", Status));
            }
        }

        private void ClearEvents()
        {
            OnProgress = null;
            OnSucceeded = null;
            OnFailed = null;
        }

        /// <summary>
        /// 清理引用。
        /// </summary>
        public void Clear()
        {
            ClearEvents();
            m_ReferenceCount = 1;
            m_IsRunning = false;
            Status = AsyncOperationStatus.None;
            Progress = 0;
            Result = null;
        }
    }
}