# xml-parsing Specification

## Purpose
Defines the requirements for streaming XML parsing of SMS/MMS backup files using `XmlReader`, ensuring large files are processed without loading the full document into memory.
## Requirements
### Requirement: XML Streaming Parser
The system SHALL implement a streaming XML parser using `XmlReader` to process SMS/MMS backup files without loading the entire document into memory, and the end-to-end loading flow SHALL avoid requiring eager full-data materialization before the conversation list can be shown.

#### Scenario: Parse valid SMS messages
- **WHEN** the parser encounters a valid `<sms>` element in the XML file
- **THEN** it SHALL extract all required attributes (address, date, type, body, read, status) into an `SmsMessage` object

#### Scenario: Parse valid MMS messages
- **WHEN** the parser encounters a valid `<mms>` element
- **THEN** it SHALL extract message metadata and all child `<part>` elements into an `MmsMessage` object

#### Scenario: Handle malformed XML
- **WHEN** the parser encounters a malformed or invalid XML node
- **THEN** it SHALL log the error and continue parsing the next valid sibling node if possible

#### Scenario: Conversation list loads without eager thread materialization
- **WHEN** the user opens a large XML backup file
- **THEN** the system SHALL be able to populate conversation-list data without first materializing every message of every thread into UI-ready collections

