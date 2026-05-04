## Context

The current `MainWindowViewModel` holds an `ObservableCollection<IMessage>` fed directly from `ISmsRepository`. The UI is a single `ListBox` rendering all messages in chronological order with no grouping. For any realistic backup file this is unusable — thousands of messages from dozens of contacts in a single undifferentiated scroll.

The change introduces a service layer to group messages by contact and a two-pane UI to browse and read conversations.

## Goals / Non-Goals

**Goals:**
- Group all messages for a given contact into a `Conversation` aggregate.
- Two-pane layout: contact list (left) + message thread (right).
- Visual distinction between sent and received messages (bubble alignment/color).
- No new dependencies.

**Non-Goals:**
- Search or filtering (Phase C).
- Media attachment rendering (MMS images/audio).
- Multiple-contact group threads (treat `address` as a single canonical key).
- Pagination or lazy loading of the thread panel.

## Decisions

### 1. Grouping happens in a new `ConversationService`, not in the ViewModel

**Decision**: Introduce `IConversationService` / `ConversationService` between the ViewModel and `ISmsRepository`.

**Rationale**: The ViewModel's responsibility is managing UI state, not aggregation logic. Extracting grouping into a service keeps the ViewModel thin and makes the grouping logic independently testable. The ViewModel should call the service, not the repository directly.

**Alternative considered**: Group inline in the ViewModel during the `await foreach` loop. Rejected because it mixes aggregation logic with UI state management, violating SRP.

### 2. `ConversationService` buffers the full stream before returning

**Decision**: `IConversationService.GetConversationsAsync(Stream)` returns `Task<IReadOnlyList<Conversation>>` (not `IAsyncEnumerable`).

**Rationale**: Conversations can only be complete after all messages in the stream are seen — you don't know which message is "last" until you've consumed the stream. Streaming partial conversations would require complex forward-merge logic. The buffer-first approach is simple and correct. Memory cost is the same as the previous flat-list approach.

**Alternative considered**: Streaming `IAsyncEnumerable<Conversation>` by treating each address's first-seen occurrence as a conversation header, then updating it. Rejected for now as over-engineering.

### 3. Sent/received direction added to `IMessage` as `bool IsSent`

**Decision**: Add `bool IsSent { get; }` to `IMessage`; implement as `Type == 2` on `SmsMessage` and `MsgBox == 2` on `MmsMessage`. Parse `msg_box` from the MMS XML element.

**Rationale**: The view needs direction to align bubbles. Rather than pattern-matching on concrete types in the XAML DataTemplate, a single `IsSent` property keeps the binding simple and the view decoupled from subtypes.

**Alternative considered**: Pattern-match `SmsMessage`/`MmsMessage` in separate DataTemplates. Rejected because it duplicates bubble styling and couples the view to the concrete model hierarchy.

### 4. Conversation key is `Address`; display name falls back to `Address`

**Decision**: Group by `Address` attribute (phone number). Display `ContactName` if it is non-empty and not the literal string "null"; otherwise display `Address`.

**Rationale**: `ContactName` is unreliable — some backup apps emit "null" as a string. `Address` is always present and unique per contact.

### 5. Two-pane layout using a `Grid` with fixed left-column width

**Decision**: `MainWindow.axaml` uses a two-column `Grid` (250px fixed left, `*` right). No third-party panel library needed.

**Rationale**: Avalonia's `Grid` with `GridSplitter` is sufficient and adds zero dependencies.

## Risks / Trade-offs

- **[Risk]** Very large backup files fully buffered in `ConversationService` may cause a slow first-load pause before any conversations appear.
  - **Mitigation**: The existing `IsLoading` progress indicator already covers this. Future work can add streaming.

- **[Risk]** Contacts with multiple phone numbers (e.g., mobile and work) appear as separate conversations.
  - **Mitigation**: Acceptable for v1; contact merging is a future concern.

- **[Risk]** MMS `msg_box` attribute may be absent in some backup formats.
  - **Mitigation**: Default to `MsgBox = 0` (treated as received) when the attribute is missing.
