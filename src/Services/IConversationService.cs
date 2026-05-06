using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmsViewer.Models;

namespace SmsViewer.Services;

public interface IConversationService
{
    Task<IReadOnlyList<ConversationSummary>> GetConversationSummariesAsync(Stream xmlStream);
    Task<IReadOnlyList<IMessage>> GetConversationMessagesAsync(Stream xmlStream, string address);
}
