﻿using System;
using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Tools;
using BirdsiteLive.Twitter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Domain.Tests.Tools
{
    [TestClass]
    public class StatusExtractorTests
    {
        private readonly InstanceSettings _settings;

        #region Ctor
        public StatusExtractorTests()
        {
            _settings = new InstanceSettings
            {
                Domain = "domain.name"
            };
        }
        #endregion

        [TestMethod]
        public void Extract_ReturnLines_Test()
        {
            #region Stubs
            var message = "Bla.\n\n@Mention blo. https://t.co/pgtrJi9600";
            #endregion
            
            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.IsTrue(result.content.Contains("Bla."));
            Assert.IsTrue(result.content.Contains("</p><p>"));
            #endregion
        }

        [TestMethod]
        public void Extract_ReturnSingleLines_Test()
        {
            #region Stubs
            var message = "Bla.\n@Mention blo. https://t.co/pgtrJi9600";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.IsTrue(result.content.Contains("Bla."));
            Assert.IsTrue(result.content.Contains("<br/>"));
            #endregion
        }

        [TestMethod]
        public void Extract_FormatUrl_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}https://t.co/L8BpyHgg25";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(0, result.tags.Length);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://t.co/L8BpyHgg25"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">https://</span><span class=""ellipsis"">t.co/L8BpyHgg25</span><span class=""invisible""></span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_FormatUrl_Long_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}https://www.eff.org/deeplinks/2020/07/pact-act-not-solution-problem-harmful-online-content";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(0, result.tags.Length);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://www.eff.org/deeplinks/2020/07/pact-act-not-solution-problem-harmful-online-content"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">https://www.</span><span class=""ellipsis"">eff.org/deeplinks/2020/07/pact</span><span class=""invisible"">-act-not-solution-problem-harmful-online-content</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_FormatUrl_Exact_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}https://www.eff.org/deeplinks/2020/07/pact";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(0, result.tags.Length);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://www.eff.org/deeplinks/2020/07/pact"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">https://www.</span><span class=""ellipsis"">eff.org/deeplinks/2020/07/pact</span><span class=""invisible""></span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_MultiUrls__Test()
        {
            #region Stubs
            var message = $"https://t.co/L8BpyHgg25 Bla!{Environment.NewLine}https://www.eff.org/deeplinks/2020/07/pact-act-not-solution-problem-harmful-online-content";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(0, result.tags.Length);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://t.co/L8BpyHgg25"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">https://</span><span class=""ellipsis"">t.co/L8BpyHgg25</span><span class=""invisible""></span></a>"));

            Assert.IsTrue(result.content.Contains(@"<a href=""https://www.eff.org/deeplinks/2020/07/pact-act-not-solution-problem-harmful-online-content"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">https://www.</span><span class=""ellipsis"">eff.org/deeplinks/2020/07/pact</span><span class=""invisible"">-act-not-solution-problem-harmful-online-content</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleHashTag_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}#mytag⁠";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("#mytag", result.tags.First().name);
            Assert.AreEqual("Hashtag", result.tags.First().type);
            Assert.AreEqual("https://domain.name/tags/mytag", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag"" class=""mention hashtag"" rel=""tag"">#<span>mytag</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleHashTag_AtStart_Test()
        {
            #region Stubs
            var message = $"#mytag⁠ Bla!";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("#mytag", result.tags.First().name);
            Assert.AreEqual("Hashtag", result.tags.First().type);
            Assert.AreEqual("https://domain.name/tags/mytag", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag"" class=""mention hashtag"" rel=""tag"">#<span>mytag</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleHashTag_SpecialChar_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}#COVIDー19⁠";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("#COVIDー19", result.tags.First().name);
            Assert.AreEqual("Hashtag", result.tags.First().type);
            Assert.AreEqual("https://domain.name/tags/COVIDー19", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/COVIDー19"" class=""mention hashtag"" rel=""tag"">#<span>COVIDー19</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_MultiHashTags_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}#mytag #mytag2 #mytag3⁠{Environment.NewLine}Test #bal Test";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(4, result.tags.Length);
            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag"" class=""mention hashtag"" rel=""tag"">#<span>mytag</span></a>"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag2"" class=""mention hashtag"" rel=""tag"">#<span>mytag2</span></a>"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag3"" class=""mention hashtag"" rel=""tag"">#<span>mytag3</span></a>"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/bal"" class=""mention hashtag"" rel=""tag"">#<span>bal</span></a>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleMentionTag_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}@mynickname⁠";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("@mynickname@domain.name", result.tags.First().name);
            Assert.AreEqual("Mention", result.tags.First().type);
            Assert.AreEqual("https://domain.name/users/mynickname", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleMentionTag_SpecialChar_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}@my___nickname⁠";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("@my___nickname@domain.name", result.tags.First().name);
            Assert.AreEqual("Mention", result.tags.First().type);
            Assert.AreEqual("https://domain.name/users/my___nickname", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@my___nickname"" class=""u-url mention"">@<span>my___nickname</span></a></span>"));
            #endregion
        }

        [TestMethod]
        public void Extract_SingleMentionTag_AtStart_Test()
        {
            #region Stubs
            var message = $"@mynickname Bla!";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.AreEqual("@mynickname@domain.name", result.tags.First().name);
            Assert.AreEqual("Mention", result.tags.First().type);
            Assert.AreEqual("https://domain.name/users/mynickname", result.tags.First().href);

            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span>"));
            #endregion
        }
        
        [TestMethod]
        public void Extract_MultiMentionTag_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}@mynickname⁠ @mynickname2 @mynickname3{Environment.NewLine}Test @dada Test";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(4, result.tags.Length);
            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname2"" class=""u-url mention"">@<span>mynickname2</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname3"" class=""u-url mention"">@<span>mynickname3</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@dada"" class=""u-url mention"">@<span>dada</span></a></span>"));
            #endregion
        }

        [TestMethod]
        public void Extract_HeterogeneousTag_Test()
        {
            #region Stubs
            var message = $"Bla!{Environment.NewLine}@mynickname⁠ #mytag2 @mynickname3{Environment.NewLine}Test @dada #dada Test";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(5, result.tags.Length);
            Assert.IsTrue(result.content.Contains("Bla!"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/mytag2"" class=""mention hashtag"" rel=""tag"">#<span>mytag2</span></a>"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@mynickname3"" class=""u-url mention"">@<span>mynickname3</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<span class=""h-card""><a href=""https://domain.name/@dada"" class=""u-url mention"">@<span>dada</span></a></span>"));
            Assert.IsTrue(result.content.Contains(@"<a href=""https://domain.name/tags/dada"" class=""mention hashtag"" rel=""tag"">#<span>dada</span></a>"));
            #endregion
        }


        [TestMethod]
        public void Extract_Emoji_Test()
        {
            #region Stubs
            var message = $"😤@mynickname 😎😍🤗🤩😘";
            //var message = $"tests@mynickname";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.IsTrue(result.content.Contains(
                @"😤 <span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span>"));

            Assert.IsTrue(result.content.Contains(@"😎 😍 🤗 🤩 😘"));
            #endregion
        }

        [TestMethod]
        public void Extract_Parenthesis_Test()
        {
            #region Stubs
            var message = $"bla (@mynickname test)";
            //var message = $"tests@mynickname";
            #endregion

            var service = new StatusExtractor(_settings);
            var result = service.Extract(message);

            #region Validations
            Assert.AreEqual(1, result.tags.Length);
            Assert.IsTrue(result.content.Equals(@"bla ( <span class=""h-card""><a href=""https://domain.name/@mynickname"" class=""u-url mention"">@<span>mynickname</span></a></span> test)"));
            #endregion
        }
    }
}