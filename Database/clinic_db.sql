-- ============================================================
--  Clinic Appointment System — Database Setup
--  Run in SSMS or via: sqlcmd -S "(localdb)\MSSQLLocalDB" -i clinic_db.sql
-- ============================================================
USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name='ClinicDB')
    CREATE DATABASE ClinicDB;
GO
USE ClinicDB;
GO

IF OBJECT_ID('Appointments','U') IS NOT NULL DROP TABLE Appointments;
IF OBJECT_ID('Doctors','U')      IS NOT NULL DROP TABLE Doctors;
IF OBJECT_ID('Patients','U')     IS NOT NULL DROP TABLE Patients;
GO

CREATE TABLE Patients (
    PatientId    INT IDENTITY(1,1) PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Gender       NVARCHAR(10)  NOT NULL,
    DateOfBirth  DATE          NOT NULL,
    PhoneNumber  NVARCHAR(20)  NOT NULL,
    Email        NVARCHAR(100) NULL DEFAULT '',
    Address      NVARCHAR(250) NULL DEFAULT '',
    BloodGroup   NVARCHAR(5)   NULL DEFAULT '',
    RegisteredOn DATETIME      NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Doctors (
    DoctorId        INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)  NOT NULL,
    Specialization  NVARCHAR(100)  NOT NULL,
    PhoneNumber     NVARCHAR(20)   NULL DEFAULT '',
    Email           NVARCHAR(100)  NULL DEFAULT '',
    Qualification   NVARCHAR(100)  NULL DEFAULT '',
    AvailableDays   NVARCHAR(50)   NULL DEFAULT '',
    StartTime       TIME           NOT NULL DEFAULT '09:00',
    EndTime         TIME           NOT NULL DEFAULT '17:00',
    ConsultationFee DECIMAL(10,2)  NOT NULL DEFAULT 0,
    IsActive        BIT            NOT NULL DEFAULT 1
);

CREATE TABLE Appointments (
    AppointmentId   INT IDENTITY(1,1) PRIMARY KEY,
    PatientId       INT           NOT NULL REFERENCES Patients(PatientId),
    DoctorId        INT           NOT NULL REFERENCES Doctors(DoctorId),
    AppointmentDate DATE          NOT NULL,
    AppointmentTime TIME          NOT NULL,
    Reason          NVARCHAR(250) NULL DEFAULT '',
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Scheduled',
    Notes           NVARCHAR(500) NULL DEFAULT '',
    CreatedOn       DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── Sample Doctors ────────────────────────────────────────────────────────────
INSERT INTO Doctors (FullName,Specialization,PhoneNumber,Email,Qualification,AvailableDays,StartTime,EndTime,ConsultationFee,IsActive) VALUES
('Dr. Ahmad Raza',    'General Physician', '0300-1112233','ahmad@clinic.pk', 'MBBS',      'Mon,Tue,Wed,Thu,Fri','09:00','17:00',500, 1),
('Dr. Sara Khan',     'Gynecologist',      '0311-2223344','sara@clinic.pk',  'MBBS,FCPS', 'Mon,Wed,Fri',        '10:00','15:00',1500,1),
('Dr. Bilal Hussain', 'Cardiologist',      '0321-3334455','bilal@clinic.pk', 'MBBS,MD',   'Tue,Thu,Sat',        '11:00','16:00',2000,1),
('Dr. Fatima Malik',  'Dermatologist',     '0333-4445566','fatima@clinic.pk','MBBS,FCPS', 'Mon,Thu,Sat',        '09:00','14:00',1200,1),
('Dr. Usman Ali',     'Orthopedic',        '0345-5556677','usman@clinic.pk', 'MBBS,MS',   'Wed,Thu,Fri',        '08:00','13:00',2500,1);

-- ── Sample Patients ───────────────────────────────────────────────────────────
INSERT INTO Patients (FullName,Gender,DateOfBirth,PhoneNumber,Email,Address,BloodGroup) VALUES
('Muhammad Ali',    'Male',  '1990-05-15','0300-9998877','mali@gmail.com',  'House 5, Street 3, Lahore','B+'),
('Ayesha Siddiqui', 'Female','1995-08-22','0311-8887766','ayesh@gmail.com', 'Flat 2, Block A, Karachi', 'O+'),
('Hassan Tariq',    'Male',  '1985-12-01','0321-7776655','hassan@gmail.com','Plot 10, Phase 5, ISB',    'A+'),
('Zainab Noor',     'Female','2000-03-10','0333-6665544','zainab@gmail.com','House 8, Model Town, LHR', 'AB+'),
('Kamran Sheikh',   'Male',  '1978-11-20','0345-5554433','kamran@gmail.com','Street 7, Saddar, RWP',    'O-');

-- ── Sample Appointments ───────────────────────────────────────────────────────
INSERT INTO Appointments (PatientId,DoctorId,AppointmentDate,AppointmentTime,Reason,Status) VALUES
(1,1,CAST(GETDATE() AS DATE),'09:30','Routine checkup',       'Scheduled'),
(2,2,CAST(GETDATE() AS DATE),'10:00','Monthly follow-up',     'Scheduled'),
(3,3,CAST(GETDATE() AS DATE),'11:00','Chest pain evaluation', 'Completed'),
(4,4,CAST(GETDATE() AS DATE),'12:00','Skin rash consultation','Scheduled'),
(5,5,CAST(GETDATE() AS DATE),'14:00','Knee pain assessment',  'Cancelled'),
(1,2,DATEADD(DAY,-1,CAST(GETDATE() AS DATE)),'09:00','Follow-up visit','Completed'),
(2,3,DATEADD(DAY,-2,CAST(GETDATE() AS DATE)),'10:30','ECG test','Completed'),
(3,1,DATEADD(DAY,-3,CAST(GETDATE() AS DATE)),'11:30','Fever','Completed'),
(4,5,DATEADD(DAY,-4,CAST(GETDATE() AS DATE)),'13:00','Back pain','NoShow'),
(5,4,DATEADD(DAY,-5,CAST(GETDATE() AS DATE)),'15:00','Rash checkup','Completed');
GO

PRINT '============================================';
PRINT ' ClinicDB setup complete! Run the app now.';
PRINT '============================================';
