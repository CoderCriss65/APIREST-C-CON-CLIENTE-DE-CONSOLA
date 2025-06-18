// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => {
    options.LogToStandardErrorThreshold = LogLevel.Information;
});
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

var app = builder.Build();

// Middleware para loggear todas las solicitudes y respuestas
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    // Capturar solicitud entrante
    context.Request.EnableBuffering();  // Permite leer el cuerpo varias veces

    var request = context.Request;
    var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
    request.Body.Position = 0;  // Rebobinar para que el controlador pueda leerlo

    logger.LogInformation($"\n[SOLICITUD] {request.Method} {request.Path}");
    logger.LogInformation($"Headers: {string.Join(", ", request.Headers)}");
    logger.LogInformation($"Body: {requestBody}");

    // Capturar la respuesta
    var originalBodyStream = context.Response.Body;
    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    await next.Invoke();

    // Loggear respuesta
    logger.LogInformation($"\n[RESPUESTA] Status: {context.Response.StatusCode}");
    responseBody.Seek(0, SeekOrigin.Begin);
    var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
    responseBody.Seek(0, SeekOrigin.Begin);
    await responseBody.CopyToAsync(originalBodyStream);

    logger.LogInformation($"Body: {responseContent}");
    logger.LogInformation($"Headers: {string.Join(", ", context.Response.Headers)}");
});






app.MapControllers();

// Escuchar en todas las interfaces (LAN)
app.Run("http://0.0.0.0:5000");  // Cambia 5000 por tu puerto