﻿using System;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class TwitterUserPostgresDalTests : PostgresTestingBase
    {
        [TestInitialize]
        public async Task TestInit()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            var init = new DatabaseInitializer(dal);
            await init.InitAndMigrateDbAsync();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            await dal.DeleteAllAsync();
        }

        [TestMethod]
        public async Task GetTwitterUserAsync_NoUser()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            var result = await dal.GetTwitterUserAsync("dontexist");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(lastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(lastTweetId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.IsTrue(result.Id > 0);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);


            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            await dal.UpdateTwitterUserAsync(result.Id, updatedLastTweetId, updatedLastSyncId, now);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
        }

        [TestMethod]
        public async Task CreateAndDeleteUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNotNull(result);

            await dal.DeleteTwitterUserAsync(acct);
            result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllTwitterUsers()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 1000; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            var result = await dal.GetAllTwitterUsersAsync(1000);
            Assert.AreEqual(1000, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);
        }

        [TestMethod]
        public async Task GetAllTwitterUsers_Limited()
        {
            var now = DateTime.Now;
            var oldest = now.AddDays(-3);
            var newest = now.AddDays(-2);

            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 20; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            var allUsers = await dal.GetAllTwitterUsersAsync(100);
            for (var i = 0; i < 20; i++)
            {
                var user = allUsers[i];
                var date = i % 2 == 0 ? oldest : newest;
                await dal.UpdateTwitterUserAsync(user.Id, user.LastTweetPostedId, user.LastTweetSynchronizedForAllFollowersId, date);
            }

            var result = await dal.GetAllTwitterUsersAsync(10);
            Assert.AreEqual(10, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);

            foreach (var acc in result)
                Assert.IsTrue(Math.Abs((acc.LastSync - oldest.ToUniversalTime()).TotalMilliseconds) < 1000);
        }

        [TestMethod]
        public async Task CountTwitterUsers()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 10; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            var result = await dal.GetTwitterUsersCountAsync();
            Assert.AreEqual(10, result);
        }
    }
}