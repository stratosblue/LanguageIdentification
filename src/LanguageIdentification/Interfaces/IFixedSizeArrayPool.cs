namespace LanguageIdentification;

/// <summary>
/// 固定大小的数组池
/// </summary>
internal interface IFixedSizeArrayPool<T>
{
    #region Public 方法

    /// <summary>
    /// 借用
    /// </summary>
    /// <returns></returns>
    T[] Rent();

    /// <summary>
    /// 归还
    /// </summary>
    /// <param name="item"></param>
    void Return(T[] item);

    #endregion Public 方法
}