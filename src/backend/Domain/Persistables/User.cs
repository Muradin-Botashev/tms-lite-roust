using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    public class User : IPersistableWithName, ICompanyPersistable
    {
        public Guid Id { get; set; }

        [ReferenceType(typeof(Role))]
        public Guid RoleId { get; set; }

        [SortKey(nameof(Persistables.Role.Name))]
        public Role Role { get; set; }

        public string Login { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string FieldsConfig { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }

        public int[] Notifications { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}