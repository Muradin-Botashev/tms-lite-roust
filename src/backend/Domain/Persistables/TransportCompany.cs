using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Транспортная компания
    /// </summary>
    public class TransportCompany : IPersistable, ICompanyPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>    
        public Guid Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Номер доверенности
        /// </summary>
        public string PowerOfAttorneyNumber { get; set; }

        /// <summary>
        /// Дата доверенности
        /// </summary>
        public DateTime? DateOfPowerOfAttorney { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Контактные данные
        /// </summary>
        public string ContactInfo { get; set; }

        /// <summary>
        /// Экспедитор
        /// </summary>
        public string Forwarder { get; set; }

        /// <summary>
        /// Время на рассмотрения заявки
        /// </summary>
        public int? RequestReviewDuration { get; set; }

        /// <summary>
        /// ID ТК
        /// </summary>
        public string PoolingId { get; set; }

        /// <summary>
        /// Активен
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}