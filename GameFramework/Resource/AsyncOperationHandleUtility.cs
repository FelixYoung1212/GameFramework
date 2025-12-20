using System.Collections.Generic;

namespace GameFramework.Resource
{
    internal static class AsyncOperationHandleUtility
    {
        /// <summary>
        /// 更新目标句柄列表
        /// </summary>
        /// <param name="handles">目标句柄列表</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal static void UpdateHandles(List<AsyncOperationHandleBase> handles, float elapseSeconds, float realElapseSeconds)
        {
            for (var i = 0; i < handles.Count; i++)
            {
                var handle = handles[i];
                if (handle.IsValid)
                {
                    handle.Update(elapseSeconds, realElapseSeconds);
                }
            }
        }

        /// <summary>
        /// 从目标句柄列表以及字典中移除指定名字的句柄
        /// </summary>
        /// <param name="handleNames">需要移除的句柄名字列表</param>
        /// <param name="handles">目标句柄列表</param>
        /// <param name="nameToHandleMap">目标句柄字典</param>
        internal static void RemoveHandlesByName(List<string> handleNames, List<AsyncOperationHandleBase> handles, Dictionary<string, AsyncOperationHandleBase> nameToHandleMap)
        {
            if (handleNames.Count <= 0)
            {
                return;
            }

            foreach (var handleName in handleNames)
            {
                if (!nameToHandleMap.TryGetValue(handleName, out var handle))
                {
                    continue;
                }

                handles.Remove(handle);
                nameToHandleMap.Remove(handleName);
            }

            handleNames.Clear();
        }

        /// <summary>
        /// 从目标句柄列表以及字典中移除无效的句柄
        /// </summary>
        /// <param name="handles">目标句柄列表</param>
        /// <param name="nameToHandleMap">目标句柄字典</param>
        internal static void RemoveInvalidHandles(List<AsyncOperationHandleBase> handles, Dictionary<string, AsyncOperationHandleBase> nameToHandleMap)
        {
            for (var i = handles.Count - 1; i >= 0; i--)
            {
                var handle = handles[i];
                if (handle.IsValid)
                {
                    continue;
                }

                handles.RemoveAt(i);
                nameToHandleMap.Remove(handle.AssetName);
            }
        }
    }
}