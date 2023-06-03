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
avatar MEDIUMBLOB
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
actual_result VARCHAR(200) NOT NULL,
steps TEXT NOT NULL,
details TEXT,
priority CHAR,
severity TINYINT,
status VARCHAR(30) DEFAULT "NEW" NOT NULL,
date DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
FOREIGN KEY (reportee) REFERENCES users(uid) ON DELETE SET NULL,
FOREIGN KEY (project) REFERENCES projects(pid) ON DELETE CASCADE
);

CREATE TABLE comments (
cid CHAR(64) PRIMARY KEY,
commenter VARCHAR(64),
bug_report CHAR(64) NOT NULL,
reply_to CHAR(64),
comment TEXT NOT NULL,
date DATETIME DEFAULT CURRENT_TIMESTAMP,
FOREIGN KEY (commenter) REFERENCES users(uid) ON DELETE SET NULL,
FOREIGN KEY (bug_report) REFERENCES bug_reports(bid) ON DELETE CASCADE,
FOREIGN KEY (reply_to) REFERENCES comments(cid) ON DELETE CASCADE
);

CREATE TABLE developments (
did CHAR(64) PRIMARY KEY,
project CHAR(64) NOT NULL,
developer VARCHAR(64),
FOREIGN KEY (project) REFERENCES projects(pid) ON DELETE CASCADE,
FOREIGN KEY (developer) REFERENCES users(uid) ON DELETE SET NULL
);

CREATE TABLE assignments (
aid CHAR(64) PRIMARY KEY,
assignee VARCHAR(64),
bug_report CHAR(64),
FOREIGN KEY (assignee) REFERENCES users(uid) ON DELETE SET NULL,
FOREIGN KEY (bug_report) REFERENCES bug_reports(bid) ON DELETE SET NULL
);

## Tutorials/Guides
Name: How to write a software requirement document (with template) </br>
Author: Team Asana </br>
Source: https://asana.com/resources/software-requirement-document-template </br>