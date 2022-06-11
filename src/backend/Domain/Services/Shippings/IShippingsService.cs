using Domain.Persistables;
using Domain.Services.AppConfiguration;
using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services.Shippings
{
    public interface IShippingsService : IGridService<Shipping, ShippingDto, ShippingFormDto, ShippingSummaryDto, ShippingFilterDto>
    {
        IEnumerable<LookUpDto> FindByNumber(NumberSearchFormDto dto);

        ValidateResult Create(CreateShippingDto dto);
        CreateShippingDto DefaultCreateForm();
        UserConfigurationGridItem GetCreateFormConfiguration();
    }
}