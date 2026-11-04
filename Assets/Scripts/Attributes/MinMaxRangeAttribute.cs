using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class MinMaxRangeAttribute : PropertyAttribute
{
    public float minLimit;
    public float maxLimit;

    public MinMaxRangeAttribute(float minLimit, float maxLimit)
    {
        this.minLimit = minLimit;
        this.maxLimit = maxLimit;
    }
}

[Serializable]
public struct MinMaxRange
{
    [SerializeField] private float m_Min;
    [SerializeField] private float m_Max;

    public float Min => m_Min;
    public float Max => m_Max;

    public MinMaxRange(float min, float max)
    {
        m_Min = min;
        m_Max = max;
    }
}