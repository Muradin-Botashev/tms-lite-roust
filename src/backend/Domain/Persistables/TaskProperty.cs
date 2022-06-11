using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Параметры запуска таски по расписанию
    /// </summary>
    public class TaskProperty : IPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>   
        public Guid Id { get; set; }

        /// <summary>
        /// Наименование таски (как в статье https://artlogics.atlassian.net/wiki/spaces/TL/pages/1105952861)
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Параметры в формате Key1=Value1;Key2=Value2
        /// </summary>
        public string Properties { get; set; }
    }
}
