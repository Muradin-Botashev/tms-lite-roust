using Application.Shared.Shippings;
using Application.Shared.Triggers;
using DAL.Services;
using Domain.Enums;
using Domain.Persistables;
using Domain.Services.Pooling;
using Domain.Services.Pooling.Models;
using Domain.Shared;
using System.Linq;

namespace Application.Services.Pooling
{
    public class InputReservationsService : IInputReservationsService
    {
        private readonly ICommonDataService _dataService;
        private readonly ITriggersService _triggersService;
        private readonly IShippingActionService _shippingActionService;

        public InputReservationsService(
            ICommonDataService dataService, 
            ITriggersService triggersService, 
            IShippingActionService shippingActionService)
        {
            _dataService = dataService;
            _triggersService = triggersService;
            _shippingActionService = shippingActionService;
        }

        public void SaveData(ReservationDto dto)
        {
            if (dto?.Orders == null || !dto.Orders.Any())
            {
                var result = new ValidateResult("Не обнаружено заказов в запросе", true);
                throw new ValidationException(result);
            }

            var shippingNumber = dto.Orders.Select(x => x.ConsignorNumber)
                                           .Where(x => !string.IsNullOrEmpty(x))
                                           .FirstOrDefault();
            if (string.IsNullOrEmpty(shippingNumber))
            {
                var result = new ValidateResult("Не обнаружено номера перевозки в запросе", true);
                throw new ValidationException(result);
            }

            var shipping = _dataService.GetDbSet<Shipping>()
                                        .Where(x => x.ShippingNumber == shippingNumber)
                                        .FirstOrDefault();
            if (shipping == null)
            {
                throw new NotFoundException();
            }

            var orders = _dataService.GetDbSet<Order>().Where(x => x.ShippingId == shipping.Id).ToList();

            shipping.VehicleNumber = dto.Vehicle?.RegistrationNumber ?? shipping.VehicleNumber;
            shipping.TrailerNumber = dto.Vehicle?.TrailerRegistrationNumber ?? shipping.TrailerNumber;
            shipping.VehicleMake = dto.Vehicle?.Model ?? shipping.VehicleMake;
            shipping.DriverName = dto.Driver?.Name ?? shipping.DriverName;
            shipping.DriverPhone = dto.Driver?.Phone ?? shipping.DriverPhone;
            shipping.IsPooling = dto.ShippingType == "Pooling";

            if (dto.Status == "Delivered")
            {
                shipping.Status = ShippingState.ShippingCompleted;
            }
            else if (dto.Status == "Rejected")
            {
                _shippingActionService.RejectShippingRequest(shipping, orders);
            }

            foreach (var order in orders)
            {
                order.VehicleNumber = shipping.VehicleNumber;
                order.TrailerNumber = shipping.TrailerNumber;
                order.VehicleMake = shipping.VehicleMake;
                order.DriverName = shipping.DriverName;
                order.DriverPhone = shipping.DriverPhone;
                order.IsPooling = shipping.IsPooling;
                order.OrderShippingStatus = shipping.Status;
            }

            _triggersService.Execute(false);
            _dataService.SaveChanges();
        }
    }
}
