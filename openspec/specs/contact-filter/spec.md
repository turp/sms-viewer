# contact-filter Specification

## Purpose
TBD - created by archiving change search-and-filter. Update Purpose after archive.
## Requirements
### Requirement: Live Contact Search
The system SHALL provide a text input above the conversation list that filters conversations in real time. The filter SHALL operate on conversation summary data and SHALL not require loading full thread contents for non-selected conversations.

#### Scenario: Filtering by contact name
- **WHEN** the user types text into the contact search input
- **THEN** the conversation list SHALL show only conversations whose contact display name contains the search text (case-insensitive)

#### Scenario: Filtering by address
- **WHEN** the user types text into the contact search input
- **THEN** the conversation list SHALL show only conversations whose phone number address contains the search text

#### Scenario: Empty search shows all conversations
- **WHEN** the contact search input is empty or cleared
- **THEN** the conversation list SHALL show all conversations

#### Scenario: No matching conversations
- **WHEN** the search text matches no conversations
- **THEN** the conversation list SHALL show a "No conversations match" placeholder

#### Scenario: Search does not require non-selected threads to load
- **WHEN** the user filters the conversation list in a large backup
- **THEN** the system SHALL evaluate the filter without loading full thread contents for conversations that are not selected

### Requirement: Filter Reset on File Load
The system SHALL reset all conversation filters when a new file is loaded.

#### Scenario: Filters clear on load
- **WHEN** the user opens a new XML file
- **THEN** the contact search text, from-date, and to-date SHALL all be cleared

