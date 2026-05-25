SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @LockResult int;
EXEC @LockResult = sys.sp_getapplock
    @Resource = N'TrainingHub.SeedData',
    @LockMode = N'Exclusive',
    @LockOwner = N'Transaction',
    @LockTimeout = 10000;

IF @LockResult < 0
    THROW 50000, 'Unable to obtain the TrainingHub seed-data lock.', 1;

SET IDENTITY_INSERT [Categories] ON;
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 1)
    INSERT INTO [Categories] ([Id], [Name], [Description])
    VALUES (1, N'Programming', N'Courses about software development and programming languages.');
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 2)
    INSERT INTO [Categories] ([Id], [Name], [Description])
    VALUES (2, N'Data Science', N'Courses on data analysis, machine learning and statistics.');
SET IDENTITY_INSERT [Categories] OFF;

SET IDENTITY_INSERT [Instructors] ON;
IF NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = 1)
    INSERT INTO [Instructors] ([Id], [Name], [Email], [PhoneNumber], [Bio], [ExpertiseArea], [IsAvailable], [IsActive], [CreatedAt])
    VALUES (1, N'Alice Johnson', N'alice.johnson@example.com', N'555-0101', N'Senior .NET developer and trainer.', N'Software Development', 1, 1, '2026-01-01T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [Instructors] WHERE [Id] = 2)
    INSERT INTO [Instructors] ([Id], [Name], [Email], [PhoneNumber], [Bio], [ExpertiseArea], [IsAvailable], [IsActive], [CreatedAt])
    VALUES (2, N'Bob Smith', N'bob.smith@example.com', N'555-0202', N'Data scientist and Python instructor.', N'Data Science', 1, 1, '2026-01-01T00:00:00');
SET IDENTITY_INSERT [Instructors] OFF;

SET IDENTITY_INSERT [Classrooms] ON;
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 1)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (1, 101, 30, N'Building A - Room 101', 1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [Classrooms] WHERE [Id] = 2)
    INSERT INTO [Classrooms] ([Id], [RoomCode], [Capacity], [Location], [HasProjector], [HasLabComputer], [IsAvailable])
    VALUES (2, 202, 20, N'Building B - Lab 202', 0, 1, 1);
SET IDENTITY_INSERT [Classrooms] OFF;

SET IDENTITY_INSERT [CertificationTracks] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 1)
    INSERT INTO [CertificationTracks] ([Id], [Name], [Description], [IsActive])
    VALUES (1, N'Full Stack Developer', N'Track covering front-end and back-end development.', 1);
IF NOT EXISTS (SELECT 1 FROM [CertificationTracks] WHERE [Id] = 2)
    INSERT INTO [CertificationTracks] ([Id], [Name], [Description], [IsActive])
    VALUES (2, N'Data Analyst Track', N'Track focused on data analysis and visualization.', 1);
SET IDENTITY_INSERT [CertificationTracks] OFF;

SET IDENTITY_INSERT [Trainees] ON;
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 1)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (1, N'John Doe', N'john.doe@example.com', N'555-1001', '2026-01-10T00:00:00', 1);
IF NOT EXISTS (SELECT 1 FROM [Trainees] WHERE [Id] = 2)
    INSERT INTO [Trainees] ([Id], [FullName], [Email], [PhoneNumber], [RegisteredAt], [IsActive])
    VALUES (2, N'Jane Smith', N'jane.smith@example.com', N'555-1002', '2026-02-05T00:00:00', 1);
SET IDENTITY_INSERT [Trainees] OFF;

SET IDENTITY_INSERT [Courses] ON;
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 1)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (1, N'C# Fundamentals', N'Introductory course to C# and .NET fundamentals.', 24.0, 20, 499.99, 1, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 2)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (2, N'Python for Data Analysis', N'Using Python to analyze and visualize data.', 30.0, 25, 599.50, 1, 2, NULL);
IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Id] = 3)
    INSERT INTO [Courses] ([Id], [Title], [Description], [DurationHours], [DefaultCapacity], [Fee], [IsActive], [CategoryId], [PrerequisiteCourseId])
    VALUES (3, N'ASP.NET Core MVC', N'Building web applications using ASP.NET Core MVC.', 45.0, 20, 799.99, 1, 1, 1);
SET IDENTITY_INSERT [Courses] OFF;

SET IDENTITY_INSERT [CertificationTrackCourses] ON;
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 1)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (1, 1, 1);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 2)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (2, 2, 2);
IF NOT EXISTS (SELECT 1 FROM [CertificationTrackCourses] WHERE [Id] = 3)
    INSERT INTO [CertificationTrackCourses] ([Id], [CertificationTrackId], [CourseId]) VALUES (3, 1, 3);
SET IDENTITY_INSERT [CertificationTrackCourses] OFF;

SET IDENTITY_INSERT [CourseSessions] ON;
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 1)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (1, 1, 1, 1, '2026-06-01T00:00:00', '2026-06-10T00:00:00', 20, N'Scheduled', '2026-05-01T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 2)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (2, 2, 2, 2, '2026-07-01T00:00:00', '2026-07-15T00:00:00', 25, N'Scheduled', '2026-05-10T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [CourseSessions] WHERE [Id] = 3)
    INSERT INTO [CourseSessions] ([Id], [CourseId], [InstructorId], [ClassroomId], [StartDate], [EndDate], [Capacity], [Status], [CreatedAt])
    VALUES (3, 3, 1, 1, '2026-08-01T00:00:00', '2026-08-10T00:00:00', 20, N'Scheduled', '2026-07-01T00:00:00');
SET IDENTITY_INSERT [CourseSessions] OFF;

SET IDENTITY_INSERT [Enrollments] ON;
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 1)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (1, 1, 1, N'Completed', '2026-05-20T00:00:00', N'Present', N'Pass', '2026-06-11T00:00:00');
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 2)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (2, 2, 2, N'Enrolled', '2026-05-22T00:00:00', N'Pending', N'Pending', NULL);
IF NOT EXISTS (SELECT 1 FROM [Enrollments] WHERE [Id] = 3)
    INSERT INTO [Enrollments] ([Id], [TraineeId], [CourseSessionId], [Status], [EnrolledAt], [AttendanceStatus], [ResultStatus], [ResultRecordedAt])
    VALUES (3, 1, 3, N'Completed', '2026-07-05T00:00:00', N'Present', N'Pass', '2026-08-11T00:00:00');
SET IDENTITY_INSERT [Enrollments] OFF;

SET IDENTITY_INSERT [Payments] ON;
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 1)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (1, 1, 1, 499.99, '2026-05-21T00:00:00', N'CreditCard', N'PAY-1001', N'');
IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [Id] = 2)
    INSERT INTO [Payments] ([Id], [TraineeId], [EnrollmentId], [AmountPaid], [PaidAt], [PaymentMethod], [ReferenceNumber], [Notes])
    VALUES (2, 2, 2, 300.00, '2026-05-23T00:00:00', N'CreditCard', N'PAY-1002', N'');
SET IDENTITY_INSERT [Payments] OFF;

SET IDENTITY_INSERT [Certificates] ON;
IF NOT EXISTS (SELECT 1 FROM [Certificates] WHERE [Id] = 1)
    INSERT INTO [Certificates] ([Id], [TraineeId], [CertificationTrackId], [CertificateReferenceNumber], [IssuedAt], [Status])
    VALUES (1, 1, 1, N'CERT-0001', '2026-08-15T00:00:00', N'Issued');
SET IDENTITY_INSERT [Certificates] OFF;

SET IDENTITY_INSERT [Notifications] ON;
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 1)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (1, N'Welcome', N'Welcome to TrainingHub, John!', N'System', '2026-01-11T00:00:00', 0, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [Notifications] WHERE [Id] = 2)
    INSERT INTO [Notifications] ([Id], [Title], [Message], [Type], [CreatedAt], [IsRead], [TraineeId], [InstructorId])
    VALUES (2, N'Session Assigned', N'You have been assigned to C# Fundamentals.', N'Assignment', '2026-05-02T00:00:00', 0, NULL, 1);
SET IDENTITY_INSERT [Notifications] OFF;

COMMIT TRANSACTION;
