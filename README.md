# Municipality Services Application

A Windows Forms application that allows citizens to report issues and service requests to their local municipality.

## Features

- **Report Issues**: Submit detailed reports about municipal issues including location, category, and description
- **File Attachments**: Attach images or documents to support your report
- **User-Friendly Interface**: Clean and intuitive design with clear feedback
- **Form Validation**: Ensures all required fields are completed before submission

## Prerequisites

- .NET Framework 4.8 or later
- Windows 10/11
- Visual Studio 2019 or later (for development)

## Installation

1. Clone this repository or download the source code
2. Open the solution file `PROG7312-POE.sln` in Visual Studio
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

## Usage

1. **Main Menu**
   - Launch the application to see the main menu
   - Select "Report Issues" to start a new report
   - Other options are currently disabled and will be implemented in future updates

2. **Reporting an Issue**
   - Fill in the location of the issue
   - Select the appropriate category from the dropdown
   - Provide a detailed description of the issue
   - Optionally attach supporting files
   - Click "Submit Report" to submit your report

3. **After Submission**
   - You'll see a confirmation message upon successful submission
   - The form will close automatically
   - You can start a new report by selecting "Report Issues" again

## Development

### Project Structure

- `Form1.cs`: Main menu form
- `ReportIssueForm.cs`: Form for submitting issue reports
- `IssueReport.cs`: Data model for issue reports

### Adding New Features

1. To implement the "Local Events and Announcements" or "Service Request Status" features:
   - I am going to enable the corresponding buttons in `Form1.cs`
   - I am going to create new forms for each feature
   - I will implement the required functionality


