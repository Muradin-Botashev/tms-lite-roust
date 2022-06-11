using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public interface ICompanyDto : IDto
    {
        LookUpDto CompanyId { get; set; }
    }
}
