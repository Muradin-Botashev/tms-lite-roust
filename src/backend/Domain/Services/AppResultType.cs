using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    /// <summary>
    /// Тип сообщения
    /// </summary>
    public enum AppResultType
    {
        /// <summary>
        /// Простое сообщение
        /// </summary>
        SimpleNotification = 0,

        /// <summary>
        /// Окно с детальной информацией
        /// </summary>
        DetailedNotification = 1
    }
}
