# thread-export Specification

## Purpose
Defines the requirements for exporting selected conversation threads to an XML file in SMS Backup & Restore format, including command availability, output fidelity, and error handling.

## Requirements

### Requirement: Thread Export Command
The system SHALL provide an export command that writes selected conversation threads to a new XML file in the same format as an SMS Backup & Restore XML backup.

#### Scenario: Export button disabled with no selections
- **WHEN** no conversation checkboxes are checked
- **THEN** the "Export selected…" button SHALL be disabled

#### Scenario: Export button disabled with no file loaded
- **WHEN** no SMS backup file is currently loaded
- **THEN** the "Export selected…" button SHALL be disabled

#### Scenario: Export button enabled when selections exist
- **WHEN** at least one conversation checkbox is checked and a source file is loaded
- **THEN** the "Export selected…" button SHALL be enabled

#### Scenario: Export prompts for save location
- **WHEN** the user clicks "Export selected…"
- **THEN** the system SHALL open a save-file dialog for the user to choose a destination path and filename

#### Scenario: Export cancelled by user
- **WHEN** the user dismisses the save-file dialog without selecting a path
- **THEN** no file SHALL be written and the application state SHALL be unchanged

### Requirement: Exported XML Fidelity
The exported XML file SHALL be a valid SMS backup XML containing only the messages from the selected threads, with full attribute and child-element fidelity.

#### Scenario: Output file has correct root element
- **WHEN** an export completes successfully
- **THEN** the output file SHALL begin with `<?xml version="1.0" encoding="UTF-8"?>` and have `<smses>` as the root element with a `count` attribute equal to the number of exported `<sms>` and `<mms>` elements

#### Scenario: Only selected thread messages are exported
- **WHEN** the user exports with threads A and B selected out of A, B, C
- **THEN** the output file SHALL contain all messages whose `address` matches thread A or thread B, and no messages from thread C

#### Scenario: MMS elements preserve parts and addresses
- **WHEN** a selected thread contains MMS messages with `<parts>` and `<addrs>` child elements
- **THEN** the exported `<mms>` elements SHALL include all their original child elements and attributes

#### Scenario: Message order preserved
- **WHEN** a thread is exported
- **THEN** messages SHALL appear in the output file in the same relative order as the source file

#### Scenario: Source file is not modified by export
- **WHEN** the user exports one or more threads
- **THEN** the source XML file SHALL be unchanged after the export completes

### Requirement: Export Error Handling
The system SHALL surface export errors to the user without crashing.

#### Scenario: Source file unreadable during export
- **WHEN** the source file becomes inaccessible after load (deleted, locked)
- **THEN** the system SHALL display an error message and SHALL NOT write a partial output file
