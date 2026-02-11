using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

public class NotifierMessage
{
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string Context { get; set; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotifierMessageSeverity Severity { get; set; } = NotifierMessageSeverity.Info;

    public string ColorSeverity => Severity switch
    {
        NotifierMessageSeverity.Success => "#00c853",
        NotifierMessageSeverity.Info => "#2196f3",
        NotifierMessageSeverity.Warning => "#ff9800",
        NotifierMessageSeverity.Error => "#f44336",
        _ => string.Empty
    };

    public IEnumerable<Attachment> Attachments { get; set; } = [];
    public Dictionary<string, object> Data { get; set; } = new()!;
}
