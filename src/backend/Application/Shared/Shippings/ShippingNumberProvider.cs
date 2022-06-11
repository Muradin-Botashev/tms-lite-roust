namespace Application.Shared.Shippings
{
    public static class ShippingNumberProvider
    {
        private static object _lock = new object();
        private static int _lastIndex = 0;

        public static string GetLastShippingNumber()
        {
            return string.Format("SH{0:000000}", _lastIndex);
        }

        public static string GetNextShippingNumber()
        {
            lock (_lock)
            {
                ++_lastIndex;
                return GetLastShippingNumber();
            }
        }

        public static void InitLastNumber(int shippingsCount)
        {
            lock (_lock)
            {
                _lastIndex = shippingsCount;
            }
        }
    }
}
