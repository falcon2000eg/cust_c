# Customer Issues Manager

A WPF application for managing customer issues, attachments, and employee actions with robust settings and validation.

## Features
- Case management with audit logging
- Attachment management (move, organize, backup)
- Persistent settings (JSON)
- SQLite database
- Real-time validation and notifications
- Responsive UI and error handling

## Getting Started
1. **Build**: Open `CustomerIssuesManager.sln` in Visual Studio and build.
2. **Run**: Use Visual Studio or `run_with_debug.bat` for debugging.
3. **Settings**: Configure via the Settings window. Import/export with JSON files.
4. **Database**: Default path is `CustomerIssues.db` (configurable).

## Project Structure
- `CustomerIssuesManager/` - Main WPF UI and logic
- `CustomerIssuesManager.Core/` - Core services, models, and database
- `.github/copilot-instructions.md` - AI agent instructions

## Contributing
- Fork the repository
- Create a feature branch
- Submit pull requests with clear descriptions

## License
MIT

## Support
Contact: support@customerissues.com
