## 1. ViewModel — Conversation Filter (TDD)

- [x] 1.1 Add `SearchText` string property to `MainWindowViewModel`; changing it calls `ApplyConversationFilter()`
- [x] 1.2 Add `FilterFromDate` and `FilterToDate` string properties; changing either calls `ApplyConversationFilter()`
- [x] 1.3 Add `FilteredConversations` as `ObservableCollection<ConversationListItemViewModel>`
- [x] 1.4 Implement `ApplyConversationFilter()`: case-insensitive contains on `DisplayName` and `Address`, date-range check on last-message `Date` (Unix ms), AND-ed together
- [x] 1.5 Write tests: empty filter shows all, name match, address match, case-insensitive, no match returns empty, from-date excludes early, to-date excludes late, invalid date ignored, AND logic, resets on load

## 2. ViewModel — Message Filter (TDD)

- [x] 2.1 Add `ThreadSearchText` string property to `MainWindowViewModel`; changing it calls `ApplyMessageFilter()`
- [x] 2.2 Add `FilteredMessages` as `ObservableCollection<IMessage>`
- [x] 2.3 Implement `ApplyMessageFilter()`: case-insensitive contains on `DisplayBody`; rebuilds when `SelectedConversation` changes (resets `ThreadSearchText`)
- [x] 2.4 Write tests: empty filter shows all, body match, case-insensitive, no match returns empty, resets on conversation change, resets on load

## 3. UI

- [x] 3.1 Add contact search `TextBox` bound to `SearchText` above the conversation `ListBox` (with placeholder text)
- [x] 3.2 Add from/to date `TextBox` inputs bound to `FilterFromDate` / `FilterToDate` below the search box (placeholder `yyyy-MM-dd`)
- [x] 3.3 Add "No conversations match" `TextBlock` bound to `FilteredConversations` emptiness, visible only when list is empty and file is loaded
- [x] 3.4 Rebind conversation `ListBox` from `Conversations` to `FilteredConversations`
- [x] 3.5 Add message search `TextBox` bound to `ThreadSearchText` in the thread pane header (with placeholder text)
- [x] 3.6 Add "No messages match" `TextBlock` in the thread pane, visible only when `FilteredMessages` is empty and a conversation is selected
- [x] 3.7 Rebind thread `ItemsControl` from `SelectedConversation.Messages` to `FilteredMessages`

## 4. Verification

- [x] 4.1 All unit tests pass; coverage ≥ 80%
- [x] 4.2 Manual verification with `xml/sms-sample.xml`
- [x] 4.3 [✓ CONSTITUTION CHECK] against `AGENTS.md`
