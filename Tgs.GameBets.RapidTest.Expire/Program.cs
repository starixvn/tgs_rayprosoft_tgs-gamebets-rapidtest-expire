using System.Net.Mime;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NATS.Client.Core;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameBets.RapidTest.Expire;

using Tgs.GameBets.RapidTest.Expire.Common;
using Tgs.GameBets.RapidTest.Expire.Core;
using Tgs.GameBets.RapidTest.Expire.DependencyInjection;
using Tgs.GameBets.RapidTest.Expire.Messages;
using Tgs.MessageQueue.Nats.Serializers;

var filterInstrumentations = new[] { "html", "ico", "png", "jpg" };

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack();
		});
	});

// Add services to the container.
builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers(options =>
	{
		options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
		options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
	})
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	foreach (var xmlFileName in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
	{
		c.IncludeXmlComments(xmlFileName);
	}
});

// Learn more about configuring Hangfire
builder
	.Services
	.AddHangfire((sp, config) =>
	{
		var systemConfig = sp.GetRequiredService<IConfiguration>();
		var connectionString = builder.Configuration.GetConnectionString("Redis")!;
		var options = new RedisStorageOptions
		{
			Prefix = "gamebets-rapidtest-expire-schedule:hangfire:"
		};
		_ = config
		.UseRedisStorage(connectionString, options)
		.WithJobExpirationTimeout(TimeSpan.FromMinutes(10));
	})
	.AddHangfireServer();

builder.Services
	.AddOptions<RedisOptions>()
	.Configure((RedisOptions options, IServiceProvider sp) =>
	{
		var configuration = sp.GetRequiredService<IConfiguration>();
		options.ConnectionString = configuration.GetConnectionString("Redis")!;
	})
	.Services
	.AddOptions<PolicySettingsOptions>()
	.Configure((options) => builder.Configuration.GetSection("PolicySettings").Bind(options))
	.Services
	.AddSingleton<RedisConnectionManager>()
	.AddGameBetsRapidTestExpireCore()
	.AddGameBetsRapidTestExpireRepositories(
		dbContextOptions: (sp, dbBuilder) =>
		{
			var config = sp.GetRequiredService<IConfiguration>();
			_ = dbBuilder.UseNpgsql(config.GetConnectionString("Tgs"));
		},
		redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database)
	.AddGameBetsClient(httpClientBuilder => httpClientBuilder.ConfigureHttpClient((sp, http) =>
	{
		var config = sp.GetRequiredService<IConfiguration>();
		http.BaseAddress = config.GetValue<Uri>("GameBetsRapidTestExpireConnectionStrings:GameBetsClientBaseUrl");
	}));

builder.Services
	.AddKeyedSingleton("Nats", (sp, _) => new NatsConnection(NatsOpts.Default with
	{
		Url = builder.Configuration.GetConnectionString("Nats")!,
		SerializerRegistry = TgsNatsSerializerRegistration.Default
	}))
	.AddMessageQueue()
	.AddNatsMessageQueue(config => config
	.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
	.AddHandler<RapidTestExpireMessageHandler>(GameBetsRapidTestExpireNatsConst.GameBetsRapidTestExpireRequest, GameBetsRapidTestExpireNatsConst.GameBetsRapidTestExpireRequest))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange(GameBetsRapidTestExpireNatsConst.GameBetsRapidTestExpireRequest);

builder.Services
	.AddOpenTelemetry()
	.WithTracing(builder => builder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddSource("Valhalla.*")
		.AddSource("MediatR")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddGrpcClientInstrumentation()
		.AddGrpcCoreInstrumentation()
		.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
	.SetResourceBuilder(ResourceBuilder.CreateDefault()
	.AddEnvironmentVariableDetector())
	.AddRuntimeInstrumentation()
	.AddAspNetCoreInstrumentation()
	.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !httpContext.Request.Path.StartsWithSegments("/hangfire")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !filterInstrumentations.Any(ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
	.AddInterceptionService()
	.AddServiceUtiltiyInterceptors();

builder.Services.AddHealthChecks();

var app = builder.Build();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

app.UseHangfireDashboard(
	"/hangfire",
	new DashboardOptions()
	{
		Authorization =
		[
			new IgnoreHangfireAuthorizationFilter()
		]
	});

_ = app.UseAuthorization();

_ = app.MapControllers();

app.MapHealthChecks("/healthz");

_ = app.UseHangfireRegisterAsync();

_ = app.UseRegister();

app.Run();
