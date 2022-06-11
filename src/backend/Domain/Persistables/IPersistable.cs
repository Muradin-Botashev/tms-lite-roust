using System;

namespace Domain.Persistables
{
    public interface ICompanyPersistable
    {
        Guid? CompanyId { get; }
    }

    public interface IPersistableWithName : IPersistable
    {
        string Name { get; set; }
    }

    public interface IPersistable
    {
        Guid Id { get; set; }
    }
}