using System;
using System.Collections.Generic;
using GameFramework.ObjectPool;

namespace GameFramework.Resource.Addressables
{
    internal sealed partial class AddressablesManager : GameFrameworkModule, IAddressablesManager
    {
        /// <summary>
        /// 加载资源器
        /// </summary>
        private sealed partial class AddressablesLoader
        {
            private const int CachedHashBytesLength = 4;
            
            private readonly AddressablesManager m_ResourceManager;
            private readonly TaskPool<LoadResourceTaskBase> m_TaskPool;
            private readonly Dictionary<object, object> m_AssetToResourceMap;
            private readonly Dictionary<string, object> m_SceneToAssetMap;
            private readonly LoadBytesCallbacks m_LoadBytesCallbacks;
            private readonly byte[] m_CachedHashBytes;
            private IObjectPool<AssetObject> m_AssetPool;
            private IObjectPool<ResourceObject> m_ResourcePool;
            
            /// <summary>
            /// 初始化加载资源器的新实例。
            /// </summary>
            /// <param name="resourceManager">资源管理器。</param>
            public AddressablesLoader(AddressablesManager resourceManager)
            {
                m_ResourceManager = resourceManager;
                m_TaskPool = new TaskPool<LoadResourceTaskBase>();
                m_AssetToResourceMap = new Dictionary<object, object>();
                m_SceneToAssetMap = new Dictionary<string, object>(StringComparer.Ordinal);
                m_LoadBytesCallbacks = new LoadBytesCallbacks(OnLoadBinarySuccess, OnLoadBinaryFailure);
                m_CachedHashBytes = new byte[CachedHashBytesLength];
                m_AssetPool = null;
                m_ResourcePool = null;
            }
        }
    }
}