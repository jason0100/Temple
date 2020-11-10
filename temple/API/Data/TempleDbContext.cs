using API.Models;
using API.Models.FinancialItem;
using API.Models.FinancialRecord;
using API.Models.Friends;
using API.Models.MemberData;
using API.Models.Notification;
using API.Models.RitualMoney;
using API.Models.ToDoList;
using API.Models.Transfer;
using API.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class TempleDbContext : DbContext
    {
        public TempleDbContext(DbContextOptions<TempleDbContext> options)
            : base(options) { }

        public DbSet<member> members { get; set; }
        public DbSet<RitualMoneyRecord> RitualMoneyRecords { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<FinancialItem> FinancialItems { get; set; }
        public DbSet<FinancialRecord> FinancialRecords { get; set; }
        public DbSet<TransferRecord> TransferRecords { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<ToDoListItem> ToDoLists { get; set; }
        public DbSet<NotifyModel> Notification { get; set; }

    }
}
