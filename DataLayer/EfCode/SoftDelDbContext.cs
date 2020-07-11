﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfClasses;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SoftDelDbContext : DbContext
    {
        public SoftDelDbContext(DbContextOptions<SoftDelDbContext> options)
            : base(options) { }

        public DbSet<EmployeeSoftCascade> Employees { get; set; }
        public DbSet<EmployeeContract> Contracts { get; set; }

        public DbSet<BookSoftDel> Books { get; set; }

        public DbSet<CompanySoftCascade> Companies { get; set; }
        public DbSet<QuoteSoftCascade> Quotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeSoftCascade>()
                .HasMany(x => x.WorksFromMe)
                .WithOne(x => x.Manager)
                .HasForeignKey(x => x.ManagerEmployeeSoftDelId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<EmployeeSoftCascade>()
                .HasOne(x => x.Contract)
                .WithOne()
                .HasForeignKey<EmployeeContract>(x => x.EmployeeSoftCascadeId)
                .OnDelete(DeleteBehavior.ClientCascade);

            //This automatically configures the two types of soft deletes
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISingleSoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.AddSoftDeleteQueryFilter();
                }
                if (typeof(ICascadeSoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.AddCascadeSoftDeleteQueryFilter();
                }
            }
        }
    }
}
