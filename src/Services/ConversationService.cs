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
        var phoneToName = new Dictionary<string, string>();
        var groupAddrs = new Dictionary<string, HashSet<string>>();

        await foreach (var message in _repository.GetMessagesAsync(xmlStream))
        {
            var cn = message.ContactName;
            if (!string.IsNullOrWhiteSpace(cn) && cn != "null" && cn != "(Unknown)" && !message.Address.Contains('@'))
                phoneToName[message.Address] = cn;

            if (message is MmsMessage mms && mms.Addrs.Count > 0)
            {
                if (!groupAddrs.TryGetValue(message.Address, out var addrSet))
                    groupAddrs[message.Address] = addrSet = new HashSet<string>();
                foreach (var addr in mms.Addrs)
                    addrSet.Add(addr);
            }

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

                if ((string.IsNullOrWhiteSpace(contactName) || contactName == "null" || contactName == "(Unknown)") &&
                    groupAddrs.TryGetValue(kvp.Key, out var addrSet) && addrSet.Count > 0)
                {
                    contactName = string.Join(", ", addrSet
                        .Select(phone => phoneToName.TryGetValue(phone, out var name) ? name : phone));
                }

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
