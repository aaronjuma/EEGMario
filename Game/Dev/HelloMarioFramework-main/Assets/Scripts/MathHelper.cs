using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MathHelper
{
    public static float Average(List<float> arr) {
        int n = arr.Count;
        float sum = 0;
        for(int i = 0; i < n; ++i) {
            sum += arr[i];
        }
        return sum;
    }

    public static float StandardDeviation(List<float> arr) {
        float avg = Average(arr);
        int n = arr.Count;
        float sum = 0;
        for(int i = 0; i < n; ++i) {
            sum += Mathf.Pow(arr[i] - avg, 2);
        }
        sum = Mathf.Sqrt(sum/n);
        return sum;
    }
}