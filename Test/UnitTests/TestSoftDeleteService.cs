﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using SoftDeleteServices.Concrete;
using SoftDeleteServices.ExampleConfigs;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSoftDeleteService
    {
        [Fact]
        public void TestAddBookWithReviewOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                AddBookWithReviewToDb(context);
            }
            using (var context = new SoftDelDbContext(options))
            {
                //VERIFY
                var book = context.Books.Include(x => x.Reviews).Single();
                book.Title.ShouldEqual("test");
                book.Reviews.ShouldNotBeNull();
                book.Reviews.Single().NumStars.ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDelete(book);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(0);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<BookSoftDel>(book.BookSoftDelId);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(0);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysNotFoundBad()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<BookSoftDel>(123);

                //VERIFY
                status.IsValid.ShouldBeFalse();
                status.GetAllErrors().ShouldEqual("Could not find the entry you ask for.");
                status.Result.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceSetSoftDeleteViaKeysNotFoundReturnsZero()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty) {NotFoundIsNotAnError = true};
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var status = service.SetSoftDeleteViaKeys<BookSoftDel>(123);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book = AddBookWithReviewToDb(context);

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                service.SetSoftDelete(book);

                //ATTEMPT
                var status = service.ResetSoftDelete(book);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                status.Result.ShouldEqual(1);
            }
            using (var context = new SoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceResetSoftDeleteViaKeysOk()
        {
            //SETUP
            int bookId;
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                bookId = AddBookWithReviewToDb(context).BookSoftDelId;

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                var status1 = service.SetSoftDeleteViaKeys<BookSoftDel>(bookId);
                status1.IsValid.ShouldBeTrue(status1.GetAllErrors());

                //ATTEMPT
                var status2 = service.ResetSoftDeleteViaKeys<BookSoftDel>(bookId);

                //VERIFY
                status2.IsValid.ShouldBeTrue(status2.GetAllErrors());
                status2.Result.ShouldEqual(1);
            }
            using (var context = new SoftDelDbContext(options))
            {
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestSoftDeleteServiceGetSoftDeletedEntriesOk()
        {
            //SETUP
            int bookId;
            var options = SqliteInMemory.CreateOptions<SoftDelDbContext>();
            using (var context = new SoftDelDbContext(options))
            {
                context.Database.EnsureCreated();
                var book1 = AddBookWithReviewToDb(context, "test1");
                var book2 = AddBookWithReviewToDb(context, "test2");

                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);
                var status = service.SetSoftDelete(book1);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());

            }
            using (var context = new SoftDelDbContext(options))
            {
                var config = new ConfigISoftDeleteWithUserId(Guid.Empty);
                var service = new SingleSoftDeleteService<ISingleSoftDelete>(context, config);

                //ATTEMPT
                var softDelBooks = service.GetSoftDeletedEntries<BookSoftDel>().ToList();

                //VERIFY
                softDelBooks.Count.ShouldEqual(1);
                softDelBooks.Single().Title.ShouldEqual("test1");
                context.Books.Count().ShouldEqual(1);
                context.Books.IgnoreQueryFilters().Count().ShouldEqual(2);
            }
        }

        private static BookSoftDel AddBookWithReviewToDb(SoftDelDbContext context, string title = "test")
        {
            var book = new BookSoftDel
                {Title = title, Reviews = new List<ReviewSoftDel> {new ReviewSoftDel {NumStars = 1}}};
            context.Add(book);
            context.SaveChanges();
            return book;
        }
    }
}