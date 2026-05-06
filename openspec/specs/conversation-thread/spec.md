# conversation-thread Specification

## Purpose
TBD - created by archiving change group-by-conversation. Update Purpose after archive.
## Requirements
### Requirement: Message Thread Display
The system SHALL display all messages for the selected conversation in chronological order (oldest first) in a right pane. The thread SHALL load on demand for the selected conversation and SHALL remain usable for conversations with very large message counts.

#### Scenario: Sent messages are right-aligned
- **WHEN** a message in the thread has IsSent equal to true
- **THEN** it SHALL be displayed aligned to the right with a distinct background color

#### Scenario: Received messages are left-aligned
- **WHEN** a message in the thread has IsSent equal to false
- **THEN** it SHALL be displayed aligned to the left with a distinct background color

#### Scenario: Each message shows its body and date
- **WHEN** a message is displayed in the thread
- **THEN** it SHALL show the message body and the human-readable date

#### Scenario: MMS with no text body shows placeholder
- **WHEN** an MMS message has an empty body (media-only)
- **THEN** it SHALL display a placeholder such as "[Media message]"

#### Scenario: Long thread becomes scrollable without rendering every row first
- **WHEN** the selected conversation contains a very large number of messages
- **THEN** the thread pane SHALL become scrollable without requiring all message rows to be realized before initial display

