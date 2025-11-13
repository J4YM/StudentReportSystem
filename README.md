# Student Information and Progress Report System

A comprehensive Windows Forms application built with .NET 8 for managing student information, attendance, and grades with role-based access control.

## Features

### User Roles
- **Admin**: Full system access - manage users, students, and subjects
- **Professor**: Record attendance and grades for assigned subjects
- **Guardian**: View child's grades and attendance reports
- **Student**: View own grades and attendance reports

### Core Functionality

#### Admin Features
- **User Management**: Add, edit, and manage all system users
- **Student Management**: Add, edit, and manage student records
- **Subject Management**: Create and assign subjects to professors
- **System Reports**: View comprehensive system statistics

#### Professor Features
- **Attendance Recording**: Record daily attendance for students
- **Grade Management**: Input and manage student grades
- **Subject Management**: View assigned subjects and classes
- **Reports**: Generate attendance and grade reports

#### Guardian/Student Features
- **Grade Viewing**: View detailed grade reports with averages
- **Attendance Tracking**: Monitor attendance records
- **Profile Management**: View and update personal information

## Technical Details

### Database
- **SQL Server LocalDB** for data storage
- **Entity Framework** for data access
- **Automatic database initialization** on first run

### Architecture
- **Windows Forms** with modern UI design
- **Role-based authentication** system
- **Modular panel-based interface**
- **Async/await** for database operations

### Database Schema
- **Users**: System users with role-based access
- **Students**: Student information and enrollment data
- **Subjects**: Course and subject management
- **Attendance**: Daily attendance records
- **Grades**: Assignment and exam grades

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server LocalDB (included with Visual Studio)
- Windows operating system

### Installation
1. Clone or download the project
2. Open in Visual Studio 2022 or later
3. Restore NuGet packages
4. Build and run the application

### Default Login
- **Username**: admin
- **Password**: admin123

## Usage

### First Time Setup
1. Login as admin
2. Create professor accounts
3. Create guardian accounts
4. Add students and assign guardians
5. Create subjects and assign to professors

### Daily Operations
1. **Professors**: Record attendance and input grades
2. **Guardians/Students**: View reports and progress
3. **Admin**: Manage users and system settings

## File Structure

```
StudentReportInitial/
├── Models/                 # Data models
│   ├── User.cs
│   ├── Student.cs
│   ├── Attendance.cs
│   ├── Grade.cs
│   └── Subject.cs
├── Forms/                  # User interface forms
│   ├── LoginForm.cs
│   ├── MainDashboard.cs
│   ├── AdminUserManagement.cs
│   ├── AdminStudentManagement.cs
│   ├── AdminSubjectManagement.cs
│   ├── ProfessorAttendancePanel.cs
│   ├── ProfessorGradesPanel.cs
│   ├── ViewerGradesPanel.cs
│   └── ViewerAttendancePanel.cs
├── Data/                   # Database access layer
│   └── DatabaseHelper.cs
└── Program.cs             # Application entry point
```

## Modern UI Features

- **Clean, modern design** with consistent color scheme
- **Responsive layout** that adapts to different screen sizes
- **Intuitive navigation** with sidebar menus
- **Color-coded status indicators** for attendance and grades
- **Professional typography** using Segoe UI font
- **Smooth user experience** with proper loading states

## Security Features

- **Role-based access control** ensuring users only see relevant data
- **Password authentication** for all user types
- **Data validation** on all input forms
- **SQL injection protection** using parameterized queries

## Future Enhancements

- Email notifications for guardians
- Grade analytics and trends
- Export functionality for reports
- Mobile-responsive web interface
- Advanced reporting and analytics
- Integration with external systems

## Support

For technical support or feature requests, please contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: 2024  
**Framework**: .NET 8.0 Windows Forms
