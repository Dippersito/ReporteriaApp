using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Reporteria.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la conexión a la base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configurar MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// 3. Agregar servicios para controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HEMOS QUITADO EL if (app.Environment.IsDevelopment())
// Ahora Swagger se activará siempre.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Esta línea es opcional, pero hace que Swagger sea la página de inicio
    // para que no tengas que escribir /swagger manualmente.
    c.RoutePrefix = string.Empty; 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reporteria API V1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// ESTE ES UN COMENTARIO DE PRUEBA PARA ACTIVAR GITHUB ACTIONS
app.Run();