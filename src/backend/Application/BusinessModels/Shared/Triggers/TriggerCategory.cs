using System;

namespace Application.BusinessModels.Shared.Triggers
{
    /*
     * 
       Категория триггера определяет порядок его запуска:
        - триггеры одной категории запускаются в рамках одной "сессии" между получениями списка изменений с сохранением данных в базу
        - категории запускаются в порядке значения их номера

       Для корректности работы триггеров нужно соблюдать следующие правила:
        - триггеры не должны менять значения полей, используемых в триггерах той же или более ранней категории согласно порядка запуска

     */
    public enum TriggerCategory
    {
        Preparation = 0,

        UpdateFields = 20,

        SyncFields = 40,

        Synchronization = 60,

        Calculation = 100,

        PostUpdates = 200
    }

    public class TriggerCategoryAttribute : Attribute
    { 
        public TriggerCategory Category { get; private set; }

        public TriggerCategoryAttribute(TriggerCategory category)
        {
            Category = category;
        }
    }
}
