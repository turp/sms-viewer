using System.Collections.Generic;
using System.IO;
using SmsViewer.Models;

namespace SmsViewer.Repositories;

/// <summary>
/// Defines the contract for retrieving SMS and MMS messages from a storage source.
/// </summary>
public interface ISmsRepository
{
    /// <summary>
    /// Asynchronously streams messages from the provided XML stream.
    /// </summary>
    IAsyncEnumerable<IMessage> GetMessagesAsync(Stream xmlStream);
}
