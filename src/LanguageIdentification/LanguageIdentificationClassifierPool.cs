using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LanguageIdentification
{
    /// <summary>
    /// <see cref="LanguageIdentificationClassifier"/> 池
    /// </summary>
    public class LanguageIdentificationClassifierPool
    {
        #region Private 字段

        private readonly ConcurrentBag<ILanguageIdentificationClassifier> _items = new();
        private readonly int _maxRetainCount;
        private int _count;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 默认的 <see cref="LanguageIdentificationClassifier"/> 池
        /// </summary>
        public static LanguageIdentificationClassifierPool Default { get; private set; } = new(Environment.ProcessorCount);

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="LanguageIdentificationClassifierPool"/>
        /// </summary>
        /// <param name="maxRetainCount">最大保留大小</param>
        public LanguageIdentificationClassifierPool(int maxRetainCount)
        {
            if (maxRetainCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetainCount), "min value is 1.");
            }
            _maxRetainCount = maxRetainCount;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 设置默认池的最大保留大小
        /// </summary>
        /// <param name="maxRemainCount"></param>
        public static void SetDefaultMaxRemainCount(int maxRemainCount)
        {
            Default = new LanguageIdentificationClassifierPool(maxRemainCount);
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            while (_items.TryTake(out var _))
            {
                Interlocked.Decrement(ref _count);
            }
        }

        /// <summary>
        /// 借用一个 <see cref="ILanguageIdentificationClassifier"/>
        /// </summary>
        /// <returns></returns>
        public ILanguageIdentificationClassifier Rent()
        {
            if (_items.TryTake(out var item))
            {
                Interlocked.Decrement(ref _count);
                return item;
            }
            return CreateClassifier();
        }

        /// <summary>
        /// 归还一个 <see cref="ILanguageIdentificationClassifier"/>
        /// </summary>
        /// <param name="item"></param>
        public void Return(ILanguageIdentificationClassifier item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Interlocked.Increment(ref _count) <= _maxRetainCount)
            {
                item.Reset();
                _items.Add(item);
            }
        }

        #endregion Public 方法

        #region Protected 方法

        /// <summary>
        /// 创建分类器
        /// </summary>
        /// <returns></returns>
        protected virtual ILanguageIdentificationClassifier CreateClassifier()
        {
            return new LanguageIdentificationClassifier();
        }

        #endregion Protected 方法
    }
}