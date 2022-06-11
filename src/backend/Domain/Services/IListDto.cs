using System.Collections.Generic;

namespace Domain.Services
{
    public interface IListDto : IDto
    {
        IEnumerable<string> Backlights { get; set; }
    }
}
