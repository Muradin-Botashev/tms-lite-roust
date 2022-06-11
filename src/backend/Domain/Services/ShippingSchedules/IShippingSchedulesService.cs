using Domain.Persistables;

namespace Domain.Services.ShippingSchedules
{
    public interface IShippingSchedulesService : IDictonaryService<ShippingSchedule, ShippingScheduleDto, ShippingScheduleFilterDto>
    {
    }
}
