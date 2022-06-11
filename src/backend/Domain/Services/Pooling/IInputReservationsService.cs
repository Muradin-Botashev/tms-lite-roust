using Domain.Services.Pooling.Models;

namespace Domain.Services.Pooling
{
    public interface IInputReservationsService
    {
        void SaveData(ReservationDto dto);
    }
}
