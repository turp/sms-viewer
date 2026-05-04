## 1. Domain Model Changes

- [ ] 1.1 Add `bool IsSent { get; }` to `IMessage` interface
- [ ] 1.2 Implement `IsSent` in `SmsMessage` record as `Type == 2`
- [ ] 1.3 Add `MsgBox` int property to `MmsMessage` record; implement `IsSent` as `MsgBox == 2`
- [ ] 1.4 Parse `msg_box` attribute in `XmlSmsRepository.ParseMmsAsync`; update existing MMS tests to cover IsSent
- [ ] 1.5 Add `Conversation` record to `Models/` with `Address`, `ContactName`, `DisplayName`, and `IReadOnlyList<IMessage> Messages`

## 2. Conversation Service (TDD)

- [ ] 2.1 Write unit tests for `ConversationService`: groups by address, sorts by most-recent-date desc, display name fallback, empty stream
- [ ] 2.2 Define `IConversationService` interface: `Task<IReadOnlyList<Conversation>> GetConversationsAsync(Stream xmlStream)`
- [ ] 2.3 Implement `ConversationService` using `ISmsRepository`

## 3. ViewModel Layer (TDD)

- [ ] 3.1 Add `ConversationListItemViewModel` with `DisplayName`, `LastMessagePreview`, `LastMessageDate`, `MessageCount`, `Messages`
- [ ] 3.2 Write unit tests for `ConversationListItemViewModel` construction from a `Conversation`
- [ ] 3.3 Update `MainWindowViewModel`: replace `ObservableCollection<IMessage>` with `ObservableCollection<ConversationListItemViewModel>` and add `SelectedConversation` property
- [ ] 3.4 Update `MainWindowViewModel` tests: loading populates `Conversations`, selecting sets `SelectedConversation`, error handling still works

## 4. UI

- [ ] 4.1 Redesign `MainWindow.axaml` with a two-column `Grid` and a `GridSplitter`
- [ ] 4.2 Left pane: `ListBox` bound to `Conversations` showing `DisplayName`, `LastMessagePreview`, `LastMessageDate`; selection bound to `SelectedConversation`
- [ ] 4.3 Right pane: `ItemsControl` bound to `SelectedConversation.Messages` with chat-bubble `DataTemplate` (right-aligned + accent color for sent, left-aligned + muted color for received)
- [ ] 4.4 Right pane: show `[Media message]` placeholder when `Body` is empty

## 5. Wiring & Cleanup

- [ ] 5.1 Update `App.axaml.cs` to construct and inject `ConversationService` into `MainWindowViewModel`
- [ ] 5.2 Remove now-unused `ISmsRepository` direct injection from `MainWindowViewModel` constructor

## 6. Verification

- [ ] 6.1 All unit tests pass; coverage ≥ 80%
- [ ] 6.2 Manual verification with `xml/sms-sample.xml`
- [ ] 6.3 [✓ CONSTITUTION CHECK] against `AGENTS.md`
