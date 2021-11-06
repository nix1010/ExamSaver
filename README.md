## About

Exam saver is a client-server application which main purpose is to provide a way for students to send their exam files to the professors so that they could review them.

Professors have the ability to create (schedule) an exam or update an existing exam and later view and download all exam files that students have uploaded for that specific exam.

It also has option to check for exam similarities among students for some specific exam. For that [Moss](http://theory.stanford.edu/~aiken/moss/) (Measure of software similarity) is used. It works in such way that professors are required to specify file extension for which files they are interested to check similarities. As a result HTML page is provided by Moss which shows similarities between students' work.

Students can upload single file. Multiple files can be uploaded by compressing them to `.zip` format and then uploading that one file.

Exam files are stored on the server in archived format (`.zip` file extension).
Each student exam files are stored in separate folder named: `<examId>-<studentId>-<firstName>-<lastName>-<index>`.

Where:
- `<examId>` - Id of the exam in the database
- `<studentId>` - Id of the student in the database
- `<firstName>` - First name of the student
- `<lastName>` - Last name of the student
- `<index>` - Student faculty index number


## Requirements
- ASP.NET Core 3.1
- Angular 10
- SQL Server
- Perl (binary file location must be available in PATH environment variable)

## Configuration
Location where exam files are stored on the server can be specified in `appSettings.json` file, section "**AppSettings**" > "**ExamsDirectoryPath**".
Relative or absolute paths can be used.

## Testing
There are 3 users available to test the application:
1. Milos (Professor and student)
    - Email: milos@gmail.com
    - Password: pass1
2. Petar (Professor)
    - Email: petar@gmail.com
    - Password: pass1
3. Ljuba (Student)
    - Email: ljuba@gmail.com
    - Password: pass1