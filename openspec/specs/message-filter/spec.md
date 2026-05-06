# message-filter Specification

## Purpose
TBD - created by archiving change search-and-filter. Update Purpose after archive.
## Requirements
### Requirement: Live Message Search
The system SHALL provide a text input in the thread pane that filters the visible messages within the selected conversation in real time. The filter SHALL operate on the currently loaded thread and SHALL not require rebuilding presentation data for every off-screen message before results can be shown.

#### Scenario: Filtering by message body
- **WHEN** the user types text into the message search input
- **THEN** the thread SHALL show only messages whose body contains the search text (case-insensitive)

#### Scenario: Empty search shows all messages
- **WHEN** the message search input is empty or cleared
- **THEN** the thread SHALL show all messages in the selected conversation

#### Scenario: No matching messages
- **WHEN** the search text matches no messages in the thread
- **THEN** the thread SHALL show a "No messages match" placeholder

#### Scenario: Search updates without eager row realization
- **WHEN** the selected conversation contains a very large number of messages and the user changes the message search text
- **THEN** the system SHALL update the visible result set without requiring every message row to be realized before the thread can respond

### Requirement: Message Filter Reset on Conversation Change
The system SHALL reset the message search when the user selects a different conversation.

#### Scenario: Filter clears on conversation change
- **WHEN** the user selects a different conversation from the list
- **THEN** the message search input SHALL be cleared and all messages in the new conversation SHALL be shown

### Requirement: Message Filter Reset on File Load
The system SHALL reset the message search when the user opens a new XML file.

#### Scenario: Filter clears on load
- **WHEN** the user opens a new XML file
- **THEN** the message search text SHALL be cleared

