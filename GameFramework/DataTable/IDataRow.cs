//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFramework.DataTable
{
    /// <summary>
    /// 数据表行接口。
    /// </summary>
    public interface IDataRow
    {
        /// <summary>
        /// 获取数据表行的编号。
        /// </summary>
        int Id
        {
            get;
        }
    }
}
