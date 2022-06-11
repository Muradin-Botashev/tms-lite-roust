using Domain.Persistables;

namespace Domain.Services.Tariffs
{
    public interface ITariffsService : IDictonaryService<Tariff, TariffDto, TariffFilterDto>
    {
    }
}