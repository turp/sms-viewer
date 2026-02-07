using System.Collections.Generic;
using SmsViewer.Models;

namespace SmsViewer.Repositories;

/// <summary>
/// Defines the contract for retrieving SMS and MMS messages from a storage source.
/// </summary>
public interface ISmsRepository
{
    /// <summary>
    /// asynchronously streams messages from the specified file path.
    /// </summary>
    /// <param name="filePath">The absolute path to the XML backup file.</param>
    /// <returns>An async stream of IMessage objects.</returns>
    IAsyncEnumerable<IMessage> GetMessagesAsync(string filePath);
}
