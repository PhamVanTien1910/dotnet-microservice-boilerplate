using System.Text;

namespace NotificationService.Email
{
    public class MustacheTemplateRenderer : ITemplateRenderer
    {
        private readonly string? _templatePath;

        public MustacheTemplateRenderer(string? templatePath) => _templatePath = templatePath;
        public async Task<string> RenderAsync(string templateName, IDictionary<string, string> model, CancellationToken cancellationToken = default)
        {
            var html = await LoadTemplateAsync(templateName, cancellationToken);
            foreach (var kv in model)
                html = html.Replace("{{" + kv.Key + "}}", kv.Value?.ToString() ?? string.Empty);
            return html;
        }

        private async Task<string> LoadTemplateAsync(string name, CancellationToken cancellationToken)
        {
            // Try to load from disk
            if (!string.IsNullOrWhiteSpace(_templatePath))
            {
                var path = Path.Combine(_templatePath!, name);
                if (File.Exists(path)) return await File.ReadAllTextAsync(path, cancellationToken);
            }

            // Try to load from embedded resources
            var asm = typeof(MustacheTemplateRenderer).Assembly;
            var res = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith($"Templates.{name}", StringComparison.OrdinalIgnoreCase));

            if (res is null)
                throw new FileNotFoundException($"Template '{name}' not found");

            using var s = asm.GetManifestResourceStream(res)!;
            using var sr = new StreamReader(s, Encoding.UTF8);

            return await sr.ReadToEndAsync();
        }
    }
}
