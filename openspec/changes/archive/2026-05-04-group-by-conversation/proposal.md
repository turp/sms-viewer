## Why

The app currently renders a flat chronological dump of every message in the backup file, making it unusable for actually reading a conversation. Grouping messages by contact and presenting a two-pane conversation view is the minimum change needed for the app to deliver real value.

## What Changes

- Introduce a `Conversation` domain model that aggregates all messages for a single contact.
- Add a `IConversationService` that consumes `ISmsRepository` and builds grouped conversations.
- Replace the single-pane flat `ListBox` with a two-pane layout: contact list (left) and message thread (right).
- Display messages as chat bubbles distinguishing sent (type=2) from received (type=1).
- `MainWindowViewModel` gains a selected-conversation concept; loading now populates conversations, not raw messages.

## Capabilities

### New Capabilities
- `conversation-list`: Left-pane contact list showing each conversation's contact name, last message preview, and date. Selecting a contact loads the thread.
- `conversation-thread`: Right-pane message thread showing bubbles for the selected conversation, with visual distinction between sent and received messages.

### Modified Capabilities
- `message-view`: Requirements change from a flat chronological list to a two-pane conversation UI. The flat-list scenario is replaced by the conversation-list and conversation-thread scenarios.

## Impact

- **New models**: `Conversation` record in `Models/`
- **New service**: `IConversationService` / `ConversationService` in `Services/`
- **New ViewModels**: `ConversationListItemViewModel`, `ConversationThreadViewModel`
- **Refactored**: `MainWindowViewModel` — replaces `ObservableCollection<IMessage>` with `ObservableCollection<ConversationListItemViewModel>` and a `SelectedConversation` property
- **Refactored**: `MainWindow.axaml` — two-pane `Grid` layout replaces the single `ListBox`
- **No changes**: `ISmsRepository`, `XmlSmsRepository`, `IFilePickerService`, domain message records
