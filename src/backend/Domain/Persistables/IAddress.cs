namespace Domain.Persistables
{
    public interface IAddress
    {
        string Address { get; set; }
        string PostalCode { get; set; }
        string Region { get; set; }
        string City { get; set; }
        string Street { get; set; }
        string House { get; set; }
    }
}
