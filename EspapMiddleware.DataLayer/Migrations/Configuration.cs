namespace EspapMiddleware.DataLayer.Migrations
{
    using EspapMiddleware.DataLayer.Context;
    using EspapMiddleware.Shared.Entities;
    using EspapMiddleware.Shared.Enums;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EspapMiddlewareDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(EspapMiddlewareDbContext context)
        {
            if (!context.DocumentTypes.Any())
                context.DocumentTypes.AddRange(Enum.GetValues(typeof(DocumentTypeEnum)).Cast<DocumentTypeEnum>()
                    .Select(e => new DocumentType()
                    {
                        Id = e,
                        Description = e.ToString()
                    }));

            if (!context.DocumentStates.Any())
                context.DocumentStates.AddRange(Enum.GetValues(typeof(DocumentStateEnum)).Cast<DocumentStateEnum>()
                    .Select(e => new DocumentState()
                    {
                        Id = e,
                        Description = e.ToString()
                    }));

            if (!context.DocumentActions.Any())
                context.DocumentActions.AddRange(Enum.GetValues(typeof(DocumentActionEnum)).Cast<DocumentActionEnum>()
                    .Select(e => new DocumentAction()
                    {
                        Id = e,
                        Description = e.ToString()
                    }));

            if (!context.DocumentMessagesTypes.Any())
                context.DocumentMessagesTypes.AddRange(Enum.GetValues(typeof(DocumentMessageTypeEnum)).Cast<DocumentMessageTypeEnum>()
                    .Select(e => new DocumentMessageType()
                    {
                        Id = e,
                        Description = e.ToString()
                    }));

            if (!context.RequestLogTypes.Any())
                context.RequestLogTypes.AddRange(Enum.GetValues(typeof(RequestLogTypeEnum)).Cast<RequestLogTypeEnum>()
                    .Select(e => new RequestLogType()
                    {
                        Id = e,
                        Description = e.ToString()
                    }));

            base.Seed(context);
        }
    }
}
