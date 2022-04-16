namespace QuantileCalculation.Repository
{
    public class QuantileCalculationRepository : IQuantileCalculationRepository
    {
        public static readonly HashSet<Pool> _dataItems = new();

        public Pool Create(Pool data)
        {
            _dataItems.Add(data);
            return data;
        }

        public Pool Append(Pool data, int updatedId)
        {
            var updatedItem =  _dataItems.FirstOrDefault(x=>x.PoolId == updatedId);
            updatedItem.PoolValues.AddRange(data.PoolValues);
            return updatedItem;
        }

        public Pool Get(int id)
        {
            var item = _dataItems.FirstOrDefault(x => x.PoolId == id);
            return item;
        }

        public bool Any(int id)
        {
            return _dataItems.Any(x => x.PoolId == id);
        }
    }
}
