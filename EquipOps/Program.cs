using EquipOps;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Controllers + Swagger
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------
// Core Services
// ----------------------------
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// ----------------------------
// CORS
// ----------------------------
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:5174")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

// ----------------------------
// Auth
// ----------------------------
builder.Services.AddAuthorization();

// ----------------------------
// App Services
// ----------------------------
builder.Services.WithRegisterServices();

// ----------------------------
// Build App
// ----------------------------
var app = builder.Build();

// ----------------------------
// Middleware
// ----------------------------
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "EquipOps API V1");
		c.RoutePrefix = "swagger";
	});
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
