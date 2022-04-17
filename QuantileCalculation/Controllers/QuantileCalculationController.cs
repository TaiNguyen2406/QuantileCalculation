using Microsoft.AspNetCore.Mvc;
using QuantileCalculation.Repository;

namespace QuantileCalculation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuantileCalculationController : ControllerBase
    {
        private readonly IQuantileCalculationRepository _repository;
        private static readonly Dictionary<int, Dictionary<decimal, CalculateDto>> _cache = new();

        public QuantileCalculationController(IQuantileCalculationRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("AddorAppend")]
        public IActionResult AddorAppend(Pool data)
        {
            if (_repository.Any(data.PoolId))
            {
                var result = AppendData(data);
                return Ok(result);
            }
            else
            {
                var result = CreateData(data);
                return Ok(result);
            }
        }

        [HttpPost("Calculate")]
        public IActionResult Calculate(CalculateRequest request)
        {
            var level2 = _cache.FirstOrDefault(x => x.Key == request.PoolId).Value;
            if (level2 != null)
            {
                var cacheItem = level2.FirstOrDefault(x => x.Key == request.Percentile).Value;
                if (cacheItem != null)
                    return Ok(cacheItem);
            }
            var item = _repository.Get(request.PoolId);
            if (item == null)
                return NotFound();

            var arr = item.PoolValues.ToArray();
            var len = arr.Length;
            var quantile = Helpers.CalculateQuantile(arr, request.Percentile, len);
            var result = new CalculateDto
            {
                PoolId = request.PoolId,
                CalculatedValue = quantile,
                TotalCount = len
            };
            if (level2 == null)
            {
                level2 = new Dictionary<decimal, CalculateDto>();
            }
            level2.Add(request.Percentile, result);
            if (_cache.ContainsKey(request.PoolId))
            {
                _cache[request.PoolId] = level2;
            }
            else
            {
                _cache.Add(request.PoolId, level2);
            }

            return Ok(result);
        }

        private PoolDto CreateData(Pool data)
        {
            data = _repository.Create(data);
            var poolDto = new PoolDto
            {
                PoolId = data.PoolId,
                PoolValues = data.PoolValues,
                Status = Status.Inserted
            };
            return poolDto;
           
        }

        private PoolDto AppendData(Pool data)
        {
            var updatedItem = _repository.Get(data.PoolId);
            var updatedData = _repository.Append(data, updatedItem.PoolId);
            var poolDto = new PoolDto
            {
                PoolId = updatedData.PoolId,
                PoolValues = updatedData.PoolValues,
                Status = Status.Appended
            };
            _cache.Remove(data.PoolId);
            return poolDto;
        }
    }
}