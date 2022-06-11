using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public interface ICommonMappingService<TEntity, TDto>
    {
        ValidateResult DtoToEntity(TDto dto, TEntity entity);

        ValidateResult EntityToDto(TEntity entity, TDto dto);
    }
}
