using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Артикул
    /// </summary>
    public class Article : IPersistable, ICompanyPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// NART
        /// </summary>
        public string Nart { get; set; }

        /// <summary>
        /// Температурного режима
        /// </summary>
        public string TemperatureRegime { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}