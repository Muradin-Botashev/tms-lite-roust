using System;

namespace Domain.Persistables
{
    public class CityDistance : IPersistable
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Город от
        /// </summary>
        public string FromCity { get; set; }

        /// <summary>
        /// Город до
        /// </summary>
        public string ToCity { get; set; }

        /// <summary>
        /// Расстояние, м
        /// </summary>
        public decimal? Distance { get; set; }
    }
}
