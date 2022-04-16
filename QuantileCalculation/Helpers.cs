using Microsoft.AspNetCore.Mvc;

namespace QuantileCalculation
{
    public static class Helpers
    {
        public static decimal CalculateQuantile(decimal[] arr, decimal percentile, int len)
        {
            if (len == 1)
                return arr[0];
            Array.Sort(arr);
            if (percentile >= 100.0m)
                return arr[len - 1];

            decimal n = (len - 1) * percentile / 100.0m + 1;
            if (n == 1m)
                return arr[0];
            else
            {
                if (n == len)
                    return arr[len - 1];
                else
                {
                    int k = (int)Math.Round(n);
                    decimal d = n - k;
                    return arr[k - 1] + d * (arr[k] - arr[k - 1]);
                }
            }
        }
    }
}
