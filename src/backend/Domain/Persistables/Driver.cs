using System;

namespace Domain.Persistables
{
    public class Driver : IPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ФИО
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Водительское удостоверение
        /// </summary>
        public string DriverLicence { get; set; }

        /// <summary>
        /// Серия и номер паспорта
        /// </summary>
        public string Passport { get; set; }

        /// <summary>
        /// Телефон водителя
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// В черном списке
        /// </summary>
        public bool IsBlackList { get; set; }

        /// <summary>
        /// Активность
        /// </summary>
        public bool IsActive { get; set; }
    }
}