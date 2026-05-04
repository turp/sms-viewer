using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmsViewer.Models;

namespace SmsViewer.Services;

/// <summary>
/// Groups messages from a stream into conversations by contact.
/// </summary>
public interface IConversationService
{
    Task<IReadOnlyList<Conversation>> GetConversationsAsync(Stream xmlStream);
}
