using Domain.Extensions;

namespace Domain.Enums
{
    /// <summary>
    /// Статус ТС
    /// </summary>
    public enum VehicleState
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [StateColor(AppColor.Black), OrderNumber(0)]
        VehicleEmpty = 0,

        /// <summary>
        /// Ожидает ТС
        /// </summary>
        [StateColor(AppColor.Grey), OrderNumber(1)]
        VehicleWaiting = 1,

        /// <summary>
        /// ТС прибыло
        /// </summary>
        [StateColor(AppColor.Orange), OrderNumber(2)]
        VehicleArrived = 2,

        /// <summary>
        /// ТС убыло
        /// </summary>
        [StateColor(AppColor.Green), OrderNumber(3)]
        VehicleDepartured = 3
    }
}
