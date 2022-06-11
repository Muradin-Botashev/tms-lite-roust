using Domain.Enums;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.Reports
{
    public class ReportParametersDto
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public bool DeliveryType { get; set; }

        public bool Client { get; set; }

        public bool Daily { get; set; }

        public SortingDto Sort { get; set; }

        public ReportFilterDto Filter { get; set; }
    }

    public class ReportFilterDto
    {
        public string DeliveryType { get; set; }

        public string ClientName { get; set; }

        public string ShippingDate { get; set; }

        public string OrdersCount { get; set; }

        public string PalletsCount { get; set; }

        public string OrderAmountExcludingVAT { get; set; }
    }
}
