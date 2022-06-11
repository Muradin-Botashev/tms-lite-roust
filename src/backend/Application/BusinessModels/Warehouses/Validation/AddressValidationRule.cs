using Application.BusinessModels.Shared.Validation;
using Application.Shared.Addresses;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Translations;
using Domain.Services.Warehouses;
using Domain.Shared;
using Domain.Shared.UserProvider;
using System.Collections.Generic;
using System.Linq;

namespace Application.BusinessModels.Warehouses.Validation
{
    public class AddressValidationRule : IValidationRule<WarehouseDto, Warehouse>
    {
        private readonly IUserProvider _userProvider;
        private readonly ICleanAddressService _cleanAddressService;

        public AddressValidationRule(IUserProvider userProvider, ICleanAddressService cleanAddressService)
        {
            _userProvider = userProvider;
            _cleanAddressService = cleanAddressService;
        }

        public bool IsApplicable(string fieldName)
        {
            return fieldName?.ToLower() == nameof(WarehouseDto.Address).ToLower();
        }

        public DetailedValidationResult Validate(WarehouseDto dto, Warehouse entity)
        {
            if (dto.Address == entity.Address)
            {
                return null;
            }

            var lang = _userProvider.GetCurrentUser()?.Language;
            var address = _cleanAddressService.CleanAddress(dto.Address);

            var fields = new List<string>();
            if (string.IsNullOrEmpty(address?.PostalCode))
            {
                fields.Add(nameof(address.PostalCode).ToLowerFirstLetter().Translate(lang));
            }
            if (string.IsNullOrEmpty(address?.Region))
            {
                fields.Add(nameof(address.Region).ToLowerFirstLetter().Translate(lang));
            }
            if (string.IsNullOrEmpty(address?.City))
            {
                fields.Add(nameof(address.City).ToLowerFirstLetter().Translate(lang));
            }
            if (string.IsNullOrEmpty(address?.Street))
            {
                fields.Add(nameof(address.Street).ToLowerFirstLetter().Translate(lang));
            }
            if (string.IsNullOrEmpty(address?.House))
            {
                fields.Add(nameof(address.House).ToLowerFirstLetter().Translate(lang));
            }

            if (fields.Any())
            {
                return new DetailedValidationResult(
                    nameof(dto.Address),
                    "IncompleteAddressError".Translate(lang, dto.Address, string.Join(", ", fields)),
                    ValidationErrorType.InvalidValueFormat);
            }
            else
            {
                return null;
            }
        }
    }
}
