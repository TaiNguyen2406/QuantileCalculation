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

        [HttpPost("AddorUpdate")]
        public IActionResult AddorUpdate(Pool data)
        {
            if (_repository.Any(data.PoolId))
            {
                var updatedItem = _repository.Get(data.PoolId);
                return UpdateData(data, updatedItem.PoolId);
            }
            else
            {
                return CreateData(data);
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
            var quantile = Helpers.CalculateQuantile(arr, request.Percentile, arr.Length);
            var result = new CalculateDto
            {
                PoolId = request.PoolId,
                CalculatedValue = quantile,
                TotalCount = arr.Length
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

        private IActionResult CreateData(Pool data)
        {
            data = _repository.Create(data);
            var poolDto = new PoolDto
            {
                PoolId = data.PoolId,
                PoolValues = data.PoolValues,
                Status = Status.Inserted
            };
            return Ok(poolDto);
        }

        private IActionResult UpdateData(Pool data, int updatedId)
        {
            var updatedData = _repository.Append(data, updatedId);
            var poolDto = new PoolDto
            {
                PoolId = updatedData.PoolId,
                PoolValues = updatedData.PoolValues,
                Status = Status.Appended
            };
            _cache.Remove(data.PoolId);
            return Ok(poolDto);
        }
    }
}