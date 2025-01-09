using Haondt.Identity.StorageKey;
using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;
using Midas.Domain.Shared.Extensions;
using Midas.Extensions;
using Midas.Persistence.Extensions;
using Midas.UI.Extensions;
using Midas.UI.Shared.Extensions;
using Midas.UI.Shared.ModelBinders;

const string CORS_POLICY = "_midasPolicy";

StorageKeyConvert.DefaultSerializerSettings = new StorageKeySerializerSettings
{
    TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter,
    KeyEncodingStrategy = KeyEncodingStrategy.String
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(o =>
    {
        o.ModelBinderProviders.Insert(0, new AbsoluteDateTimeModelBinderProvider());
    })
    .AddApplicationPart(typeof(Haondt.Web.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(Haondt.Web.BulmaCSS.Extensions.ServiceCollectionExtensions).Assembly)
    .AddApplicationPart(typeof(Midas.Api.Controllers.MidasApiController).Assembly);

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    .AddMidasServices(builder.Configuration)
    .AddMidasDomainServices(builder.Configuration)
    .AddMidasPersistenceServices(builder.Configuration)
    .AddMidasUI(builder.Configuration)
    .AddMidasSharedUI(builder.Configuration);

builder.Services.AddMvc();
builder.Services.AddCors(o => o.AddPolicy(CORS_POLICY, p =>
{
    p.AllowAnyOrigin();
    p.AllowAnyHeader();
}));
builder.Services.AddServerSideBlazor();


var app = builder.Build();

app.UseStaticFiles();
app.UseCors(CORS_POLICY);
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();


app.Run();
