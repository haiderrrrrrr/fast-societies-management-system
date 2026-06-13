-- ========================================================
-- FAST Societies Management System - One Time SQL Server Setup
-- Target: SQL Server Express / SQL Server Developer on Windows
-- Run once in SSMS or Azure Data Studio. This file replaces all migrations.
-- ========================================================

IF DB_ID('FASTSocietiesDB') IS NULL
BEGIN
    CREATE DATABASE FASTSocietiesDB;
END
GO

USE FASTSocietiesDB;
GO

-- ====================
-- LOOKUP TABLES
-- ====================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleID INT IDENTITY(1,1) CONSTRAINT PK_Roles PRIMARY KEY,
        RoleName VARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.MembershipStatus', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MembershipStatus (
        StatusID INT IDENTITY(1,1) CONSTRAINT PK_MembershipStatus PRIMARY KEY,
        StatusName VARCHAR(50) NOT NULL CONSTRAINT UQ_MembershipStatus_StatusName UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.EventStatus', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventStatus (
        StatusID INT IDENTITY(1,1) CONSTRAINT PK_EventStatus PRIMARY KEY,
        StatusName VARCHAR(50) NOT NULL CONSTRAINT UQ_EventStatus_StatusName UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.SocietyStatus', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SocietyStatus (
        StatusID INT IDENTITY(1,1) CONSTRAINT PK_SocietyStatus PRIMARY KEY,
        StatusName VARCHAR(50) NOT NULL CONSTRAINT UQ_SocietyStatus_StatusName UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.TaskStatus', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskStatus (
        StatusID INT IDENTITY(1,1) CONSTRAINT PK_TaskStatus PRIMARY KEY,
        StatusName VARCHAR(50) NOT NULL CONSTRAINT UQ_TaskStatus_StatusName UNIQUE
    );
END
GO

INSERT INTO dbo.Roles (RoleName)
SELECT v.RoleName
FROM (VALUES ('Admin'), ('SocietyHead'), ('Student')) v(RoleName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.RoleName = v.RoleName);

INSERT INTO dbo.MembershipStatus (StatusName)
SELECT v.StatusName
FROM (VALUES ('Pending'), ('Approved'), ('Rejected'), ('Suspended')) v(StatusName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.MembershipStatus s WHERE s.StatusName = v.StatusName);

INSERT INTO dbo.EventStatus (StatusName)
SELECT v.StatusName
FROM (VALUES ('Draft'), ('PendingApproval'), ('Approved'), ('Cancelled'), ('Completed')) v(StatusName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.EventStatus s WHERE s.StatusName = v.StatusName);

INSERT INTO dbo.SocietyStatus (StatusName)
SELECT v.StatusName
FROM (VALUES ('Pending'), ('Active'), ('Suspended')) v(StatusName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SocietyStatus s WHERE s.StatusName = v.StatusName);

INSERT INTO dbo.TaskStatus (StatusName)
SELECT v.StatusName
FROM (VALUES ('ToDo'), ('InProgress'), ('Completed')) v(StatusName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TaskStatus s WHERE s.StatusName = v.StatusName);
GO

-- ====================
-- CORE TABLES
-- ====================
IF OBJECT_ID('dbo.DatabaseSchemaVersion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DatabaseSchemaVersion (
        VersionID INT IDENTITY(1,1) CONSTRAINT PK_DatabaseSchemaVersion PRIMARY KEY,
        VersionName VARCHAR(100) NOT NULL CONSTRAINT UQ_DatabaseSchemaVersion_VersionName UNIQUE,
        AppliedAt DATETIME NOT NULL CONSTRAINT DF_DatabaseSchemaVersion_AppliedAt DEFAULT GETDATE(),
        Notes VARCHAR(500) NULL
    );
END
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserID INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
        Username VARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        Email VARCHAR(150) NULL CONSTRAINT UQ_Users_Email UNIQUE,
        PasswordHash VARCHAR(256) NOT NULL,
        Salt VARCHAR(256) NOT NULL,
        RoleID INT NOT NULL CONSTRAINT FK_Users_Roles REFERENCES dbo.Roles(RoleID),
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL
    );

    CREATE INDEX IX_Users_Username ON dbo.Users(Username);
    CREATE INDEX IX_Users_RoleID ON dbo.Users(RoleID);
END
GO

IF OBJECT_ID('dbo.StudentProfiles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentProfiles (
        StudentID INT IDENTITY(1,1) CONSTRAINT PK_StudentProfiles PRIMARY KEY,
        UserID INT NOT NULL CONSTRAINT UQ_StudentProfiles_UserID UNIQUE
            CONSTRAINT FK_StudentProfiles_Users REFERENCES dbo.Users(UserID),
        FullName VARCHAR(100) NOT NULL,
        RollNumber VARCHAR(50) NOT NULL CONSTRAINT UQ_StudentProfiles_RollNumber UNIQUE,
        Department VARCHAR(100) NOT NULL,
        Email VARCHAR(150) NOT NULL CONSTRAINT UQ_StudentProfiles_Email UNIQUE,
        IsDeleted BIT NOT NULL CONSTRAINT DF_StudentProfiles_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_StudentProfiles_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL
    );
END
GO

IF OBJECT_ID('dbo.Societies', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Societies (
        SocietyID INT IDENTITY(1,1) CONSTRAINT PK_Societies PRIMARY KEY,
        Name VARCHAR(150) NOT NULL CONSTRAINT UQ_Societies_Name UNIQUE,
        Description VARCHAR(MAX) NOT NULL,
        HeadUserID INT NULL CONSTRAINT FK_Societies_HeadUser REFERENCES dbo.Users(UserID),
        StatusID INT NOT NULL CONSTRAINT FK_Societies_SocietyStatus REFERENCES dbo.SocietyStatus(StatusID),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Societies_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Societies_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        RowVer ROWVERSION
    );

    CREATE INDEX IX_Societies_StatusID ON dbo.Societies(StatusID);
    CREATE INDEX IX_Societies_HeadUserID ON dbo.Societies(HeadUserID);
END
GO

IF OBJECT_ID('dbo.Venues', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venues (
        VenueID INT IDENTITY(1,1) CONSTRAINT PK_Venues PRIMARY KEY,
        Name VARCHAR(100) NOT NULL CONSTRAINT UQ_Venues_Name UNIQUE,
        Capacity INT NOT NULL CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Venues_IsDeleted DEFAULT 0
    );
END
GO

IF OBJECT_ID('dbo.Memberships', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Memberships (
        MembershipID INT IDENTITY(1,1) CONSTRAINT PK_Memberships PRIMARY KEY,
        StudentID INT NOT NULL CONSTRAINT FK_Memberships_StudentProfiles REFERENCES dbo.StudentProfiles(StudentID),
        SocietyID INT NOT NULL CONSTRAINT FK_Memberships_Societies REFERENCES dbo.Societies(SocietyID),
        StatusID INT NOT NULL CONSTRAINT FK_Memberships_MembershipStatus REFERENCES dbo.MembershipStatus(StatusID),
        Role VARCHAR(50) NOT NULL CONSTRAINT DF_Memberships_Role DEFAULT 'Member',
        JoinedDate DATETIME NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Memberships_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Memberships_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        RowVer ROWVERSION,
        CONSTRAINT UQ_Memberships_Student_Society UNIQUE (StudentID, SocietyID)
    );

    CREATE INDEX IX_Memberships_SocietyID_StatusID ON dbo.Memberships(SocietyID, StatusID);
    CREATE INDEX IX_Memberships_StudentID ON dbo.Memberships(StudentID);
END
GO

IF OBJECT_ID('dbo.Events', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Events (
        EventID INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
        SocietyID INT NOT NULL CONSTRAINT FK_Events_Societies REFERENCES dbo.Societies(SocietyID),
        Title VARCHAR(200) NOT NULL,
        Description VARCHAR(MAX) NOT NULL,
        VenueID INT NOT NULL CONSTRAINT FK_Events_Venues REFERENCES dbo.Venues(VenueID),
        EventDate DATETIME NOT NULL,
        MaxCapacity INT NOT NULL CONSTRAINT CK_Events_MaxCapacity CHECK (MaxCapacity > 0),
        StatusID INT NOT NULL CONSTRAINT FK_Events_EventStatus REFERENCES dbo.EventStatus(StatusID),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Events_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        RowVer ROWVERSION
    );

    CREATE INDEX IX_Events_EventDate ON dbo.Events(EventDate);
    CREATE INDEX IX_Events_SocietyID_StatusID ON dbo.Events(SocietyID, StatusID);
END
GO

IF OBJECT_ID('dbo.EventRegistrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventRegistrations (
        RegistrationID INT IDENTITY(1,1) CONSTRAINT PK_EventRegistrations PRIMARY KEY,
        EventID INT NOT NULL CONSTRAINT FK_EventRegistrations_Events REFERENCES dbo.Events(EventID),
        StudentID INT NOT NULL CONSTRAINT FK_EventRegistrations_StudentProfiles REFERENCES dbo.StudentProfiles(StudentID),
        TicketNumber UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EventRegistrations_TicketNumber DEFAULT NEWID()
            CONSTRAINT UQ_EventRegistrations_TicketNumber UNIQUE,
        RegistrationDate DATETIME NOT NULL CONSTRAINT DF_EventRegistrations_RegistrationDate DEFAULT GETDATE(),
        IsDeleted BIT NOT NULL CONSTRAINT DF_EventRegistrations_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EventRegistrations_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CONSTRAINT UQ_EventRegistrations_Event_Student UNIQUE(EventID, StudentID)
    );

    CREATE INDEX IX_EventRegistrations_StudentID ON dbo.EventRegistrations(StudentID);
END
GO

IF OBJECT_ID('dbo.Tasks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks (
        TaskID INT IDENTITY(1,1) CONSTRAINT PK_Tasks PRIMARY KEY,
        SocietyID INT NOT NULL CONSTRAINT FK_Tasks_Societies REFERENCES dbo.Societies(SocietyID),
        AssignedToStudentID INT NOT NULL CONSTRAINT FK_Tasks_StudentProfiles REFERENCES dbo.StudentProfiles(StudentID),
        Title VARCHAR(200) NOT NULL,
        Description VARCHAR(MAX) NULL,
        StatusID INT NOT NULL CONSTRAINT FK_Tasks_TaskStatus REFERENCES dbo.TaskStatus(StatusID),
        DueDate DATETIME NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Tasks_IsDeleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        RowVer ROWVERSION
    );

    CREATE INDEX IX_Tasks_SocietyID ON dbo.Tasks(SocietyID);
    CREATE INDEX IX_Tasks_AssignedToStudentID ON dbo.Tasks(AssignedToStudentID);
END
GO

IF OBJECT_ID('dbo.Announcements', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Announcements (
        AnnouncementID INT IDENTITY(1,1) CONSTRAINT PK_Announcements PRIMARY KEY,
        SocietyID INT NOT NULL CONSTRAINT FK_Announcements_Societies REFERENCES dbo.Societies(SocietyID),
        Title VARCHAR(200) NOT NULL,
        Message VARCHAR(MAX) NOT NULL,
        PublishedAt DATETIME NOT NULL CONSTRAINT DF_Announcements_PublishedAt DEFAULT GETDATE(),
        IsDeleted BIT NOT NULL CONSTRAINT DF_Announcements_IsDeleted DEFAULT 0,
        CreatedBy INT NOT NULL CONSTRAINT FK_Announcements_Users REFERENCES dbo.Users(UserID)
    );
END
GO

IF OBJECT_ID('dbo.EventAttendance', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventAttendance (
        AttendanceID INT IDENTITY(1,1) CONSTRAINT PK_EventAttendance PRIMARY KEY,
        EventID INT NOT NULL CONSTRAINT FK_EventAttendance_Events REFERENCES dbo.Events(EventID),
        StudentID INT NOT NULL CONSTRAINT FK_EventAttendance_StudentProfiles REFERENCES dbo.StudentProfiles(StudentID),
        IsPresent BIT NOT NULL CONSTRAINT DF_EventAttendance_IsPresent DEFAULT 0,
        MarkedAt DATETIME NOT NULL CONSTRAINT DF_EventAttendance_MarkedAt DEFAULT GETDATE(),
        MarkedBy INT NOT NULL CONSTRAINT FK_EventAttendance_Users REFERENCES dbo.Users(UserID),
        CONSTRAINT UQ_EventAttendance_Event_Student UNIQUE(EventID, StudentID)
    );
END
GO

IF OBJECT_ID('dbo.EventFeedback', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventFeedback (
        FeedbackID INT IDENTITY(1,1) CONSTRAINT PK_EventFeedback PRIMARY KEY,
        EventID INT NOT NULL CONSTRAINT FK_EventFeedback_Events REFERENCES dbo.Events(EventID),
        StudentID INT NOT NULL CONSTRAINT FK_EventFeedback_StudentProfiles REFERENCES dbo.StudentProfiles(StudentID),
        Rating INT NOT NULL CONSTRAINT CK_EventFeedback_Rating CHECK (Rating BETWEEN 1 AND 5),
        Comments VARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EventFeedback_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT UQ_EventFeedback_Event_Student UNIQUE(EventID, StudentID)
    );
END
GO

IF OBJECT_ID('dbo.SystemAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemAuditLogs (
        LogID INT IDENTITY(1,1) CONSTRAINT PK_SystemAuditLogs PRIMARY KEY,
        LogLevel VARCHAR(20) NOT NULL,
        SourceContext VARCHAR(150) NULL,
        Message VARCHAR(MAX) NOT NULL,
        IPAddress VARCHAR(50) NULL,
        MachineName VARCHAR(150) NULL,
        UserID INT NULL CONSTRAINT FK_SystemAuditLogs_Users REFERENCES dbo.Users(UserID),
        Timestamp DATETIME NOT NULL CONSTRAINT DF_SystemAuditLogs_Timestamp DEFAULT GETDATE()
    );
END
GO

-- Compatibility guards for databases created from an older script.
IF COL_LENGTH('dbo.Users', 'Email') IS NULL
    ALTER TABLE dbo.Users ADD Email VARCHAR(150) NULL;
GO

IF COL_LENGTH('dbo.StudentProfiles', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.StudentProfiles ADD Email VARCHAR(150) NULL;
    UPDATE dbo.StudentProfiles
    SET Email = CONCAT(RollNumber, '@fast.edu.pk')
    WHERE Email IS NULL;
    ALTER TABLE dbo.StudentProfiles ALTER COLUMN Email VARCHAR(150) NOT NULL;
END
GO

-- ====================
-- REQUIRED STARTER DATA
-- ====================
DECLARE @AdminRoleID INT = (SELECT RoleID FROM dbo.Roles WHERE RoleName = 'Admin');
DECLARE @HeadRoleID INT = (SELECT RoleID FROM dbo.Roles WHERE RoleName = 'SocietyHead');
DECLARE @ActiveSocietyStatusID INT = (SELECT StatusID FROM dbo.SocietyStatus WHERE StatusName = 'Active');

-- admin password: admin123
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Users (Username, Email, PasswordHash, Salt, RoleID, IsActive)
    VALUES ('admin', 'admin@fast.edu.pk', '3BM7qX/YxO4U8Eie/PKkpDMNjz0rcGURng9ZMD09AV4=', 'KBDm6YcFQJVuqIs4LSlEHg==', @AdminRoleID, 1);
END

INSERT INTO dbo.Venues (Name, Capacity)
SELECT v.Name, v.Capacity
FROM (VALUES
    ('Main Auditorium', 450),
    ('CS Seminar Hall', 120),
    ('Sports Ground', 1000),
    ('Library Conference Room', 80),
    ('Media Lab', 60)
) v(Name, Capacity)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Venues existing WHERE existing.Name = v.Name);

-- Society head starter accounts. Admin can also create more from the frontend.
-- password for all three: Head1234
INSERT INTO dbo.Users (Username, Email, PasswordHash, Salt, RoleID, IsActive)
SELECT v.Username, v.Email, 'dTEgAIdo816NV5tOeRpUPN5yq0ENcBsMIhQ2CRhYqf4=', 'AAAAAAAAAAAAAAAAAAAAAA==', @HeadRoleID, 1
FROM (VALUES
    ('gaming.head', 'gaming.head@fast.edu.pk'),
    ('sports.head', 'sports.head@fast.edu.pk'),
    ('dev.head', 'dev.head@fast.edu.pk')
) v(Username, Email)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Username = v.Username);

INSERT INTO dbo.Societies (Name, Description, HeadUserID, StatusID, CreatedBy)
SELECT v.Name, v.Description, u.UserID, @ActiveSocietyStatusID, NULL
FROM (VALUES
    ('Gaming Society', 'Organizes esports tournaments, game development meetups, and recreational campus gaming activities.', 'gaming.head'),
    ('Sports Society', 'Coordinates sports competitions, tryouts, inter-department matches, and fitness campaigns.', 'sports.head'),
    ('Developers Club', 'Runs coding workshops, hackathons, tech talks, and software project teams.', 'dev.head'),
    ('Literary Society', 'Manages debates, writing circles, poetry sessions, and literary competitions.', NULL),
    ('Media Society', 'Handles photography, videography, campus coverage, and creative media campaigns.', NULL)
) v(Name, Description, HeadUsername)
LEFT JOIN dbo.Users u ON u.Username = v.HeadUsername
WHERE NOT EXISTS (SELECT 1 FROM dbo.Societies s WHERE s.Name = v.Name);
GO

-- ====================
-- STORED PROCEDURES
-- ====================
CREATE OR ALTER PROCEDURE dbo.sp_RegisterForEvent
    @EventID INT,
    @StudentID INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentCapacity INT;
        DECLARE @MaxCapacity INT;
        DECLARE @EventStatusID INT;
        DECLARE @SocietyID INT;
        DECLARE @ApprovedEventStatusID INT = (SELECT StatusID FROM dbo.EventStatus WHERE StatusName = 'Approved');
        DECLARE @ApprovedMembershipStatusID INT = (SELECT StatusID FROM dbo.MembershipStatus WHERE StatusName = 'Approved');

        SELECT
            @MaxCapacity = e.MaxCapacity,
            @EventStatusID = e.StatusID,
            @SocietyID = e.SocietyID
        FROM dbo.Events e WITH (UPDLOCK, HOLDLOCK)
        WHERE e.EventID = @EventID AND e.IsDeleted = 0;

        IF @MaxCapacity IS NULL
            THROW 50000, 'Event was not found.', 1;

        IF @EventStatusID <> @ApprovedEventStatusID
            THROW 50001, 'Event is not open for registration.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Events WHERE EventID = @EventID AND EventDate < GETDATE())
            THROW 50002, 'Event has already occurred.', 1;

        SELECT @CurrentCapacity = COUNT(*)
        FROM dbo.EventRegistrations
        WHERE EventID = @EventID AND IsDeleted = 0;

        IF @CurrentCapacity >= @MaxCapacity
            THROW 50003, 'Event has reached maximum capacity.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Memberships
            WHERE StudentID = @StudentID
              AND SocietyID = @SocietyID
              AND StatusID = @ApprovedMembershipStatusID
              AND IsDeleted = 0
        )
            THROW 50004, 'Only approved members of the society can register.', 1;

        INSERT INTO dbo.EventRegistrations (EventID, StudentID, CreatedBy)
        VALUES (@EventID, @StudentID, @CreatedBy);

        COMMIT TRANSACTION;
        SELECT 'Success' AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ====================
-- REPORTING VIEWS
-- ====================
CREATE OR ALTER VIEW dbo.vw_SocietyMembershipSummary
AS
SELECT
    s.SocietyID,
    s.Name AS SocietyName,
    ss.StatusName AS SocietyStatus,
    COALESCE(u.Username, 'Unassigned') AS HeadUsername,
    COUNT(DISTINCT CASE WHEN ms.StatusName = 'Approved' THEN m.MembershipID END) AS ApprovedMembers,
    COUNT(DISTINCT CASE WHEN ms.StatusName = 'Pending' THEN m.MembershipID END) AS PendingRequests,
    COUNT(DISTINCT CASE WHEN ms.StatusName = 'Rejected' THEN m.MembershipID END) AS RejectedRequests
FROM dbo.Societies s
INNER JOIN dbo.SocietyStatus ss ON ss.StatusID = s.StatusID
LEFT JOIN dbo.Users u ON u.UserID = s.HeadUserID
LEFT JOIN dbo.Memberships m ON m.SocietyID = s.SocietyID AND m.IsDeleted = 0
LEFT JOIN dbo.MembershipStatus ms ON ms.StatusID = m.StatusID
WHERE s.IsDeleted = 0
GROUP BY s.SocietyID, s.Name, ss.StatusName, COALESCE(u.Username, 'Unassigned');
GO

CREATE OR ALTER VIEW dbo.vw_EventRegistrationSummary
AS
SELECT
    e.EventID,
    e.Title,
    s.Name AS SocietyName,
    v.Name AS VenueName,
    e.EventDate,
    es.StatusName AS EventStatus,
    e.MaxCapacity,
    COUNT(er.RegistrationID) AS RegisteredCount,
    e.MaxCapacity - COUNT(er.RegistrationID) AS SeatsRemaining
FROM dbo.Events e
INNER JOIN dbo.Societies s ON s.SocietyID = e.SocietyID
INNER JOIN dbo.Venues v ON v.VenueID = e.VenueID
INNER JOIN dbo.EventStatus es ON es.StatusID = e.StatusID
LEFT JOIN dbo.EventRegistrations er ON er.EventID = e.EventID AND er.IsDeleted = 0
WHERE e.IsDeleted = 0
GROUP BY e.EventID, e.Title, s.Name, v.Name, e.EventDate, es.StatusName, e.MaxCapacity;
GO

CREATE OR ALTER VIEW dbo.vw_StudentMembershipStatus
AS
SELECT
    sp.StudentID,
    sp.FullName,
    sp.RollNumber,
    sp.Department,
    s.SocietyID,
    s.Name AS SocietyName,
    ms.StatusName AS MembershipStatus,
    m.Role AS SocietyRole,
    m.CreatedAt AS RequestedAt,
    m.JoinedDate
FROM dbo.Memberships m
INNER JOIN dbo.StudentProfiles sp ON sp.StudentID = m.StudentID
INNER JOIN dbo.Societies s ON s.SocietyID = m.SocietyID
INNER JOIN dbo.MembershipStatus ms ON ms.StatusID = m.StatusID
WHERE m.IsDeleted = 0 AND sp.IsDeleted = 0 AND s.IsDeleted = 0;
GO

CREATE OR ALTER VIEW dbo.vw_UniversityActivityReport
AS
SELECT
    COUNT(DISTINCT s.SocietyID) AS TotalSocieties,
    COUNT(DISTINCT CASE WHEN ss.StatusName = 'Active' THEN s.SocietyID END) AS ActiveSocieties,
    COUNT(DISTINCT sp.StudentID) AS RegisteredStudents,
    COUNT(DISTINCT CASE WHEN ms.StatusName = 'Approved' THEN m.MembershipID END) AS ApprovedMemberships,
    COUNT(DISTINCT e.EventID) AS TotalEvents,
    COUNT(DISTINCT CASE WHEN es.StatusName = 'Approved' THEN e.EventID END) AS ApprovedEvents,
    COUNT(DISTINCT CASE WHEN es.StatusName = 'PendingApproval' THEN e.EventID END) AS PendingEvents,
    COUNT(DISTINCT er.RegistrationID) AS EventRegistrations,
    CAST(ISNULL(AVG(CAST(ef.Rating AS DECIMAL(10,2))), 0) AS DECIMAL(10,2)) AS AverageFeedback
FROM dbo.Societies s
INNER JOIN dbo.SocietyStatus ss ON ss.StatusID = s.StatusID
LEFT JOIN dbo.Memberships m ON m.SocietyID = s.SocietyID AND m.IsDeleted = 0
LEFT JOIN dbo.MembershipStatus ms ON ms.StatusID = m.StatusID
LEFT JOIN dbo.StudentProfiles sp ON sp.StudentID = m.StudentID AND sp.IsDeleted = 0
LEFT JOIN dbo.Events e ON e.SocietyID = s.SocietyID AND e.IsDeleted = 0
LEFT JOIN dbo.EventStatus es ON es.StatusID = e.StatusID
LEFT JOIN dbo.EventRegistrations er ON er.EventID = e.EventID AND er.IsDeleted = 0
LEFT JOIN dbo.EventFeedback ef ON ef.EventID = e.EventID
WHERE s.IsDeleted = 0;
GO

INSERT INTO dbo.DatabaseSchemaVersion (VersionName, Notes)
SELECT '2026-05-10-one-time-enterprise-schema',
       'Single setup file for tables, required seed data, stored procedures, reporting views, and enterprise workflows.'
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.DatabaseSchemaVersion
    WHERE VersionName = '2026-05-10-one-time-enterprise-schema'
);
GO
