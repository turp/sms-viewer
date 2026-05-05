# contact-filter Specification

## Purpose
TBD - created by archiving change search-and-filter. Update Purpose after archive.
## Requirements
### Requirement: Live Contact Search
The system SHALL provide a text input above the conversation list that filters conversations in real time.

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

### Requirement: Date Range Filter
The system SHALL provide from-date and to-date inputs that filter the conversation list by the date of each conversation's most recent message.

#### Scenario: Filtering by from-date
- **WHEN** the user enters a valid from-date
- **THEN** the conversation list SHALL exclude conversations whose last message date is before the from-date

#### Scenario: Filtering by to-date
- **WHEN** the user enters a valid to-date
- **THEN** the conversation list SHALL exclude conversations whose last message date is after the to-date

#### Scenario: Invalid date input is ignored
- **WHEN** the user enters a value that cannot be parsed as a date
- **THEN** the date bound SHALL be treated as absent and no date filtering SHALL be applied for that bound

### Requirement: Filter Reset on File Load
The system SHALL reset all conversation filters when a new file is loaded.

#### Scenario: Filters clear on load
- **WHEN** the user opens a new XML file
- **THEN** the contact search text, from-date, and to-date SHALL all be cleared

