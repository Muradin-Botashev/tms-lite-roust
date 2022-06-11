using Application.Shared.Pooling.Models;
using Domain.Persistables;
using Domain.Services.Pooling.Models;
using System.Collections.Generic;

namespace Application.Shared.Pooling
{
    public interface IPoolingApiService
    {
        string Url { get; }

        HttpResult<List<SlotDto>> GetSlots(SlotFilterDto dto, Company company);

        HttpResult<ReservationRequestDto> BookSlot(ReservationRequestDto dto, Company company);

        HttpResult<ReservationRequestDto> UpdateReservation(ReservationRequestDto dto, Company company);

        HttpResult CancelSlot(string id, string number, string foreignId, Company company);

        HttpResult<SlotDto> GetSlot(string slotId, Company company);
    }

}
