
using Azure.Data.Tables;
using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.Middleware;
using LibraryGatewayAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace LibraryGatewayAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();
			// to discover all the routes in the controllers, we need the endpoit explorer
			builder.Services.AddEndpointsApiExplorer();

			// this section is fot the Azurite configuration - so that we can access users in the table
			string connectionString = "UseDevelopmentStorage=True;";
			builder.Services.AddSingleton(new TableServiceClient(connectionString));
			// make sure that our AuthService.cs is able to access the table client
			builder.Services.AddScoped<AuthService>();

			// this section deals with configuring authentication and authorization with our custom setup
			var jwtSettings = builder.Configuration.GetSection("JwtSettings");
			var secret = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

			// configure the auth service
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings["Issuer"],
					ValidAudience = jwtSettings["Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(secret)
				};
			});

			// add authorization
			builder.Services.AddAuthorization();

			// configuring swagger, with additional options to allow for us to test token authentication
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Library Swagger Docs",
					Version = "v1"
				});
				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter JWT Token"
				});
			});

			// setup the connection using the HTTPClients back to the Functions from the API
			string functionUrl = "http://localhost:7017/v1/api";
			Uri functionUri = new Uri(functionUrl);

			builder.Services.AddHttpClient<BooksTableClient>(c => c.BaseAddress = functionUri);
			builder.Services.AddHttpClient<DocumentBlobClient>(c => c.BaseAddress = functionUri);
			builder.Services.AddHttpClient<LoanQueueClient>(c => c.BaseAddress = functionUri);

			var app = builder.Build();

			// enable our global error handling middleware
			app.UseMiddleware<GlobalExceptionMiddleware>();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Gateway API");
					c.RoutePrefix = string.Empty;
				});
			}

			app.UseHttpsRedirection();

			// authentication must always come before authorization!
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
