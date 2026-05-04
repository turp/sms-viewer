## 1. Domain Model Changes

- [x] 1.1 Add `bool IsSent { get; }` to `IMessage` interface
- [x] 1.2 Implement `IsSent` in `SmsMessage` record as `Type == 2`
- [x] 1.3 Add `MsgBox` int property to `MmsMessage` record; implement `IsSent` as `MsgBox == 2`
- [x] 1.4 Parse `msg_box` attribute in `XmlSmsRepository.ParseMmsAsync`; update existing MMS tests to cover IsSent
- [x] 1.5 Add `Conversation` record to `Models/` with `Address`, `ContactName`, `DisplayName`, and `IReadOnlyList<IMessage> Messages`

## 2. Conversation Service (TDD)

- [x] 2.1 Write unit tests for `ConversationService`: groups by address, sorts by most-recent-date desc, display name fallback, empty stream
- [x] 2.2 Define `IConversationService` interface: `Task<IReadOnlyList<Conversation>> GetConversationsAsync(Stream xmlStream)`
- [x] 2.3 Implement `ConversationService` using `ISmsRepository`

## 3. ViewModel Layer (TDD)

- [x] 3.1 Add `ConversationListItemViewModel` with `DisplayName`, `LastMessagePreview`, `LastMessageDate`, `MessageCount`, `Messages`
- [x] 3.2 Write unit tests for `ConversationListItemViewModel` construction from a `Conversation`
- [x] 3.3 Update `MainWindowViewModel`: replace `ObservableCollection<IMessage>` with `ObservableCollection<ConversationListItemViewModel>` and add `SelectedConversation` property
- [x] 3.4 Update `MainWindowViewModel` tests: loading populates `Conversations`, selecting sets `SelectedConversation`, error handling still works

## 4. UI

- [x] 4.1 Redesign `MainWindow.axaml` with a two-column `Grid` and a `GridSplitter`
- [x] 4.2 Left pane: `ListBox` bound to `Conversations` showing `DisplayName`, `LastMessagePreview`, `LastMessageDate`; selection bound to `SelectedConversation`
- [x] 4.3 Right pane: `ItemsControl` bound to `SelectedConversation.Messages` with chat-bubble `DataTemplate` (right-aligned + accent color for sent, left-aligned + muted color for received)
- [x] 4.4 Right pane: show `[Media message]` placeholder when `Body` is empty

## 5. Wiring & Cleanup

- [x] 5.1 Update `App.axaml.cs` to construct and inject `ConversationService` into `MainWindowViewModel`
- [x] 5.2 Remove now-unused `ISmsRepository` direct injection from `MainWindowViewModel` constructor

## 6. Verification

- [x] 6.1 All unit tests pass; coverage ≥ 80%
- [x] 6.2 Manual verification with `xml/sms-sample.xml`
- [x] 6.3 [✓ CONSTITUTION CHECK] against `AGENTS.md`
