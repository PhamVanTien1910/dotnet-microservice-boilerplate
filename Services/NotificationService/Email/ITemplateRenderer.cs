namespace NotificationService.Email
{
    public interface ITemplateRenderer
    {
        Task<string> RenderAsync(string templateName, IDictionary<string, string> model, CancellationToken cancellationToken = default);
    }
}
