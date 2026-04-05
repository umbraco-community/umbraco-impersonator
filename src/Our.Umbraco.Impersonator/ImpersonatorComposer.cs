using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;

namespace Our.Umbraco.Impersonator
{
    public class UmbracoCommunityRollbackPreviewerApiComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IOperationIdHandler, CustomOperationHandler>();

            builder.Services.Configure<SwaggerGenOptions>(opt =>
            {
                // Related documentation:
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-swagger-document
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
                // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies

                // Configure the Swagger generation options
                // Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
                // Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
                opt.SwaggerDoc("impersonator", new OpenApiInfo
                {
                    Title = "Impersonator API",
                    Version = "1.0",
                });

                // Enable Umbraco authentication for the "Example" Swagger document
                // PR: https://github.com/umbraco/Umbraco-CMS/pull/15699
                opt.OperationFilter<UmbracoCommunityImpersonatorOperationSecurityFilter>();
            });
        }

        public class UmbracoCommunityImpersonatorOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
        {
            protected override string ApiName => "impersonator";
        }

        // This is used to generate nice operation IDs in our swagger json file
        // So that the gnerated TypeScript client has nice method names and not too verbose
        // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
        public class CustomOperationHandler : OperationIdHandler
        {
            public CustomOperationHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions)
            {
            }

            protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
            {
                return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("Our.Umbraco.Impersonator", comparisonType: StringComparison.InvariantCultureIgnoreCase) is true;
            }

            public override string Handle(ApiDescription apiDescription) => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
        }
    }
}
