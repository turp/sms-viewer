using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmsViewer.Models;
using SmsViewer.Repositories;

namespace SmsViewer.Services;

/// <summary>
/// Implements IConversationService by grouping repository messages by address.
/// </summary>
public class ConversationService : IConversationService
{
    private readonly ISmsRepository _repository;

    public ConversationService(ISmsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Conversation>> GetConversationsAsync(Stream xmlStream)
    {
        var groups = new Dictionary<string, (string contactName, List<IMessage> messages)>();

        await foreach (var message in _repository.GetMessagesAsync(xmlStream))
        {
            if (!groups.TryGetValue(message.Address, out var group))
            {
                group = (message.ContactName, new List<IMessage>());
                groups[message.Address] = group;
            }
            group.messages.Add(message);
        }

        return groups
            .Select(kvp =>
            {
                var sorted = kvp.Value.messages.OrderBy(m => m.Date).ToList();
                return new Conversation(kvp.Key, kvp.Value.contactName, sorted);
            })
            .OrderByDescending(c => c.Messages[^1].Date)
            .ToList();
    }
}
