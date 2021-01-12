using System;
using System.Linq;
using ScruMster.Models;

namespace ScruMster.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ScrumContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var teams = new Team[]
            {
            new Team{Name="Juniors",Description="Junior Developers"},
            new Team{Name="Mids",Description="Mid Developers"},
            new Team{Name="Seniors",Description="Senior Developers"},

            };

            foreach (Team t in teams)
            {
                context.Teams.Add(t);
            }
            context.SaveChanges();

            var users = new User[]
            {
            new User{TeamID=1,Name="Carson",Surname="Alexander",Mail="carson.alexander@gmail.com",Position="Intern",Boss=false},
            new User{TeamID=1,Name="Caprice",Surname="Gray",Mail="caprice.gray@gmail.com",Position="Junior Developer",Boss=false},
            new User{TeamID=1,Name="Faye",Surname="Zavala",Mail="faye.zavala@gmail.com",Position="Junior Developer",Boss=false},
            new User{TeamID=2,Name="Dante",Surname="Bautista",Mail="dante.bautista@gmail.com",Position="Mid Developer",Boss=false},
            new User{TeamID=2,Name="Burhan",Surname="Good",Mail="burhan.good@gmail.com",Position="Mid Developer",Boss=false},
            new User{TeamID=3,Name="Shannan",Surname="Simmons",Mail="shannan.simmons@gmail.com",Position="Senior Developer",Boss=false},
            new User{TeamID=3,Name="Gwen",Surname="Whelan",Mail="gwen.whelan@gmail.com",Position="Senior Developer",Boss=false},
            new User{TeamID=3,Name="Ollie",Surname="Pena",Mail="ollie.pena@gmail.com",Position="Manager",Boss=true},
            new User{TeamID=2,Name="Jesus",Surname="Sutherland",Mail="jesus.sutherland@gmail.com",Position="Project Manager",Boss=true},
            new User{TeamID=1,Name="Lacie",Surname="Beasley",Mail="lacie.beasley@gmail.com",Position="Leader",Boss=true},

            
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var sprints = new Sprint[]
            {
            new Sprint{TeamID=1,Name="Task1",Description="Task 1",DeadlineDate=DateTime.Parse("2021-01-20 12:00")},
            new Sprint{TeamID=1,Name="Task2",Description="Task 2",DeadlineDate=DateTime.Parse("2021-01-22 12:30")},
            new Sprint{TeamID=2,Name="Task3",Description="Task 3",DeadlineDate=DateTime.Parse("2021-01-24 13:00")},
            new Sprint{TeamID=2,Name="Task4",Description="Task 4",DeadlineDate=DateTime.Parse("2021-01-26 13:30")},
            new Sprint{TeamID=3,Name="Task5",Description="Task 5",DeadlineDate=DateTime.Parse("2021-01-28 14:00")},

            };
            foreach (Sprint s in sprints)
            {
                context.Sprints.Add(s);
            }
            context.SaveChanges();
        }
    }
}