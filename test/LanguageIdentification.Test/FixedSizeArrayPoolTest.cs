using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using LanguageIdentification.Internal;

namespace LanguageIdentification.Test;

[TestClass]
public class FixedSizeArrayPoolTest
{
    #region Public 方法

    [TestMethod]
    [DataRow(10, 2)]
    [DataRow(10, 4)]
    [DataRow(10, 8)]
    [DataRow(10, 16)]
    [DataRow(10, 32)]
    public void ParallelTest(int arraySize, int retainSize)
    {
        Run(arraySize, retainSize);
    }

    #endregion Public 方法

    #region Private 方法

    private static void Run(int arraySize, int retainSize)
    {
        const int Count = 10_000_000;

        var pool = new FixedSizeArrayPool<byte>(arraySize, retainSize);

        var hashSet = new HashSet<int>();

        Parallel.For(0, Count, new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, _ =>
        {
            var array = pool.Rent();
            Assert.AreEqual(arraySize, array.Length);
            lock (hashSet)
            {
                hashSet.Add(RuntimeHelpers.GetHashCode(array));
            }
            pool.Return(array);
        });

        Console.WriteLine($"Count:{Count}, hashSet.Count: {hashSet.Count}, pool.Count:{pool.Count}");

        Assert.IsTrue(hashSet.Count < Count);
        Assert.IsTrue(retainSize >= pool.Count);
    }

    #endregion Private 方法
}
