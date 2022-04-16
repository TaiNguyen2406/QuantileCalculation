namespace QuantileCalculation.Repository
{
    public interface IQuantileCalculationRepository
    {
        Pool Create(Pool data);
        Pool Append(Pool data, int updatedId);
        Pool Get(int id);
        bool Any(int id);
    }
}
