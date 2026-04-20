using BDO;

using BDO.GWT.Core;

using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;

using BDO.GWT.Web.Core.DTOs.Pages;

using BDO.GWT.Web.Core.LiveSite.Controllers;

using BDO.GWT.Web.Core.Services;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;

using System;

using System.Threading;

using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers

{

    public class ContactControllerTests

    {

        private Mock<IContactPageService> _contactPageServiceMock = null!;

        private ContactController _controller = null!;

        [SetUp]

        public void Setup()

        {

            _contactPageServiceMock = new Mock<IContactPageService>();

            _controller = new ContactController(_contactPageServiceMock.Object);

        }

        [TearDown]

        public void TearDown()

        {

            _controller.Dispose();

        }

        // ?? View result type ?????????????????????????????????????????????????

        [Test]

        public async Task Index_AlwaysReturnsViewResult()

        {

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(new FormPageDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<ViewResult>());

        }

        [Test]

        public async Task Index_ReturnsCorrectViewName()

        {

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(new FormPageDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);

            Assert.That(result!.ViewName, Is.EqualTo("~/Views/FormPage/Index.cshtml"));

        }

        // ?? Model mapping ????????????????????????????????????????????????????

        [Test]

        public async Task Index_ReturnsViewResult_WithFullFormPageDtoMapping()

        {

            var expectedDto = new FormPageDto

            {

                Title = "Contact Us",

                Description = "Get in touch with us.",

                MetaData = new MetaDataDto

                {

                    MetaTitle = "Contact Meta Title",

                    MetaDescription = "Contact Meta Description",

                    MetaTeaserImageAltText = "Alt Text",

                    MetaCanonical = "https://example.com/contact"

                },

                SearchAndSEO = new SearchAndSEODto

                {

                    IncludeInSitemap = true,

                    MetadataNofollow = false,

                    MetadataNoindex = false

                },

                OGMetaData = new OGMetaDataDto

                {

                    MetadataOGTitle = "OG Contact Title",

                    MetadataOGDescription = "OG Contact Description",

                    MetadataOGTwitterImageSizeCheckbox = true

                },

                BaseContent = new BaseContent

                {

                    ContentItemID = 10,

                    ContentItemGUID = Guid.NewGuid(),

                    ContentItemName = "Contact Page",

                    ContentItemContentTypeID = 201,

                    ContentItemIsSecured = false,

                    ContentItemCommonDataContentLanguageID = 1,

                    ContentItemCommonDataFirstPublishedWhen = DateTime.UtcNow.AddDays(-5),

                    ContentItemCommonDataLastPublishedWhen = DateTime.UtcNow

                },

                BaseWebPage = new BaseWebPage

                {

                    WebPageItemGUID = Guid.NewGuid(),

                    WebPageItemID = 200,

                    WebPageItemName = "Contact",

                    WebPageItemWebsiteChannelID = 1,

                    WebPageUrlPath = "/contact",

                    WebPageItemOrder = 2,

                    WebPageItemTreePath = "/contact",

                    WebPageUrlPathContentLanguageID = 1,

                    WebPageItemParentID = 0

                }

            };

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;

            Assert.Multiple(() =>

            {

                Assert.That(viewResult, Is.Not.Null);

                var model = viewResult?.Model as FormPageDto;

                Assert.That(model, Is.Not.Null);

                Assert.That(model?.Title, Is.EqualTo(expectedDto.Title));

                Assert.That(model?.Description, Is.EqualTo(expectedDto.Description));

                Assert.That(model?.MetaData?.MetaTitle, Is.EqualTo(expectedDto.MetaData!.MetaTitle));

                Assert.That(model?.MetaData?.MetaDescription, Is.EqualTo(expectedDto.MetaData.MetaDescription));

                Assert.That(model?.SearchAndSEO?.IncludeInSitemap, Is.EqualTo(expectedDto.SearchAndSEO!.IncludeInSitemap));

                Assert.That(model?.OGMetaData?.MetadataOGTitle, Is.EqualTo(expectedDto.OGMetaData!.MetadataOGTitle));

                Assert.That(model?.BaseContent?.ContentItemName, Is.EqualTo(expectedDto.BaseContent!.ContentItemName));

                Assert.That(model?.BaseWebPage?.WebPageItemName, Is.EqualTo(expectedDto.BaseWebPage!.WebPageItemName));

            });

        }

        [Test]

        public async Task Index_ReturnsViewResult_WithEmptyFormPageDto()

        {

            var emptyDto = new FormPageDto();

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(emptyDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);

            Assert.That(result!.Model, Is.EqualTo(emptyDto));

        }

        // ?? Service interaction ??????????????????????????????????????????????

        [Test]

        public async Task Index_CallsService_ExactlyOnce()

        {

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(new FormPageDto());

            await _controller.Index(CancellationToken.None);

            _contactPageServiceMock.Verify(

                s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()),

                Times.Once

            );

        }

        [Test]

        public async Task Index_PassesNullContactPage_ToService()

        {

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ReturnsAsync(new FormPageDto());

            await _controller.Index(CancellationToken.None);

            _contactPageServiceMock.Verify(

                s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()),

                Times.Once

            );

        }

        // ?? CancellationToken propagation ????????????????????????????????????

        [Test]

        public async Task Index_PassesCancellationToken_ToService()

        {

            var cts = new CancellationTokenSource();

            var token = cts.Token;

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, token))

                .ReturnsAsync(new FormPageDto());

            await _controller.Index(token);

            _contactPageServiceMock.Verify(

                s => s.GetContactPageDtoAsync(null, token),

                Times.Once

            );

        }

        // ?? Exception propagation ????????????????????????????????????????????

        [Test]

        public void Index_WhenServiceThrows_ExceptionIsPropagated()

        {

            _contactPageServiceMock

                .Setup(s => s.GetContactPageDtoAsync(null, It.IsAny<CancellationToken>()))

                .ThrowsAsync(new Exception("Service error"));

            Assert.ThrowsAsync<Exception>(async () =>

                await _controller.Index(CancellationToken.None));

        }

    }

}

