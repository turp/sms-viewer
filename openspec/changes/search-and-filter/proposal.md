## Why

The conversation list and thread view are read-only with no way to narrow down what's on screen. For anyone with a large backup file this makes finding a specific conversation or message impractical. Search and filter is the last piece needed before the app is genuinely useful day-to-day.

## What Changes

- Add a live-filter text box above the conversation list that narrows results by contact name or address as you type.
- Add a live-filter text box in the thread pane header that narrows messages within the selected conversation by body text.
- Add date-range inputs (from / to) to the conversation list pane that filter by the date of each conversation's most recent message.
- Show a "no results" placeholder in each pane when the active filter matches nothing.
- All filtering is in-memory and stateless — no changes to parsing, storage, or the service layer.

## Capabilities

### New Capabilities
- `contact-filter`: live text search on the conversation list by contact name or phone number address
- `message-filter`: live text search within the selected thread by message body text

### Modified Capabilities
- `conversation-list`: adds requirements for date-range filtering and a "no results" state when filters are active

## Impact

- **Modified**: `MainWindowViewModel` — adds `SearchText`, `FilteredConversations`, `ThreadSearchText`, `FilteredMessages`, `FilterFromDate`, `FilterToDate` properties
- **New**: no new classes needed; filtering logic lives in the ViewModel
- **Modified**: `MainWindow.axaml` — search inputs added to both panes; `ListBox` and `ItemsControl` re-bound to filtered collections
- **No changes**: models, repository, `ConversationService`, `ConversationListItemViewModel`
