public static class TinyMath
{
    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    public static long Clamp(long value, long min, long max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    public static double Clamp(double value, double min, double max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    public static int Clamp01(int value) => Clamp(value, 0, 1);

    public static float Clamp01(float value) => Clamp(value, 0, 1);

    public static double Clamp01(double value) => Clamp(value, 0, 1);
}
