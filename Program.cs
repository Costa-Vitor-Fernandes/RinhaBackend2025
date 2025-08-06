using System.Text.Json;
using RinhaBackend2025.Repositories;
using RinhaBackend2025.Services;
using RinhaBackend2025.Clients;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços ao contêiner
builder.Services.AddControllers();

// Conexão com o Redis. Usamos 'redis' como hostname para o Docker Compose.
// Adicionamos 'abortConnect=false' para permitir que o cliente continue tentando a conexão.
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("redis:6379,abortConnect=false"));

// Adiciona as classes de serviço, cliente e repositório
builder.Services.AddSingleton<RedisRepository>();
builder.Services.AddSingleton<PaymentsProcessorClient>();
builder.Services.AddSingleton<PaymentsService>();
builder.Services.AddHttpClient<PaymentsProcessorClient>(client =>
{
    // Opcional, mas útil para definir um tempo limite global.
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
