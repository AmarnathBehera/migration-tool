using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class BlogPostPageServiceTests
    {
        private Mock<ISidebarService> _sidebarServiceMock = null!;
        private Mock<IExternalPeopleHelper> _externalPeopleHelperMock = null!;
        private BlogPostPageService _service = null!;

        [SetUp]
        public void Setup()
        {
             _sidebarServiceMock = new Mock<ISidebarService>();
            _externalPeopleHelperMock = new Mock<IExternalPeopleHelper>();

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new ContentSidebarDto());

            _externalPeopleHelperMock
                .Setup(x => x.GetSidebarExternalPeopleDtosAsync(
                    It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<SidebarExternalPersonDto>());

            _service = new BlogPostPageService(
                _sidebarServiceMock.Object,
                _externalPeopleHelperMock.Object);
        }

        // ==========================================================================
        // GetBlogPostDtoAsync - Null input
        // ==========================================================================

        #region Null Input

        [Test]
        public async Task GetBlogPostDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            var result = await _service.GetBlogPostDtoAsync(null!, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.Null);
                Assert.That(result.Description, Is.Null);
                Assert.That(result.Content, Is.Null.Or.Empty);
                Assert.That(result.ImageDto, Is.Null);
                Assert.That(result.Sidebar, Is.Null);
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotCallSidebarService_WhenPageIsNull()
        {
            await _service.GetBlogPostDtoAsync(null!, CancellationToken.None);

            _sidebarServiceMock.Verify(
                x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotCallExternalPeopleHelper_WhenPageIsNull()
        {
            await _service.GetBlogPostDtoAsync(null!, CancellationToken.None);

            _externalPeopleHelperMock.Verify(
                x => x.GetSidebarExternalPeopleDtosAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - Image mapping
        // ==========================================================================

        #region Image Mapping

        [Test]
        public async Task GetBlogPostDtoAsync_MapsImageDto_WhenImageFileExists()
        {
            var page = CreateBlogPostPage();
            page.ImageFile = new[]
            {
                new ContentAttachment
                {
                    ContentAttachmentAsset = new ContentItemAsset { Url = "/media/blog-hero.jpg" }
                }
            };
            page.ImageFileAltText = "Blog hero image";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ImageDto, Is.Not.Null);
                Assert.That(result.ImageDto!.Url, Is.EqualTo("/media/blog-hero.jpg"));
                Assert.That(result.ImageDto.AltText, Is.EqualTo("Blog hero image"));
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_MapsEmptyImageUrl_WhenNoImageFile()
        {
            var page = CreateBlogPostPage();
            page.ImageFile = Array.Empty<ContentAttachment>();

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ImageDto, Is.Not.Null);
                Assert.That(result.ImageDto!.Url, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_MapsEmptyAltText_WhenImageFileAltTextIsNull()
        {
            var page = CreateBlogPostPage();
            page.ImageFileAltText = null;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.ImageDto!.AltText, Is.EqualTo(string.Empty));
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - ShowTitle / ShowDescription
        // ==========================================================================

        #region ShowTitle and ShowDescription

        [Test]
        public async Task GetBlogPostDtoAsync_SetsTitleInDto_WhenShowTitleIsTrue()
        {
            var page = CreateBlogPostPage();
            page.ShowTitle = true;
            page.MetadataTitle = "My Blog Post Title";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowTitle, Is.True);
                Assert.That(result.Title, Is.EqualTo("My Blog Post Title"));
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotSetTitle_WhenShowTitleIsFalse()
        {
            var page = CreateBlogPostPage();
            page.ShowTitle = false;
            page.MetadataTitle = "Should Not Appear";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowTitle, Is.False);
                Assert.That(result.Title, Is.Null);
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_SetsDescriptionInDto_WhenShowDescriptionIsTrue()
        {
            var page = CreateBlogPostPage();
            page.ShowDescription = true;
            page.MetadataDescription = "Blog description content";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowDescription, Is.True);
                Assert.That(result.Description, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotSetDescription_WhenShowDescriptionIsFalse()
        {
            var page = CreateBlogPostPage();
            page.ShowDescription = false;
            page.MetadataDescription = "Should Not Appear";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowDescription, Is.False);
                Assert.That(result.Description, Is.Null);
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_TitleIsEmptyString_WhenShowTitleTrueButMetadataTitleIsNull()
        {
            var page = CreateBlogPostPage();
            page.ShowTitle = true;
            page.MetadataTitle = null;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.Title, Is.EqualTo(string.Empty));
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - ShowSidebar
        // ==========================================================================

        #region ShowSidebar

        [Test]
        public async Task GetBlogPostDtoAsync_CallsSidebarService_WhenShowSidebarIsTrue()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(page, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto { ShowPrintButton = true });

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new ContentSidebarDto { ShowPrintButton = true });

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSidebar, Is.True);
                Assert.That(result.Sidebar, Is.Not.Null);
            });

            _sidebarServiceMock.Verify(
                x => x.GetSidebarDtoAsync(page, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotCallSidebarService_WhenShowSidebarIsFalse()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = false;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSidebar, Is.False);
                Assert.That(result.Sidebar, Is.Null);
            });

            _sidebarServiceMock.Verify(
                x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_PassesPublishedDateToSidebarDetails()
        {
            var publishDate = new DateTime(2024, 6, 15);
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.SystemFields = CreateWebPageFields(lastPublished: publishDate);

            DateTime? capturedPublishDate = null;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(
                    (_, _, _, pub, _) => capturedPublishDate = pub)
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(capturedPublishDate, Is.EqualTo(publishDate));
        }

        [Test]
        public async Task GetBlogPostDtoAsync_PassesUpdatedDateToSidebarDetails_WhenNotMinValue()
        {
            var updatedDate = new DateTime(2024, 7, 20);
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.UpdatedOn = updatedDate;

            DateTime? capturedUpdatedDate = null;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(
                    (_, _, _, _, upd) => capturedUpdatedDate = upd)
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(capturedUpdatedDate, Is.EqualTo(updatedDate));
        }

        [Test]
        public async Task GetBlogPostDtoAsync_PassesNullUpdatedDate_WhenUpdatedOnIsMinValue()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.UpdatedOn = DateTime.MinValue;

            DateTime? capturedUpdatedDate = DateTime.Now; // Start with non-null to detect null assignment

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(
                    (_, _, _, _, upd) => capturedUpdatedDate = upd)
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(capturedUpdatedDate, Is.Null);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_SidebarIsNull_WhenSidebarServiceReturnsNull()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ContentSidebarDto)null!);

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.Sidebar, Is.Null);
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - ShowCallToAction
        // ==========================================================================

        #region ShowCallToAction

        [Test]
        public async Task GetBlogPostDtoAsync_SetsShowCallToAction_WhenTrue()
        {
            var page = CreateBlogPostPage();
            page.ShowCallToAction = true;
            page.CallToActionDescription = "Subscribe now";
            page.CallToActionButtonText = "Subscribe";
            page.CallToActionButtonUrl = "https://example.com/subscribe";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.ShowCallToAction, Is.True);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_SetsShowCallToActionFalse_WhenFalse()
        {
            var page = CreateBlogPostPage();
            page.ShowCallToAction = false;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.ShowCallToAction, Is.False);
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - Content mapping
        // ==========================================================================

        #region Content Mapping

        [Test]
        public async Task GetBlogPostDtoAsync_MapsContent()
        {
            var page = CreateBlogPostPage();
            page.Content = "<p>Hello World</p>";

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.Content, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_ContentIsEmptyString_WhenContentIsNull()
        {
            var page = CreateBlogPostPage();
            page.Content = null;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(result.Content, Is.EqualTo(string.Empty));
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - RelevantContactPerson
        // ==========================================================================

        #region RelevantContactPerson

        [Test]
        public async Task GetBlogPostDtoAsync_PassesRelevantPersonGuidsToSidebarDetails()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.RelevantContactPerson = new[]
            {
                CreateWebPageFieldsSourceWithGuid(guid1),
                CreateWebPageFieldsSourceWithGuid(guid2)
            };

            List<Guid>? capturedGuids = null;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(
                    (_, _, guids, _, _) => capturedGuids = guids)
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(capturedGuids, Is.Not.Null);
                Assert.That(capturedGuids!, Has.Count.EqualTo(2));
                Assert.That(capturedGuids!, Does.Contain(guid1));
                Assert.That(capturedGuids!, Does.Contain(guid2));
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_PassesEmptyGuids_WhenRelevantContactPersonIsNull()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.RelevantContactPerson = null;

            List<Guid>? capturedGuids = null;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .Callback<ContentSidebarDto, CancellationToken, List<Guid>, DateTime?, DateTime?>(
                    (_, _, guids, _, _) => capturedGuids = guids)
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.That(capturedGuids, Is.Not.Null);
            Assert.That(capturedGuids!, Is.Empty);
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - RelatedExternalPeople
        // ==========================================================================

        #region RelatedExternalPeople

        [Test]
        public async Task GetBlogPostDtoAsync_CallsExternalPeopleHelper_WhenRelatedExternalPeopleExists()
        {
            var extPersonGuid = Guid.NewGuid();
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.RelatedExternalPeople = new[]
            {
                new ExternalPerson
                {
                    SystemFields = new WebPageFields { ContentItemGUID = extPersonGuid }
                }
            };

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new ContentSidebarDto());

            var expectedPeople = new[]
            {
                new SidebarExternalPersonDto 
                { 
                    Image = null,
                    FullName = "John Doe",
                    JobTitle = null,
                    Url = null
                }
            };

            _externalPeopleHelperMock
                .Setup(x => x.GetSidebarExternalPeopleDtosAsync(
                    It.Is<List<Guid>>(g => g.Contains(extPersonGuid)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPeople);

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Sidebar, Is.Not.Null);
                Assert.That(result.Sidebar!.RelatedExternalPeople, Is.Not.Null);
                Assert.That(result.Sidebar.RelatedExternalPeople, Has.Length.EqualTo(1));
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotCallExternalPeopleHelper_WhenRelatedExternalPeopleIsNull()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = true;
            page.RelatedExternalPeople = null;

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new ContentSidebarDto());

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            _externalPeopleHelperMock.Verify(
                x => x.GetSidebarExternalPeopleDtosAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetBlogPostDtoAsync_DoesNotCallExternalPeopleHelper_WhenSidebarIsFalse()
        {
            var page = CreateBlogPostPage();
            page.ShowSidebar = false;
            page.RelatedExternalPeople = new[]
            {
                new ExternalPerson
                {
                    SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
                }
            };

            await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            _externalPeopleHelperMock.Verify(
                x => x.GetSidebarExternalPeopleDtosAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        // ==========================================================================
        // GetBlogPostDtoAsync - All flags combined
        // ==========================================================================

        #region Combined scenarios

        [Test]
        public async Task GetBlogPostDtoAsync_AllFlagsTrue_ReturnsFullyPopulatedDto()
        {
            var page = CreateBlogPostPage();
            page.ShowTitle = true;
            page.ShowDescription = true;
            page.ShowSidebar = true;
            page.ShowCallToAction = true;
            page.MetadataTitle = "Full Blog Post";
            page.MetadataDescription = "Full description";
            page.Content = "<p>Full content</p>";

            _sidebarServiceMock
                .Setup(x => x.GetSidebarDtoAsync(It.IsAny<BlogPost>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ContentSidebarDto());

            _sidebarServiceMock
                .Setup(x => x.GetContentPageSidebarDetails(
                    It.IsAny<ContentSidebarDto>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new ContentSidebarDto());

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowTitle, Is.True);
                Assert.That(result.ShowDescription, Is.True);
                Assert.That(result.ShowSidebar, Is.True);
                Assert.That(result.ShowCallToAction, Is.True);
                Assert.That(result.Title, Is.EqualTo("Full Blog Post"));
                Assert.That(result.Description, Is.Not.Null);
                Assert.That(result.Content, Is.Not.Null.And.Not.Empty);
                Assert.That(result.Sidebar, Is.Not.Null);
                Assert.That(result.ImageDto, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetBlogPostDtoAsync_AllFlagsFalse_ReturnsMinimalDto()
        {
            var page = CreateBlogPostPage();
            page.ShowTitle = false;
            page.ShowDescription = false;
            page.ShowSidebar = false;
            page.ShowCallToAction = false;

            var result = await _service.GetBlogPostDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowTitle, Is.False);
                Assert.That(result.ShowDescription, Is.False);
                Assert.That(result.ShowSidebar, Is.False);
                Assert.That(result.ShowCallToAction, Is.False);
                Assert.That(result.Title, Is.Null);
                Assert.That(result.Description, Is.Null);
                Assert.That(result.Sidebar, Is.Null);
            });
        }

        #endregion

        // ==========================================================================
        // Helpers
        // ==========================================================================

        private static WebPageFields CreateWebPageFields(
            DateTime? lastPublished = null,
            DateTime? firstPublished = null)
        {
            return new WebPageFields
            {
                ContentItemID = 1,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemName = "test-blog-post",
                ContentItemContentTypeID = 100,
                ContentItemIsSecured = false,
                ContentItemCommonDataContentLanguageID = 1,
                ContentItemCommonDataFirstPublishedWhen = firstPublished ?? new DateTime(2024, 1, 1),
                ContentItemCommonDataLastPublishedWhen = lastPublished ?? new DateTime(2024, 6, 1),
                WebPageItemGUID = Guid.NewGuid(),
                WebPageItemID = 1,
                WebPageItemName = "test-blog-post",
                WebPageItemWebsiteChannelId = 1,
                WebPageUrlPath = "/blogs/test-blog-post",
                WebPageItemOrder = 1,
                WebPageItemTreePath = "/blogs/test-blog-post",
                WebPageUrlPathContentLanguageID = 1,
                WebPageItemParentID = 10
            };
        }

        private static BlogPost CreateBlogPostPage()
        {
            return new BlogPost
            {
                SystemFields = CreateWebPageFields(),
                MetadataTitle = "Test Blog Post",
                MetadataDescription = "Test description",
                ImageFile = Array.Empty<ContentAttachment>(),
                ImageFileAltText = "alt text",
                ContentType = Array.Empty<Contenttypes>(),
                UpdatedOn = DateTime.MinValue,
                MetadataKeywordsBlog = string.Empty,
                MetadataBusinessIssues = Array.Empty<Businessissues>(),
                ShowCallToAction = false,
                CallToActionDescription = string.Empty,
                CallToActionButtonText = string.Empty,
                CallToActionButtonUrl = string.Empty,
                ShowSidebar = false,
                ShowPrintButton = false,
                DownloadButtonAttachment = Array.Empty<ContentAttachment>(),
                ShowSocialMediaButtons = false,
                MetadataOGTitle = string.Empty,
                MetadataOGDescription = string.Empty,
                MetadataOGImage = Array.Empty<ContentAttachment>(),
                MetadataOGTwitterImage = Array.Empty<ContentAttachment>(),
                MetadataTeaserImage = Array.Empty<ContentAttachment>(),
                MetadataTeaserImageAltText = string.Empty,
                MetadataCanonical = string.Empty,
                MetadataRssImage = Array.Empty<IContentItemFieldsSource>(),
                MetadataBusinessLine = Array.Empty<BusinessLine>(),
                MetadataServices = Array.Empty<Service>(),
                MetadataServiceAreas = Array.Empty<ServiceArea>(),
                MetadataIndustryCategories = Array.Empty<IndustryCategory>(),
                MetadataIndustries = Array.Empty<Industry>(),
                MicrositePages = Array.Empty<MicrositePlaceholder>(),
                MicrositeMasterPages = Array.Empty<MicrositePlaceholder>(),
                MetadataRssCategory = Array.Empty<MicrositePlaceholder>(),
                RelevantContactPerson = Array.Empty<IWebPageFieldsSource>(),
                RelatedExternalPeople = Array.Empty<ExternalPerson>(),
                PageTitle = "Test Blog Post",
                PageDescription = string.Empty,
                PageKeywords = string.Empty,
                ShowTitle = true,
                ShowDescription = true,
                Content = "<p>Test content</p>"
            };
        }

        private static IWebPageFieldsSource CreateWebPageFieldsSourceWithGuid(Guid guid)
        {
            var mock = new Mock<IWebPageFieldsSource>();
            mock.Setup(x => x.SystemFields).Returns(new WebPageFields
            {
                ContentItemGUID = guid
            });
            return mock.Object;
        }
    }
}