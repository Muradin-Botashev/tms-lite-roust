using Domain.Extensions;

namespace Domain.Enums
{
    /// <summary>
    /// Вариант доставки при автогруппировке
    /// </summary>
    public enum AutogroupingType
    {
        /// <summary>
        /// FTL маршрут
        /// </summary>
        [OrderNumber(0)]
        FtlRoute = 0,

        /// <summary>
        /// FTL точка-точка
        /// </summary>
        [OrderNumber(1)]
        FtlDirect = 1,

        /// <summary>
        /// LTL
        /// </summary>
        [OrderNumber(2)]
        Ltl = 2,

        /// <summary>
        /// Pooling
        /// </summary>
        [OrderNumber(3)]
        Pooling = 3,

        /// <summary>
        /// Milkrun
        /// </summary>
        [OrderNumber(4)]
        Milkrun = 4
    }
}
