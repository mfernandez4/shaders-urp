using UnityEngine;

public class MinMax
{
    public float Min { get; private set; }
    public float Max { get; private set; }
    
    public MinMax()
    {
        Min = float.MaxValue; // Set the minimum value to the maximum possible value
        Max = float.MinValue; // Set the maximum value to the minimum possible value
    }
    
    public void AddValue(float value)
    {
        if (value > Max) Max = value; // If the value is greater than the maximum, set the maximum to the value
        if (value < Min) Min = value; // If the value is less than the minimum, set the minimum to the value
    }
}
