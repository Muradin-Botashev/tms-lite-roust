using DAL;
using Domain.Persistables;
using Domain.Services;
using Domain.Services.Documents;
using Domain.Services.History;
using Domain.Shared.UserProvider;
using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Shared
{
    // TODO настроить foreignkeys
    //[Obsolete("Use DocumentService", true)]
    //public abstract class GridWithDocumentsBase<TEntity, TDto, TFormDto, TSummaryDto, TSearchForm> 
    //    : GridService<TEntity, TDto, TFormDto, TSummaryDto, TSearchForm>, 
    //      IGridWithDocuments<TEntity, TDto, TFormDto, TSummaryDto, TSearchForm> 
    //    where TEntity : class, IPersistable, IWithDocumentsPersistable, new() 
    //    where TDto : IDto, new() 
    //    where TFormDto : IDto, new()
    //    where TSearchForm : PagingFormDto
    //{
    //    public ValidateResult CreateDocument(Guid id, DocumentDto dto)
    //    {
    //        TEntity entity = this.dataService.GetById<TEntity>(id);
    //        if (entity == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        bool tryParseFileId = Guid.TryParse(dto.FileId, out Guid fileId);
    //        FileStorage file = dataService.GetDbSet<FileStorage>().FirstOrDefault(x => x.Id == fileId);
    //        if (!tryParseFileId || file == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        Guid? typeId = null;
    //        if (!string.IsNullOrEmpty(dto.TypeId) && Guid.TryParse(dto.TypeId, out Guid tId))
    //        {
    //            typeId = tId;
    //        }

    //        var document = new Document
    //        {
    //            Name = dto.Name,
    //            PersistableId = entity.Id,
    //            FileId = fileId,
    //            TypeId = typeId
    //        };

    //        _historyService.Save(id, "documentAttachedMessage", file.Name);

    //        dataService.GetDbSet<Document>().Add(document);
    //        dataService.SaveChanges();

    //        return new ValidateResult
    //        {
    //            Id = document.Id.ToString()
    //        };
    //    }

    //    public ValidateResult UpdateDocument(Guid id, Guid documentId, DocumentDto dto)
    //    {
    //        TEntity entity = this.dataService.GetById<TEntity>(id);
    //        if (entity == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        bool tryParseFileId = Guid.TryParse(dto.FileId, out Guid fileId);
    //        FileStorage file = dataService.GetDbSet<FileStorage>().FirstOrDefault(x => x.Id == fileId);
    //        if (!tryParseFileId || file == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        Guid? typeId = null;
    //        if (!string.IsNullOrEmpty(dto.TypeId) && Guid.TryParse(dto.TypeId, out Guid tId))
    //        {
    //            typeId = tId;
    //        }

    //        Document document = dataService.GetDbSet<Document>().FirstOrDefault(x => x.Id == documentId);
    //        if (document == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        document.Name = dto.Name;
    //        document.FileId = fileId;
    //        document.TypeId = typeId;

    //        dataService.GetDbSet<Document>().Update(document);
    //        dataService.SaveChanges();

    //        return new ValidateResult();
    //    }

    //    public IEnumerable<DocumentDto> GetDocuments(Guid id)
    //    {
    //        return dataService.GetDbSet<Document>().Where(x => x.PersistableId == id)
    //            .Select(s => new DocumentDto
    //            {
    //                Id = s.Id.ToString(),
    //                Name = s.Name,
    //                FileId = s.FileId.ToString(),
    //                TypeId = s.TypeId.ToString()
    //            })
    //            .ToList();
    //    }

    //    public ValidateResult DeleteDocument(Guid id, Guid documentId)
    //    {
    //        Document document = dataService.GetDbSet<Document>().FirstOrDefault(x => x.Id == documentId);
    //        if (document == null)
    //        {
    //            return new ValidateResult("notFound");
    //        }

    //        FileStorage file = dataService.GetDbSet<FileStorage>().FirstOrDefault(x => x.Id == document.FileId);
    //        dataService.GetDbSet<FileStorage>().Remove(file);

    //        _historyService.Save(id, "documentRemoved", file.Name);

    //        dataService.GetDbSet<Document>().Remove(document);
    //        dataService.SaveChanges();

    //        return new ValidateResult();
    //    }

    //    protected GridWithDocumentsBase(ICommonDataService dataService, IUserIdProvider userIdProvider, IHistoryService historyService) 
    //        : base(dataService, userIdProvider)
    //    {
    //        _historyService = historyService;
    //    }

    //    protected readonly IHistoryService _historyService;
    //}
}
