# BugTracker

## Setup
### App Dependencies Setup
...

### MySQL Database Setup
Use the following commands to set up your MySQL database:

DROP DATABASE IF EXISTS bug_tracker;
CREATE DATABASE bug_tracker;
USE bug_tracker;

CREATE TABLE users (
uid VARCHAR(64) PRIMARY KEY,
avatar BLOB
);

CREATE TABLE projects (
pid CHAR(64) PRIMARY KEY,
name VARCHAR(50) NOT NULL
);

CREATE TABLE bug_reports (
bid CHAR(64) PRIMARY KEY,
reportee VARCHAR(64),
project CHAR(64),
summary VARCHAR(150) NOT NULL,
software_version DECIMAL NOT NULL,
device VARCHAR(50),
os VARCHAR(50),
expected_result VARCHAR(200) NOT NULL,
steps TEXT NOT NULL,
details TEXT,
priority CHAR,
severity TINYINT,
status VARCHAR(30),
date DATETIME DEFAULT CURRENT_TIMESTAMP,
FOREIGN KEY (reportee) REFERENCES users(uid),
FOREIGN KEY (project) REFERENCES projects(pid)
);

CREATE TABLE comments (
cid CHAR(64) PRIMARY KEY,
commenter VARCHAR(64),
bug_report CHAR(64),
reply_to CHAR(64),
comment TEXT NOT NULL,
date DATETIME DEFAULT CURRENT_TIMESTAMP,
FOREIGN KEY (commenter) REFERENCES users(uid),
FOREIGN KEY (bug_report) REFERENCES bug_reports(bid),
FOREIGN KEY (reply_to) REFERENCES comments(cid)
);

CREATE TABLE developments (
did CHAR(64) PRIMARY KEY,
project_id CHAR(64),
user_id VARCHAR(64),
FOREIGN KEY (project_id) REFERENCES projects(pid),
FOREIGN KEY (user_id) REFERENCES users(uid)
);

CREATE TABLE assignments (
aid CHAR(64) PRIMARY KEY,
assignee VARCHAR(64),
bug_report CHAR(64),
FOREIGN KEY (assignee) REFERENCES users(uid),
FOREIGN KEY (bug_report) REFERENCES bug_reports(bid)
);

## Tutorials/Guides
Name: How to write a software requirement document (with template) </br>
Author: Team Asana </br>
Source: https://asana.com/resources/software-requirement-document-template </br>