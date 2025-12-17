//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;

namespace GameFramework
{
    /// <summary>
    /// 数据提供者辅助器接口。
    /// </summary>
    public interface IDataProviderHelper<T>
    {
        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataAssetName">内容资源名称。</param>
        /// <param name="dataAsset">内容资源。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否读取数据成功。</returns>
        bool ReadData(T dataProviderOwner, string dataAssetName, object dataAsset, object userData);

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataAssetName">内容资源名称。</param>
        /// <param name="dataBytes">内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否读取数据成功。</returns>
        bool ReadData(T dataProviderOwner, string dataAssetName, byte[] dataBytes, int startIndex, int length, object userData);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析内容成功。</returns>
        bool ParseData(T dataProviderOwner, string dataString, object userData);

        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析内容成功。</returns>
        bool ParseData(T dataProviderOwner, byte[] dataBytes, int startIndex, int length, object userData);
        
        /// <summary>
        /// 解析数据表行数据源。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataRowString">要解析的数据表行字符串数据。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>解析完成的数据表行数据。</returns>
        IDataRow ParseDataRow(T dataProviderOwner, object dataRowString, object userData);

        /// <summary>
        /// 解析数据表行数据源。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataRowBytes">要解析的数据表行二进制数据。</param>
        /// <param name="startIndex">数据表行二进制流的起始位置。</param>
        /// <param name="length">数据表行二进制流的长度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>解析完成的数据表行数据。</returns>
        IDataRow ParseDataRow(T dataProviderOwner, object dataRowBytes, int startIndex, int length, object userData);

        /// <summary>
        /// 释放内容资源。
        /// </summary>
        /// <param name="dataProviderOwner">数据提供者的持有者。</param>
        /// <param name="dataAsset">要释放的内容资源。</param>
        void ReleaseDataAsset(T dataProviderOwner, object dataAsset);
    }
}
