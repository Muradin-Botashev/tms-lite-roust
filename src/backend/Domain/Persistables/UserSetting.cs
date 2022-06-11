using System;

namespace Domain.Persistables
{
    public class UserSetting : IPersistable
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
