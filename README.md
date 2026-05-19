🎓 TrainingHub — FINAL A+ Priority Milestones Checklist

🟢 PHASE 1 — Project Foundation & Architecture

✅ Goal:

Set up a clean and stable architecture before development begins.

Tasks

Create GitHub repository

Add all group members

Create Visual Studio solution

Create 3 projects:

TrainingHub.Api
TrainingHub.Mvc
TrainingHub.Reporting

Configure project references correctly

Configure SQL Server LocalDB

Configure appsettings connection strings

Configure .gitignore

Push initial architecture commit


Finish BEFORE moving on:

Solution builds successfully
Projects reference correctly
Database connection works
Repository structure is organized


---

🟡 PHASE 2 — Database & EF Core Data Layer (VERY IMPORTANT)

✅ Goal:

Finish the entire database structure first because all features depend on it.

Tasks

Create all entity models

Configure relationships

Create DbContext

Configure Fluent API relationships

Add validation annotations

Create migrations

Update database

Seed realistic sample data


REQUIRED ENTITIES

Categories
Courses
Instructors
Classrooms
CourseSessions
Trainees
Enrollments
CertificationTracks
CertificationTrackCourses
Certificates
Payments
Notifications

Additional Tasks

Generate ERD diagram

Configure safe delete behavior

Add indexes only if useful

Add optional enhancement:

Stored procedure OR trigger


Finish BEFORE moving on:

Relationships work correctly
Database created successfully
Seed data visible
No migration errors
ERD completed


---

🟠 PHASE 3 — MVC CRUD System

✅ Goal:

Get ALL scaffolded MVC pages fully functional.

Scaffold ALL entities

Category
Course
Instructor
Classroom
CourseSession
Trainee
Enrollment
CertificationTrack
CertificationTrackCourse
Certificate
Payment
Notification


---

Fix scaffolded pages

Test:

Create
Edit
Delete
Details
Index

for ALL entities.


---

Configure navigation

Update:

_Layout.cshtml

Add navigation links for:

Courses
Sessions
Enrollments
Certifications
Payments

Finish BEFORE moving on:

All CRUD pages work
Dropdowns work correctly
Relationships display correctly
Navigation works
No runtime errors


---

🔵 PHASE 4 — Core Business Logic & Edge Cases (MOST IMPORTANT)

✅ Goal:

Implement the REAL business requirements from the brief.


---

📅 Course Scheduling Logic

CourseSession Create/Edit

Implement:

Prevent instructor double-booking

Prevent classroom double-booking

Prevent room capacity violations

Validate scheduling conflicts



---

🧑‍🎓 Enrollment Lifecycle Logic

Enrollment Create/Edit

Implement:

Prevent duplicate enrollment

Prevent full-session enrollment

Validate prerequisite completion


Enrollment statuses

Enrolled
Confirmed
Attending
Completed
Dropped


---

🏆 Certification Logic

Implement:

Certification track requirements

Progress tracking

Eligibility calculation

Certificate generation



---

💳 Payment Logic

Implement:

Partial/full payments

Outstanding balances

Overdue indication



---

🔔 Notification Logic

Implement:

In-system notifications

Enrollment notifications

Assignment notifications

Certification notifications



---

⚠️ Edge Case Handling

Implement:

Prevent enrollment without prerequisite

Prevent enrollment in full session

Prevent duplicate enrollment

Prevent instructor schedule conflicts

Prevent classroom schedule conflicts

Prevent invalid certificate lookup

Handle overdue payments


Finish BEFORE moving on:

Business rules fully validated
Invalid actions blocked
Proper error messages shown
Edge cases handled correctly


---

🟣 PHASE 5 — Authentication & Authorization

✅ Goal:

Secure the system properly.

Tasks

ASP.NET Identity setup

Register

Login

Logout

Password hashing

Role management


Create roles

Training Coordinator
Instructor
Trainee

Add:

Role-based authorization

Restricted pages/actions

Hidden unauthorized UI

Role-based navigation


Finish BEFORE moving on:

Users can authenticate
Roles work correctly
Unauthorized access blocked
Protected pages secured


---

🟤 PHASE 6 — Web API + JWT

✅ Goal:

Build ONLY the APIs required by the brief.

IMPORTANT ARCHITECTURE RULE

MVC CRUD uses EF Core directly.
ONLY the public lookup page uses HttpClient.


---

Create minimum 5 REST APIs

Recommended APIs

Courses API
CourseSessions API
Enrollments API
Certificates API
Certification Lookup API

Add:

JWT authentication

Proper HTTP status codes

REST conventions

Swagger testing

JSON responses



---

IMPORTANT AUTH RULE

All APIs require JWT EXCEPT the public certification lookup endpoint.
The certification lookup endpoint must use [AllowAnonymous].

Finish BEFORE moving on:

JWT works
Swagger works
Protected endpoints secured
API responses tested


---

🟢 PHASE 7 — Public Certification Lookup (REQUIRED)

✅ Goal:

Implement the REQUIRED HttpClient feature.

Create:

Public certification verification page

Requirements

No login required

Uses:

HttpClient

Calls API endpoint

Validates:

Trainee ID
Certificate Reference Number


Display:

Certification status
Completed courses
Certificate details

Finish BEFORE moving on:

MVC successfully consumes API
Lookup works publicly
Invalid references handled


---

🔴 PHASE 8 — SignalR Real-Time Feature (10%)

✅ Goal:

Implement required real-time functionality EARLY.

BEST OPTION

Live Enrollment Counter

Implement:

Remaining spots update instantly

Real-time enrollment updates

SignalR Hub

Broadcast enrollment changes


Optional

Real-time notifications

Finish BEFORE moving on:

SignalR updates work correctly
Enrollment counts sync properly
Works locally before deployment


---

🟠 PHASE 9 — Reporting Application

✅ Goal:

Build separate reporting client using ONLY APIs.

REQUIREMENTS

Separate application

No direct database access

Uses HttpClient only

Uses JWT authentication



---

ACCESS CONTROL

ONLY Training Coordinator users can access the reporting system.
Instructor and Trainee users must receive Access Denied.


---

Reports

Enrollment statistics
Revenue reports
Instructor workload
Certification completion rates
Popular courses
Outstanding balances


---

Optional Enhancement

Dashboard charts using Chart.js

Finish BEFORE moving on:

Reports load successfully
JWT authentication works
No direct EF Core access exists
Role restrictions work correctly


---

🟣 PHASE 10 — UI / UX Polish

✅ Goal:

Improve presentation AFTER functionality is stable.

Tasks

Responsive design

Better navigation

Dashboard UI

Bootstrap cards

Alerts/messages

Better forms

Search/filter UI

Pagination

Loading indicators

Cleaner layouts


Finish BEFORE moving on:

UI consistent
Mobile-friendly
Professional appearance


---

☁️ PHASE 11 — Azure Deployment

✅ Goal:

Deploy the complete system successfully.

Deploy:

MVC app

API app

Reporting app

Azure SQL Database


Verify:

Public URLs work
APIs work
SignalR works
Connection strings work
JWT works in production

Finish BEFORE moving on:

All applications deployed successfully
Production database works
Public testing successful


---

🔎 PHASE 12 — Testing & Documentation

✅ Goal:

Prepare a professional final submission package.


---

Documentation

Create:

Test plan

README

ERD screenshot

Deployment URLs

API endpoint summary table

Enhancement screenshots

System walkthrough screenshots



---

REQUIRED API TABLE

Include:

Route
HTTP Method
Purpose
Authentication Requirement


---

REQUIRED SQL SCRIPTS

Generate:

CreateSchema.sql
SeedData.sql


---

REQUIRED NUGET PACKAGES TABLE

Include:

Package Name
Purpose

Example:

Microsoft.EntityFrameworkCore.SqlServer
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.SignalR
Swashbuckle.AspNetCore


---

Testing

Test:

Authentication
Authorization
CRUD
Scheduling
Enrollment lifecycle
Certification generation
Payments
Reporting
APIs
SignalR

Include:

Expected Result
Actual Result
Status


---

📦 PHASE 13 — Final Submission

✅ Goal:

Finalize and package everything correctly.

Final Tasks

Push final GitHub version

Verify commit history

Export final PDF report

Include deployment URLs

Include test accounts

Include screenshots

Zip submission correctly



---

👤 PHASE 14 — Individual Reflection

Each member writes:

500–800 word reflection

Include:

Contributions

Git commits

Technical decisions

Challenges/debugging process

Teamwork

Improvements for future projects

Screenshots/code snippets
