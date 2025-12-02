# Student Information and Progress Report System

A comprehensive Windows Forms application built with .NET 8 for managing student information, attendance, and grades with role-based access control.

## Features

### User Roles
- **Super Admin**: Global access across all branches, manages admins, security policies, and theme preferences
- **Branch Admin**: Full access scoped to their assigned branch – manage users, students, and subjects (except Super Admin accounts)
- **Professor**: Record attendance and grades for assigned subjects
- **Guardian**: View child's grades and attendance reports
- **Student**: View own grades and attendance reports

### Core Functionality

#### Admin / Super Admin Features
- **Advanced User Management**
  - Dynamic filtering by role, section, course, and year level
  - Guardian creation wizard with student-selection dialog (course/year/section filters)
  - Super Admin credential editing protected by dual password bypass + account verification
  - Admin request escalation workflow for branch admins
- **Student Management**: Add, edit, filter (section/course/year) and auto-link guardians
- **Subject Management**
  - Course-aware creation (course + section code)
  - Filter subjects by grade, section, and course
- **Security Console**
  - Dark/Light theme toggle (persisted)
  - Security audit logging for privileged Super Admin changes
- **Branch Isolation**: Non-Super Admins automatically scoped to their branch data

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
- **Ado.NET** / `SqlClient` data access with async operations
- **Automatic database initialization** on first run (tables, seed Super Admin, security logs)
- **SecurityAuditLogs** table tracking Super Admin credential updates

### Architecture
- **Windows Forms** with custom-painted modern UI
- **ThemeManager** with Light/Dark palettes and persisted settings
- **Role-based authentication** system with OTP fallback and admin password bypass
- **Modular panel-based interface with reusable filter components**
- **Async/await** for database operations
- **Security helper utilities** (password hashing, phone validation, audit logging)

- **Users**: System users with role-based access (includes Super Admin flag)
- **Students**: Student information, branch, guardians, computed courses
- **Subjects**: Course-aware scheduling with professor assignment
- **Attendance**: Daily attendance records
- **Grades**: Assignment and exam grades
- **AdminRequests**: Branch admin escalation requests
- **SecurityAuditLogs**: Trace privileged Super Admin actions

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
1. Login as Super Admin (default account)
2. Configure branches (optional filters) and toggle preferred theme
3. Create branch admin / professor accounts (admin-password bypass enforced for admins)
4. Import/add students and use guardian selection dialog to link responsible parties
5. Create subjects (course + section aware) and assign to professors

### Daily Operations
1. **Super Admin**: Oversees security logs, admin requests, and multi-branch data
2. **Branch Admin**: Manages users/students/subjects within branch, uses filtering to locate records quickly
3. **Professors**: Record attendance and input grades per assigned subjects
4. **Guardians/Students**: View reports and progress through Viewer panels

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

- **Clean, modern design** with custom gradient login, curved buttons, and iconography
- **Centered login layout** with envelope/lock indicators and themed accents
- **Intuitive navigation** with sidebar menus and dynamic content panels
- **Reusable filter toolbars** above data grids (role/section/course/year)
- **Guardian student picker** modal with inline filters
- **Dark/Light mode toggle** (persisted per user) via `ThemeManager`
- **Professional typography** using Segoe UI font
- **Smooth user experience** with double-buffered forms and custom painting

## Security Features

- **Role-based access control** scoped by branch with Super Admin overrides
- **Dual-step Super Admin edits** (Admin Password Bypass + account password) with audit logging
- **Admin password bypass** required for creating/editing admin-level accounts
- **OTP verification** for phone-bound operations (non-admin scenarios)
- **Security audit logs** stored in `SecurityAuditLogs`
- **Data validation** on all input forms (email, phone, OTP, guardian assignments)
- **SQL injection protection** using parameterized queries
- **Theme-aware UI** ensures consistent visibility/security prompts in both modes

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
