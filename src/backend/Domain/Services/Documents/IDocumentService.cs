using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services.Documents
{
    public interface IDocumentService
    {
        ValidateResult CreateDocument(Guid id, DocumentDto dto);

        ValidateResult UpdateDocument(Guid id, Guid documentId, DocumentDto dto);

        IEnumerable<DocumentDto> GetDocuments(Guid id);

        ValidateResult DeleteDocument(Guid id, Guid documentId);
    }
}
