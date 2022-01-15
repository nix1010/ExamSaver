using ExamSaver.Models;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using System;
using System.Collections.Generic;

namespace ExamSaver.Data
{
    public class DatabaseInitializer
    {
        public static void Initialize(DatabaseContext databaseContext)
        {
            if (databaseContext.Database.EnsureCreated())
            {
                PopulateDatabase(databaseContext);
            }
        }

        public static void PopulateDatabase(DatabaseContext databaseContext)
        {
            Role[] roles = new Role[]
            {
                new Role()
                {
                    Name = RoleType.PROFESSOR
                },
                new Role()
                {
                    Name = RoleType.STUDENT
                }
            };

            foreach (Role RoleEntity in roles)
            {
                databaseContext.Roles.Add(RoleEntity);
            }

            Subject[] subjects = new Subject[]
            {
                new Subject()
                {
                    ESPB = 7,
                    Name = "Teorija programskih jezika"
                },
                new Subject()
                {
                    ESPB = 8,
                    Name = "Razvoj veb aplikacija"
                },
                new Subject()
                {
                    ESPB = 7,
                    Name = "Razvoj mobilnih aplikacija"
                },
                new Subject()
                {
                    ESPB = 5,
                    Name = "Engleski jezik 1"
                },
                new Subject()
                {
                    ESPB = 8,
                    Name = "Dizajn softvera"
                }
            };

            foreach (Subject subject in subjects)
            {
                databaseContext.Subjects.Add(subject);
            }

            User[] users = new User[]
            {
                new User()
                {
                    FirstName = "Petar",
                    LastName = "Petrovic",
                    Email = "petar@gmail.com",
                    Password = Util.Encrypt("pass1"),
                    Roles = new List<Role>(new Role[]{ roles[0] })
                },
                new User()
                {
                    FirstName = "Milos",
                    LastName = "Milakovic",
                    Email = "milos@gmail.com",
                    Password = Util.Encrypt("pass1"),
                    Roles = new List<Role>(new Role[]{ roles[0], roles[1] })
                },
                new User()
                {
                    FirstName = "Ljuba",
                    LastName = "Ljubic",
                    Email = "ljuba@gmail.com",
                    Password = Util.Encrypt("pass1"),
                    Roles = new List<Role>(new Role[]{ roles[1] })
                }
            };

            foreach (User user in users)
            {
                databaseContext.Users.Add(user);
            }

            Student[] students = new Student[]
            {
                new Student()
                {
                    Index = "145",
                    User = users[1]
                },
                new Student()
                {
                    Index = "146",
                    User = users[2]
                }
            };

            foreach (Student student in students)
            {
                databaseContext.Students.Add(student);
            }

            UserSubject[] userSubjects = new UserSubject[]
            {
                new UserSubject()
                {
                    User = users[0],
                    Subject = subjects[0],
                    SubjectRelation = SubjectRelationType.TEACHING
                },
                new UserSubject()
                {
                    User = users[0],
                    Subject = subjects[1],
                    SubjectRelation = SubjectRelationType.TEACHING
                },
                new UserSubject()
                {
                    User = users[0],
                    Subject = subjects[2],
                    SubjectRelation = SubjectRelationType.TEACHING
                },
                new UserSubject()
                {
                    User = users[1],
                    Subject = subjects[0],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[1],
                    Subject = subjects[1],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[1],
                    Subject = subjects[2],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[1],
                    Subject = subjects[3],
                    SubjectRelation = SubjectRelationType.TEACHING
                },
                new UserSubject()
                {
                    User = users[1],
                    Subject = subjects[4],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[2],
                    Subject = subjects[0],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[2],
                    Subject = subjects[1],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[2],
                    Subject = subjects[2],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[2],
                    Subject = subjects[3],
                    SubjectRelation = SubjectRelationType.ATTENDING
                },
                new UserSubject()
                {
                    User = users[2],
                    Subject = subjects[4],
                    SubjectRelation = SubjectRelationType.ATTENDING
                }
            };

            foreach (UserSubject userSubject in userSubjects)
            {
                databaseContext.UsersSubjects.Add(userSubject);
            }

            Exam[] exams = new Exam[]
            {
                new Exam()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(3),
                    Subject = subjects[0]
                },
                new Exam()
                {
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddSeconds(-5),
                    Subject = subjects[1]
                },
                new Exam()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(4),
                    Subject = subjects[2]
                },
                new Exam()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(3),
                    Subject = subjects[3]
                }
            };

            foreach (Exam exam in exams)
            {
                databaseContext.Add(exam);
            }

            databaseContext.SaveChanges();
        }
    }
}
