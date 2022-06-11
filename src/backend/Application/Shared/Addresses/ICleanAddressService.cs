namespace Application.Shared.Addresses
{
    public interface ICleanAddressService
    {
        CleanAddressDto CleanAddress(string address);
    }
}