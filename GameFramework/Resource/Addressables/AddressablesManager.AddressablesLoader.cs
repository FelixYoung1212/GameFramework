namespace GameFramework.Resource.Addressables
{
    internal sealed partial class AddressablesManager : GameFrameworkModule, IAddressablesManager
    {
        /// <summary>
        /// 加载资源器
        /// </summary>
        private sealed partial class AddressablesLoader
        {
            /// <summary>
            /// 初始化加载资源器的新实例。
            /// </summary>
            /// <param name="resourceManager">资源管理器。</param>
            public AddressablesLoader(AddressablesManager resourceManager)
            {

            }
        }
    }
}