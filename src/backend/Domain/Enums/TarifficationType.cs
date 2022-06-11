using Domain.Extensions;

namespace Domain.Enums
{
    /// <summary>
    /// Способ тарификации
    /// </summary>
    public enum TarifficationType
    {
        /// <summary>
        /// FTL
        /// </summary>
        [OrderNumber(1)]
        Ftl = 0,

        /// <summary>
        /// LTL
        /// </summary>
        [OrderNumber(2)]
        Ltl = 1,

        /// <summary>
        /// Pooling
        /// </summary>
        [OrderNumber(3)]
        Pooling = 2,

        /// <summary>
        /// Milkrun
        /// </summary>
        [OrderNumber(4)]
        Milkrun = 3,

        /// <summary>
        /// Doubledeck
        /// </summary>
        [OrderNumber(5)]
        Doubledeck = 4
    }
}
