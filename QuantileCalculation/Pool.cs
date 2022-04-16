using System.Collections.Generic;

namespace QuantileCalculation
{
    public class Pool
    {
        public int PoolId { get; set; }
        public List<decimal> PoolValues { get; set; }
    }

    public class PoolDto
    {
        public int PoolId { get; set; }
        public List<decimal> PoolValues { get; set; }
        public string Status { get; set; }
    }

    public class CalculateRequest
    {
        public int PoolId { get; set; }
        public decimal Percentile { get; set; }
    }

    public class CalculateDto
    {
        public int PoolId { get; set; }
        public decimal CalculatedValue { get; set; }
        public decimal TotalCount { get; set; }
    }
}