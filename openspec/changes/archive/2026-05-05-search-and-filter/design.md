## Context

All conversations and messages are already in memory after the file loads. Filtering is a pure in-memory ViewModel concern — no service or repository involvement. The challenge is keeping filter results reactive without introducing a separate observable-collection library.

## Goals / Non-Goals

**Goals:**
- Live (as-you-type) filtering of the conversation list by contact name or address.
- Live filtering of messages within the selected thread by body text.
- Date-range filtering of conversations by the Unix timestamp of the last message.
- "No results" placeholder in both panes.
- All filter state resets when a new file is loaded.

**Non-Goals:**
- Persisting filter state between sessions.
- Boolean / regex search syntax.
- Highlighting matched substrings in text (future enhancement).
- Filtering across all conversations simultaneously for message content.
- Date pickers — date range is entered as plain text (`yyyy-MM-dd`) to avoid UI complexity and keep zero new dependencies.

## Decisions

### 1. Filtered collections are `ObservableCollection<T>` properties rebuilt on filter change

**Decision**: `FilteredConversations` and `FilteredMessages` are `ObservableCollection<T>` properties on `MainWindowViewModel`. A private `ApplyConversationFilter()` method clears and repopulates `FilteredConversations` from `Conversations` whenever `SearchText`, `FilterFromDate`, or `FilterToDate` changes. Similarly `ApplyMessageFilter()` rebuilds `FilteredMessages` from `SelectedConversation.Messages` when `ThreadSearchText` or `SelectedConversation` changes.

**Rationale**: Avalonia's `ListBox` and `ItemsControl` already bind to `ObservableCollection<T>`. Rebuilding the filtered collection on each keystroke is fast enough for thousands of conversations/messages in memory. Avoids introducing a dependency on `System.Reactive` or `DynamicData` for what is simple synchronous filtering.

**Alternative considered**: Expose a `ICollectionView` / `ListCollectionView` shim. Rejected — not available in cross-platform Avalonia without extra ceremony.

### 2. `SelectedConversation` setter triggers message filter rebuild

**Decision**: When `SelectedConversation` changes, `ApplyMessageFilter()` is called immediately, resetting the thread search and populating `FilteredMessages` with all messages for the new conversation.

**Rationale**: Keeps the thread search scoped to the active conversation. Changing conversations should always show the full thread first.

### 3. Date range parsed from `yyyy-MM-dd` string inputs; invalid input is silently ignored

**Decision**: `FilterFromDate` and `FilterToDate` are `string?` properties. Parsing to `DateTimeOffset` (from Unix timestamp comparison) happens inside `ApplyConversationFilter()`. If parsing fails the date bound is treated as absent.

**Rationale**: Keeps the ViewModel testable without UI date-picker dependencies. `yyyy-MM-dd` is unambiguous and easily typed. The `Date` property on `IMessage` is a Unix timestamp in milliseconds, so comparison is a simple long comparison after converting the input date to milliseconds.

### 4. All filters are AND-ed together

**Decision**: A conversation must satisfy all active filters simultaneously (text AND date range).

**Rationale**: Most natural user expectation; keeps the implementation simple.

### 5. Filter state clears on new file load

**Decision**: `OpenXmlFileAsync` resets `SearchText`, `ThreadSearchText`, `FilterFromDate`, and `FilterToDate` to null/empty before loading.

**Rationale**: Stale filters from a previous file silently hiding results would be confusing.

## Risks / Trade-offs

- **[Risk]** Rebuilding `FilteredConversations` on every keystroke for a very large number of conversations (e.g., 10,000+) could lag.
  - **Mitigation**: LINQ `.Where()` on an in-memory list of `ConversationListItemViewModel` is O(n) and sub-millisecond for typical backup sizes. Debouncing can be added later if needed.

- **[Risk]** `yyyy-MM-dd` date input is unfamiliar to some users.
  - **Mitigation**: Placeholder text in the input box shows the expected format. A date picker can replace it in a future change without any spec-level changes.
