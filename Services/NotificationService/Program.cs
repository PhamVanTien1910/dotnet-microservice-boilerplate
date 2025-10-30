using NotificationService.Email;
using NotificationService.Options;
using NotificationService.Subscription;
using NotificationService.Extensions;
using BuildingBlocks.Messaging.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

// Add structured logging
//builder.Services.AddCustomLogging(builder.Configuration);

// Worker options
builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<RetryOptions>()
    .Bind(builder.Configuration.GetSection(RetryOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<PortalKeySettings>()
    .Bind(builder.Configuration.GetSection(PortalKeySettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add RabbitMQ event bus
builder.Services.AddRabbitMQ("RabbitMQ");

// Email services
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<ITemplateRenderer>(sp =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailOptions>>().Value;
    return new MustacheTemplateRenderer(opt.TemplatePath);
});
builder.Services.AddSingleton<EmailComposer>();

// Register integration event handlers
builder.Services.AddIntegrationEventHandlers();

// Background subscription
builder.Services.AddHostedService<EventSubscriptionService>();

var host = builder.Build();

host.Run();
