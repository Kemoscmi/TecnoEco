var builder=WebApplication.CreateBuilder(args);
if(!builder.Environment.IsDevelopment())throw new InvalidOperationException("Gateway de desarrollo: pendiente autenticación y autorización.");
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app=builder.Build();app.MapGet("/health",()=>Results.Ok(new {status="ok"}));app.MapReverseProxy();app.Run();
