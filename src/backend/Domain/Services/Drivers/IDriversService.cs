using Domain.Persistables;

namespace Domain.Services.Drivers
{
    public interface IDriversService : IDictonaryService<Driver, DriverDto, DriverFilterDto>
    {
    }
}