using UnityEngine;
using System.Collections.Generic;

public static class Calculator
{
    public static float FindAverage(params float[] numbers)
    {
        if (numbers == null || numbers.Length == 0)
            return 0f;

        float sum = 0f;
        foreach (float n in numbers)
        {
            sum += n;
        }
        return sum / numbers.Length;
    }

    // Average from a List
    public static float FindAverageInList(List<float> values)
    {
        if (values == null || values.Count == 0)
            return 0f;

        float sum = 0f;
        foreach (float v in values)
        {
            sum += v;
        }
        return sum / values.Count;
    }
}
