using Domain.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Domain.Services.Import
{
    public interface IInvoicesImportService
    {
        byte[] GenerateExcelTemplate();

        OperationDetailedResult ImportFromExcel(Stream fileStream, string fileName);
    }
}
