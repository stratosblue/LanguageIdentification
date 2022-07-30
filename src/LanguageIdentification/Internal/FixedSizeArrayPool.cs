using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LanguageIdentification.Internal;

internal class FixedSizeArrayPool<T> : IFixedSizeArrayPool<T>
{
    #region Private 字段

    private readonly ConcurrentQueue<T[]> _arrayQueue = new();
    private readonly int _arraySize;
    private readonly int _maxRetainSize;
    private volatile int _currentSize;

    #endregion Private 字段

    #region Internal 属性

    internal int Count => _arrayQueue.Count;

    #endregion Internal 属性

    #region Public 构造函数

    public FixedSizeArrayPool(int arraySize, int maxRetainSize)
    {
        if (arraySize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(arraySize), "Must be greater than 0.");
        }
        if (maxRetainSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetainSize), "Must be greater than 0.");
        }

        _arraySize = arraySize;
        _maxRetainSize = maxRetainSize;
        _currentSize = 0;
    }

    #endregion Public 构造函数

    #region Public 方法

    public T[] Rent()
    {
        if (_arrayQueue.TryDequeue(out var result))
        {
            Interlocked.Add(ref _currentSize, -1);
            return result;
        }
        return new T[_arraySize];
    }

    public void Return(T[] item)
    {
        if (item.Length != _arraySize)
        {
            throw new ArgumentException("array size error.", nameof(item));
        }
        if (_currentSize < _maxRetainSize)
        {
            Interlocked.Add(ref _currentSize, 1);
            _arrayQueue.Enqueue(item);
        }
    }

    #endregion Public 方法
}
