using Domain.Services.History;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Services
{
    public class AuditDataService: CommonDataService, IAuditDataService
    {
        private readonly IHistoryService _historyService;

        public AuditDataService(AppDbContext context, IHistoryService historyService) : base(context)
        {
            _historyService = historyService;
        }

        public override void SaveChanges()
        {
            var changes = this.GetChanges();

            foreach (var entity in changes)
            {
                foreach (var change in entity.FieldChanges)
                {
                    _historyService.Save(entity.Entity.Id, "fieldChanged", change.FieldName, change.OldValue, change.NewValue);
                }
            }

            base.SaveChanges();
        }
    }
}
