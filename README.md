# 🏥 Clinic Appointment System
**Advanced Programming (.NET) — Semester Project | Spring 2026**

### Group Members

| # | Name | Roll Number |
|---|---|---|
| 1 | Muhammad Noman (Leader) | |
| 2 | Muhammad Mudassir Asghar | |
| 3 | | |

**Section:** ______ | **Demo:** 9 June 2026

---

## Rubric Coverage

| # | Criterion | Marks | Implemented |
|---|-----------|-------|-------------|
| 1 | Project Setup & Architecture | 5 | ✅ App.Core + App.WindowsApp N-tier |
| 2 | Database & Connection | 5 | ✅ SQL Server / LocalDB via ADO.NET |
| 3 | Data Access Layer (ADO.NET) | 8 | ✅ DatabaseHelper, all services |
| 4 | UI Navigation & CRUD | 10 | ✅ 4 forms, sidebar navigation |
| 5 | Validation & UX | 2 | ✅ Full validation on all forms |
| 6 | Charting Module | 10 | ✅ 4 charts: Pie, Bar×2, Line |
| 7 | Code Quality | 2 | ✅ Interfaces, separation of concerns |
| 8 | Demo & Viva | 8 | 🎯 Practice explaining each class |
| **BONUS** | Search/Filter | +1 | ✅ Search box + gender/status dropdowns |
| **BONUS** | Dashboard with charts | +1 | ✅ Stats + chart button on dashboard |
| **BONUS** | Async operation | +1 | ✅ GetAllPatientsAsync in PatientService |
| **BONUS** | Status bar | +1 | ✅ Live clock + last action message |
| **BONUS** | Column sorting | +1 | ✅ All DataGridViews sortable |

**Potential score: 50 + 5 bonus = 55/50**

---

## Project Structure

```
ClinicAppointmentSystem/
├── App.Core/
│   ├── Models/
│   │   ├── Patient.cs
│   │   ├── Doctor.cs
│   │   ├── Appointment.cs
│   │   └── ChartDataPoint.cs       ← chart data model
│   ├── Interfaces/
│   │   ├── IPatientService.cs
│   │   ├── IDoctorService.cs
│   │   ├── IAppointmentService.cs
│   │   └── IReportService.cs       ← chart data interface
│   ├── Services/
│   │   ├── PatientService.cs       ← includes async method
│   │   ├── DoctorService.cs
│   │   ├── AppointmentService.cs
│   │   └── ReportService.cs        ← 4 chart queries
│   └── Database/
│       └── DatabaseHelper.cs       ← ADO.NET (sync + async)
│
├── App.WindowsApp/
│   ├── Forms/
│   │   ├── MainForm.cs             ← Dashboard + status bar
│   │   ├── PatientForm.cs          ← CRUD + search + filter
│   │   ├── DoctorForm.cs           ← CRUD + validation
│   │   ├── AppointmentForm.cs      ← CRUD + color-coded grid
│   │   └── ChartForm.cs            ← 4 GDI+ charts
│   └── Program.cs
│
├── Database/
│   └── clinic_db.sql
└── README.md
```

---

## Setup Instructions

### Step 1 — Database
```
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database\clinic_db.sql"
```
Or open in SSMS and press F5.

### Step 2 — Connection String
Open `App.WindowsApp\Program.cs` — already set to LocalDB.
Change if needed:
- Express: `Server=.\SQLEXPRESS;Database=ClinicDB;Trusted_Connection=True;TrustServerCertificate=True;`
- LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=ClinicDB;Trusted_Connection=True;`

### Step 3 — Run
```
dotnet restore
dotnet run --project App.WindowsApp
```
Or open `.sln` in Visual Studio 2022 → F5.

---

## Group Members
| # | Name | Roll Number |
|---|------|-------------|
| 1 | (Leader) | |
| 2 | | |
| 3 | | |

**Section:** _______ | **Demo:** 9 June 2026
