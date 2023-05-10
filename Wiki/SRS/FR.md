# 2. Functional Requirements

## Details of Operations for Each Page

### Navigation Bar (Not Signed In)
This navigation bar is located at the top of all pages where the user is not signed in.
It provides navigation to the following pages: Home, About, Privacy, Contact, Login, and Sign Up

### Home Page
This it the first page the user sees when opening the web application and it welcomes the user.

### About Page
The about page details what the bug tracker does and its benefits.

### Privacy Page
The privacy page details how user data is handled and its security.

### Contact Page
The contact page contains the contact information of the project developer(s), so users can contact them should any questions or issues arise.

### Login Page
The login page integrates the Auth0 API, which boasts multi-factor identification and anomaly detection, for authentication and authorization.
The user can login with their unique username and password, their Google account, or their Github account.
If the user does not have a registered account, the user can select to sign up.

### Signed Out Page
After the user signs out of their account, they are directed to this page, which displays a message confirming their sign out.

### Navigation Bar (When Signed In)
This navigation bar is located at the top of every page while the user is signed in.
It provides navigation to the following pages: My Dashboard, My Profile, My Tasks

### My Dashboard Page
The first page the user sees after login welcomes the user, displaying the user's profile photo and username.
It displays the following user statistics: the number of unfinished assigned tasks, the number of assigned tasks the user has finished, and the user's status.
Beneath the statistics, projects that the user is a part of are displayed, and clicking on a project directs the user to the corresponding project's dashboard.
If the user is not part of any projects, then under the projects section is the message "Create a new project or join one".
The searchbar at the top of the page allows user to search for projects.
To the right of the searchbar, there is a New Project button, which allows the user to create a new project.

### My Profile Page
This page displays the user's profile information, which includes the user's username and profile photo.
Clicking on the edit username button allows you to enter a new username, and clicking on the upload photo button allows you to upload a new photo to change your profile photo to.
After editing your profile information, clicking the "Save" button changes your profile information or notify you of any issues with your changes.
There is also a delete account button, which when clicked on asks you to confirm the deletion before deleting your account.

### My Tasks
This page displays all the issues(bug reports) assigned to the user.
This page has two tabs, one for unfinished issues, and one for finished issues.
For both tabs, if there are no issues under the category, the page displays a message saying: "No assigned tasks found. Create a new project and/or assign yourself some issues."
Tasks(bug reports) are categorized by what project they belong to, so each task is under the section title of their respective project.

### Project Dashboard Page
This page displays the project's information, including the project name and developers.
It also displays the following statistics for the project: the number of bugs fixed, the number of bugs currently being fixed(pending bugs), and the number of new bug reports.
At the top right, there is a "Report Bug" button.
Clicking on this button displays a pop-up form where the user can fill out the details of the bug they experienced while using this project's software and submit a report.
If the user is a developer, clicking the "Delete Project" button at the bottom of the page will delete the project from the developer's account.
If the user is the only developer of the project, clicking on the delete button deletes the entire project.
Else, the developer is simply removed from the project, though their contributions remain.

### Bug Report Popup Fields
Summary: This field is for a summary of the bug. It has a word limit, and is a required field.
Software Version: This field is for the software version of the project the user was using when they experienced the bug. This is a required field.
Device: This field is for the device the user experienced the bug on. It is not a required field.
OS: This field is for the operating system the user experienced the bug on. It is not a required field.
Expected Result: This field is for what result the user expected the project's software to do. This is a required field that has a word limit.
Actual Result: This field is for what actually happened. This is a required field that has a word limit.
Steps to Reproduce Bug: This field is for the steps to reproduce the bug described. This is a required field without a word limit.
Evidence: This field is for any screenshots or videos the user may want to include. This is not a required field.
Other Details: This field is for other details that may be relevant to the bug. It does not have a word limit, and is not a required field.

### Project Bug Reports Page
This page displays all the bug reports of the project.
At the top right, there are two dropdown lists, one to choose the sort type and one to choose the sort order.
There are two sort types to choose from: sorting by date or sorting by priority or sorting by severity.
And there are two sort orders as well: descending and ascending order.
By default, bugs are sorted by date in descending order(most recent first).
Bug reports are categorized by their status, so each task is under the section title of their respective status.
Clicking on a bug report directs the user the bug report.

### Bug Report Page
This page displays the report given of a bug, as well as other information corresponding to the bug.
The other information includes the date the bug report was given, who reported it, who the bug is assigned to, 
Developers of the project can assign a severity, priority, status, and developers to the bug.
If the severity and priority of a bug is unassigned, then they are displayed with "Unassigned" until a developer assigns a severity or priority, respectively.
For priority, there are three options: low, medium, and high.
For severity, there are five: 1, 2, 3, 4, 5, where 1 is the lowest severity and 5 is the highest.
With regards to the status of a bug, the default status is "NEW" for the first week after it's been reported, unless a developer updates it.
After a week, the status changes to "Open".
Developers can also assign developers to work on the bug by clicking on the assign button beside "Assigned Developers" and selecting the developer they want to assign.
To un-assign developers, developers can click on the developer under "Assigned Developers" that they want to unassign from the bug and click on the remove button.
Finally, underneath all the bug report information is the comment section for the bug report, where users can comment on and provide feedback on bugs.

## User Classes
Users who visit the bug tracker website are able to check out the bug tracker's information and register an account with the site.
Users who have an account have the following functionalities:
- Search for a project
- View project dashboard
- Make a bug report on a project
- See bug reports made on a project
- Make comments on bug reports
- Update profile information
- Delete account
Meanwhile, developers, who are users that are part of a project, in addition to the above functionalities, have the following functionalities for projects they are part of:
- Assign and un-assign developers to work on bugs
- Assign priority level, severity level, and status to bugs
- Remove themselves from projects

## Data Handling
User account data is handled by the Auth0 service, while other data is stored in a MySQL database. The MySQL database schema is represented by the following physical database entity-relationship model:
![ER Model](/BugTracker/Wiki/SRS/https://github.com/IreneRSun/BugTracker/blob/main/Wiki/SRS/Bug%20Tracker%20Physical%20ER%20Diagram.png?raw=true "Bug Tracker Entity-Relationship Model")
Note that here we assume that emails are unique for each user and hence functions as a natural key.

## Performance Requirements
This project assumes a medium sized database.
Under a good internet connection, the following conditions should be met:
- Searching for a project should take less than 1 seconds
- Project dashboard information should take less than 3 seconds to show up
- Bug reports of a project should take less than 3 seconds to display
- Sorting bug reports should take less than 3 seconds
- Adding a bug report to the database should take less than 5 seconds
- Making a comment on a bug report should take less than 3 seconds
- Updating profile information should take less than 1 second
- Deleting you account should take take less than 1 second
- Assigning and un-assigning developers should take less than 1 second
- Assigning a priority level, severity level, or status to a bug should take less than 1 second
- Removing a user from a project should take less than 1 second
To this end, the database uses data normalization to reduce redundancy and indexing to reduce search time.

## Regulatory and Compliance Needs
Any user data is used solely for the functionalities described above and will not be disclosed to third-parties.