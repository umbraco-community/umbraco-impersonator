using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.Impersonator.Core.Composing;

public class ImpersonatorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ImpersonatorApiSwaggerGenOptions>();
    }
}

public class ImpersonatorApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc(
            Constants.ApiName,
            new OpenApiInfo
            {
                Title = Constants.ApiTitle,
                Version = Constants.ApiVersion
            }
        );

        options.OperationFilter<ImpersonatorApiOperationSecurityFilter>();
    }
}

public class ImpersonatorApiOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => Constants.ApiName;
}