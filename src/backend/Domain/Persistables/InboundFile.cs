using System;

namespace Domain.Persistables
{
    public class InboundFile : IPersistable
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тип файла
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Исходный файл
        /// </summary>
        public string RawContent { get; set; }

        /// <summary>
        /// Распознанная модель данных
        /// </summary>
        public string ParsedContent { get; set; }

        /// <summary>
        /// Авторизация запроса
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Дата и время получения
        /// </summary>
        public DateTime ReceivedAtUtc { get; set; }
    }
}
