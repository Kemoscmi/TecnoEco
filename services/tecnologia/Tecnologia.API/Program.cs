using Microsoft.EntityFrameworkCore;
using Tecnologia.Application;
using Tecnologia.Domain;
using Tecnologia.Infrastructure;
using Tecnologia.Shared;
var builder=WebApplication.CreateBuilder(args);
// Base de desarrollo: no exponer públicamente hasta implementar autenticación y autorización.
if(!builder.Environment.IsDevelopment())throw new InvalidOperationException("Esta base solo admite Development hasta implementar autenticación y autorización.");
var connection=builder.Configuration.GetConnectionString("TecnologiaDB")??throw new InvalidOperationException("Configura ConnectionStrings__TecnologiaDB para la nueva base independiente.");
builder.Services.AddDbContext<TechnologyDbContext>(o=>o.UseMySql(connection,new MySqlServerVersion(new Version(8,0,36))));
builder.Services.AddScoped<ITechnologyRepository,TechnologyRepository>();builder.Services.AddScoped<TechnologyService>();
var app=builder.Build();
app.Use(async(ctx,next)=>{try{await next(ctx);}catch(ArgumentException e){ctx.Response.StatusCode=400;await ctx.Response.WriteAsJsonAsync(new ApiError(e.Message));}catch(KeyNotFoundException e){ctx.Response.StatusCode=404;await ctx.Response.WriteAsJsonAsync(new ApiError(e.Message));}});
app.MapGet("/health",()=>Results.Ok(new {status="ok",service="Tecnologia.API"}));
app.MapGet("/api/tecnologia",(TechnologyService service,CancellationToken ct)=>service.Load(ct));
app.MapPost("/api/tecnologia/tickets",(TicketInput input,TechnologyService service,CancellationToken ct)=>service.Create(input,ct));
app.MapPatch("/api/tecnologia/tickets/{id}/status",(string id,StatusInput input,TechnologyService service,CancellationToken ct)=>service.ChangeStatus(id,input,ct));
app.MapPost("/api/tecnologia/activities",(ActivityInput input,TechnologyService service,CancellationToken ct)=>service.AddActivity(input,ct));
app.Run();
