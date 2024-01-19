using System;

namespace LanguageIdentification;

internal sealed class ConfidenceCounter
{
    #region Private 字段

    private readonly int _classesCount;
    private readonly IFixedSizeArrayPool<float> _confidenceArrayPool;
    private readonly int[] _counts;
    private readonly int[] _dense;
    private readonly int _featuresCount;
    private readonly LanguageIdentificationModel _langIdModel;
    private readonly float[] _origin_nb_pc;
    private readonly float[] _origin_nb_ptc;
    private readonly int[] _sparse;
    private int _elementsCount;

    #endregion Private 字段

    #region Public 构造函数

    public ConfidenceCounter(IFixedSizeArrayPool<float> confidenceArrayPool, LanguageIdentificationModel langIdModel)
    {
        _confidenceArrayPool = confidenceArrayPool ?? throw new ArgumentNullException(nameof(confidenceArrayPool));
        _langIdModel = langIdModel ?? throw new ArgumentNullException(nameof(langIdModel));

        _featuresCount = _langIdModel.FeaturesCount;
        _classesCount = _langIdModel.ClassesCount;

        _sparse = new int[_featuresCount + 1];
        _dense = new int[_featuresCount];
        _counts = new int[_featuresCount];

        _origin_nb_pc = _langIdModel.nb_pc;
        _origin_nb_ptc = _langIdModel.nb_ptc;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Clear()
    {
        _elementsCount = 0;
    }

    public void Increment(int key)
    {
        var index = _sparse[key];
        if (index < _elementsCount
            && _dense[index] == key)
        {
            _counts[index]++;
        }
        else
        {
            index = _elementsCount++;
            _sparse[key] = index;
            _dense[index] = key;
            _counts[index] = 1;
        }
    }

    public float[] NaiveBayesClassConfidence()
    {
        // Reuse scratch and initialize with nb_pc
        var pdc = _confidenceArrayPool.Rent();

        Array.Copy(_origin_nb_pc, 0, pdc, 0, pdc.Length);

        // Compute the partial log-probability of the document given each class.
        for (int i = 0, fi = 0; i < _classesCount; i++, fi += _featuresCount)
        {
            float v = 0;
            for (int j = 0; j < _elementsCount; j++)
            {
                int index = _dense[j];
                v += _counts[j] * _origin_nb_ptc[fi + index];
            }
            pdc[i] += v;
        }

        return pdc;
    }

    #endregion Public 方法
}
