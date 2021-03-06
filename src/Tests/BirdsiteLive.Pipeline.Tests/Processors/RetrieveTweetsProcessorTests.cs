using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors
{
    [TestClass]
    public class RetrieveTweetsProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_UserNotSync_Test()
        {
            #region Stubs
            var user1 = new SyncTwitterUser
            {
                Id = 1,
                Acct = "acct",
                LastTweetPostedId = -1
            };

            var users = new[]
            {
                user1
            };

            var tweets = new[]
            {
                new ExtractedTweet
                {
                    Id = 47
                }
            };
            #endregion

            #region Mocks
            var twitterServiceMock = new Mock<ITwitterTweetsService>(MockBehavior.Strict);
            twitterServiceMock
                .Setup(x => x.GetTimeline(
                    It.Is<string>(y => y == user1.Acct),
                    It.Is<int>(y => y == 1),
                    It.Is<long>(y => y == -1)
                ))
                .Returns(tweets);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.UpdateTwitterUserAsync(
                    It.Is<int>(y => y == user1.Id),
                    It.Is<long>(y => y == tweets.Last().Id),
                    It.Is<long>(y => y == tweets.Last().Id),
                    It.IsAny<DateTime>()
                ))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new RetrieveTweetsProcessor(twitterServiceMock.Object, twitterUserDalMock.Object);
            var usersResult = await processor.ProcessAsync(users, CancellationToken.None);

            #region Validations
            twitterServiceMock.VerifyAll();
            twitterUserDalMock.VerifyAll();

            Assert.AreEqual(0, usersResult.Length);
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_UserSync_Test()
        {
            #region Stubs
            var user1 = new SyncTwitterUser
            {
                Id = 1,
                Acct = "acct",
                LastTweetPostedId = 46,
                LastTweetSynchronizedForAllFollowersId = 46
            };

            var users = new[]
            {
                user1
            };

            var tweets = new[]
            {
                new ExtractedTweet
                {
                    Id = 47
                },
                new ExtractedTweet
                {
                    Id = 48
                },
                new ExtractedTweet
                {
                    Id = 49
                }
            };
            #endregion

            #region Mocks
            var twitterServiceMock = new Mock<ITwitterTweetsService>(MockBehavior.Strict);
            twitterServiceMock
                .Setup(x => x.GetTimeline(
                    It.Is<string>(y => y == user1.Acct),
                    It.Is<int>(y => y == 200),
                    It.Is<long>(y => y == user1.LastTweetSynchronizedForAllFollowersId)
                ))
                .Returns(tweets);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            #endregion

            var processor = new RetrieveTweetsProcessor(twitterServiceMock.Object, twitterUserDalMock.Object);
            var usersResult = await processor.ProcessAsync(users, CancellationToken.None);

            #region Validations
            twitterServiceMock.VerifyAll();
            twitterUserDalMock.VerifyAll();

            Assert.AreEqual(users.Length, usersResult.Length);
            Assert.AreEqual(users[0].Acct, usersResult[0].User.Acct);
            Assert.AreEqual(tweets.Length, usersResult[0].Tweets.Length);
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_UserPartiallySync_Test()
        {
            #region Stubs
            var user1 = new SyncTwitterUser
            {
                Id = 1,
                Acct = "acct",
                LastTweetPostedId = 49,
                LastTweetSynchronizedForAllFollowersId = 46
            };

            var users = new[]
            {
                user1
            };

            var tweets = new[]
            {
                new ExtractedTweet
                {
                    Id = 47
                },
                new ExtractedTweet
                {
                    Id = 48
                },
                new ExtractedTweet
                {
                    Id = 49
                }
            };
            #endregion

            #region Mocks
            var twitterServiceMock = new Mock<ITwitterTweetsService>(MockBehavior.Strict);
            twitterServiceMock
                .Setup(x => x.GetTimeline(
                    It.Is<string>(y => y == user1.Acct),
                    It.Is<int>(y => y == 200),
                    It.Is<long>(y => y == user1.LastTweetSynchronizedForAllFollowersId)
                ))
                .Returns(tweets);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            #endregion

            var processor = new RetrieveTweetsProcessor(twitterServiceMock.Object, twitterUserDalMock.Object);
            var usersResult = await processor.ProcessAsync(users, CancellationToken.None);

            #region Validations
            twitterServiceMock.VerifyAll();
            twitterUserDalMock.VerifyAll();

            Assert.AreEqual(users.Length, usersResult.Length);
            Assert.AreEqual(users[0].Acct, usersResult[0].User.Acct);
            Assert.AreEqual(tweets.Length, usersResult[0].Tweets.Length);
            #endregion
        }
    }
}