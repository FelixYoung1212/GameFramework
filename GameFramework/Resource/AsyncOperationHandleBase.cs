using System;

namespace GameFramework.Resource
{
    /// <summary>
    /// 异步加载资源句柄基类
    /// </summary>
    public abstract class AsyncOperationHandleBase
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string AssetName { get; }

        /// <summary>
        /// 异步加载资源进度
        /// </summary>
        public abstract float Progress { get; }

        /// <summary>
        /// 异步加载资源状态
        /// </summary>
        public abstract AsyncOperationStatus Status { get; }

        /// <summary>
        /// 异步资源加载结果
        /// </summary>
        public abstract object Result { get; }

        /// <summary>
        /// 异步加载资源错误信息
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// 异步资源加载耗时
        /// </summary>
        public float Duration { get; private set; }

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
        /// 异步加载资源句柄基类构造器
        /// </summary>
        /// <param name="assetName">资源名</param>
        protected AsyncOperationHandleBase(string assetName)
        {
            AssetName = assetName;
        }

        /// <summary>
        /// 开始异步加载
        /// </summary>
        internal void Start()
        {
            if (m_IsRunning)
            {
                return;
            }

            m_IsRunning = true;
            Duration = 0;
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

            Duration += realElapseSeconds;
        }

        private void ClearEvents()
        {
            OnProgress = null;
            OnSucceeded = null;
            OnFailed = null;
        }
    }

    /// <summary>
    /// 异步加载资源句柄泛型基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncOperationHandleBase<T> where T : class
    {
        private readonly AsyncOperationHandleBase m_Op;

        /// <summary>
        /// 异步加载资源句柄泛型基类构造器
        /// </summary>
        /// <param name="op"></param>
        protected AsyncOperationHandleBase(AsyncOperationHandleBase op)
        {
            m_Op = op;
            m_Op.OnProgress += HandleOnProgress;
            m_Op.OnSucceeded += HandleOnSucceeded;
            m_Op.OnFailed += HandleOnFailed;
        }

        private void HandleOnProgress(AsyncOperationHandleBase op)
        {
            OnProgress?.Invoke(this);
        }

        private void HandleOnSucceeded(AsyncOperationHandleBase op)
        {
            OnSucceeded?.Invoke(this);
        }

        private void HandleOnFailed(AsyncOperationHandleBase op)
        {
            OnFailed?.Invoke(this);
        }

        /// <summary>
        /// 资源名
        /// </summary>
        public string AssetName => m_Op.AssetName;

        /// <summary>
        /// 异步加载资源进度
        /// </summary>
        public float Progress => m_Op.Progress;

        /// <summary>
        /// 异步加载资源状态
        /// </summary>
        public AsyncOperationStatus Status => m_Op.Status;

        /// <summary>
        /// 异步资源加载结果
        /// </summary>
        public T Result => m_Op.Result as T;

        /// <summary>
        /// 异步加载资源错误信息
        /// </summary>
        public string ErrorMessage => m_Op.ErrorMessage;

        /// <summary>
        /// 异步资源加载耗时
        /// </summary>
        public float Duration => m_Op.Duration;

        /// <summary>
        /// 异步加载资源进度更新事件
        /// </summary>
        public event Action<AsyncOperationHandleBase<T>> OnProgress;

        /// <summary>
        /// 异步加载资源成功事件
        /// </summary>
        public event Action<AsyncOperationHandleBase<T>> OnSucceeded;

        /// <summary>
        /// 异步加载资源失败事件
        /// </summary>
        public event Action<AsyncOperationHandleBase<T>> OnFailed;

        /// <summary>
        /// 隐式转换符
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static implicit operator AsyncOperationHandleBase<T>(AsyncOperationHandleBase op)
        {
            return new AsyncOperationHandleBase<T>(op);
        }
    }
}