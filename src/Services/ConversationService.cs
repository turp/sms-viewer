using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmsViewer.Models;
using SmsViewer.Repositories;

namespace SmsViewer.Services;

public class ConversationService : IConversationService
{
    private readonly ISmsRepository _repository;

    public ConversationService(ISmsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ConversationSummary>> GetConversationSummariesAsync(Stream xmlStream)
    {
        var groups = new Dictionary<string, (string contactName, long lastDate, string lastReadableDate, string lastBody, int count)>();

        await foreach (var message in _repository.GetMessagesAsync(xmlStream))
        {
            if (!groups.TryGetValue(message.Address, out var g))
                g = (message.ContactName, 0, string.Empty, string.Empty, 0);

            bool isNewer = message.Date >= g.lastDate;
            groups[message.Address] = (
                message.ContactName,
                isNewer ? message.Date : g.lastDate,
                isNewer ? message.ReadableDate : g.lastReadableDate,
                isNewer ? message.DisplayBody : g.lastBody,
                g.count + 1
            );
        }

        return groups
            .Select(kvp =>
            {
                var (contactName, lastDate, lastReadableDate, lastBody, count) = kvp.Value;
                var preview = lastBody.Length > 60 ? lastBody[..60] + "…" : lastBody;
                return new ConversationSummary(kvp.Key, contactName, preview, lastReadableDate, lastDate, count);
            })
            .OrderByDescending(s => s.LastMessageDateUnixMs)
            .ToList();
    }

    public async Task<IReadOnlyList<IMessage>> GetConversationMessagesAsync(Stream xmlStream, string address)
    {
        var messages = new List<IMessage>();
        await foreach (var message in _repository.GetMessagesAsync(xmlStream))
        {
            if (message.Address == address)
                messages.Add(message);
        }
        return messages.OrderBy(m => m.Date).ToList();
    }
}
