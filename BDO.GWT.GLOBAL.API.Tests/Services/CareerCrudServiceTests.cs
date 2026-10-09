using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Extensions;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.API.Tests.Services
{
    [TestFixture]
    public class CareerCrudServiceTests
    {
        private Mock<IContentService> _contentRetrieverMock;
        private Mock<IValidationState> _validationStateMock;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
        private Mock<ICultureService> _cultureServiceMock;
        private Mock<IChannelInfoService> _channelInfoServiceMock;
        private Mock<IWebPageManager> _webPageManagerMock;
        private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock;
        private Mock<IMappingHelper> _mappingHelperMock;
        private Mock<IPageContentValidator> _pageContentValidatorMock;
        private Mock<ICultureValidationService> _cultureValidationServiceMock;
        //private Mock<IUserInfoProvider> _userInfoProviderMock;

        // -- Kentico IoC bootstrap (inlined; runs once per test-process) ---------------------
        private static Mock<CMS.DataEngine.IInfoProvider<ContentLanguageInfo>>? _contentLanguageProvider;
        private static readonly object _kenticoInitLock = new();

        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentService>();
            _validationStateMock = new Mock<IValidationState>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _cultureServiceMock = new Mock<ICultureService>();
            _channelInfoServiceMock = new Mock<IChannelInfoService>();
            _webPageManagerMock = new Mock<IWebPageManager>();
            _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>();
            _mappingHelperMock = new Mock<IMappingHelper>();
            _pageContentValidatorMock = new Mock<IPageContentValidator>();
            _cultureValidationServiceMock = new Mock<ICultureValidationService>();
            //_userInfoProviderMock = new Mock<IUserInfoProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            // No action needed; IoC container is reset per test via OneTimeTearDown
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            lock (_kenticoInitLock)
            {
                _contentLanguageProvider = null;
            }
        }

        private CareerCrudService CreateService()
        {
            // The real constructor resolves a Kentico UserInfo (an AbstractInfoBase), which
            // triggers the CMS IoC container and fails outside a running CMS runtime.
            // Bypass the constructor and inject mocks directly into the private fields.
            var service = (CareerCrudService)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(CareerCrudService));

            SetPrivateField(service, "_contentRetriever", _contentRetrieverMock.Object);
            SetPrivateField(service, "_validationState", _validationStateMock.Object);
            SetPrivateField(service, "_websiteChannelContext", _websiteChannelContextMock.Object);
            SetPrivateField(service, "_cultureService", _cultureServiceMock.Object);            
            SetPrivateField(service, "_channelInfoService1", _channelInfoServiceMock.Object);
            SetPrivateField(service, "_webPageManager", _webPageManagerMock.Object);
            SetPrivateField(service, "_mappingHelper", _mappingHelperMock.Object);
            SetPrivateField(service, "_pageContentValidator", _pageContentValidatorMock.Object);
            SetPrivateField(service, "_cultureValidationService", _cultureValidationServiceMock.Object);
            
            return service;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (field is null)
            {
                Assert.Fail($"Field '{fieldName}' was not found on {target.GetType().Name}. " +
                            "The service's private field names may have changed.");
            }

            field!.SetValue(target, value);
        }

        // Helper to set up the GetPagesAsync<Career> overload that the service actually calls:
        // Task<IEnumerable<TPage>> GetPagesAsync<TPage>(RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken)
        private void SetupGetCareerPages(IEnumerable<Career> pages)
        {
            _contentRetrieverMock
                .Setup(x => x.GetPagesAsync<Career>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);
        }

        private void SetupValidCulture() =>
            _cultureValidationServiceMock
                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(LanguageTag.GB);

        #region CreateCareerPageAsync

        [Test]
        public async Task CreateCareerPageAsync_NullRequest_ReturnsBadRequest()
        {
            var service = CreateService();

            var result = await service.CreateCareerPageAsync(1, 1, "en", null!, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void CreateCareerPageAsync_InvalidCulture_ThrowsNotFoundException()
        {
            _cultureValidationServiceMock
                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LanguageTag?)null);
            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageAsync(1, 1, "xx-invalid", new CareerPageContent(), CancellationToken.None));
        }

        [Test]
        public void CreateCareerPageAsync_ChannelNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageAsync(1, 1, "en", new CareerPageContent(), CancellationToken.None));
        }

        [Test]
        public void CreateCareerPageAsync_ParentPageNotFound_ThrowsUnprocessableEntityException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // Parent lookup uses GetPagesAsync<Careers>; return empty to trigger the not-found path.
            _contentRetrieverMock
                .Setup(x => x.GetPagesAsync<Careers>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Careers>());

            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();

            Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                service.CreateCareerPageAsync(1, 1, "en", new CareerPageContent(), CancellationToken.None));

            _validationStateMock.Verify(x => x.AddUnprocessableEntityError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

       

        [Test]
        public void CreateCareerPageAsync_ValidationErrors_ThrowsFromValidationState()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // ThrowIfThereAreErrors is an extension method and cannot be mocked directly.
            // Mock the underlying instance methods it calls instead.
            _validationStateMock.Setup(x => x.HasErrors()).Returns(false);
            _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

            var service = CreateService();

            Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                service.CreateCareerPageAsync(1, 1, "en", new CareerPageContent(), CancellationToken.None));
        }


        #endregion

        #region CreateCareerPageCultureVersionAsync

        [Test]
        public async Task CreateCareerPageCultureVersionAsync_NullRequest_ReturnsBadRequest()
        {
            var service = CreateService();
            var result = await service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", null!, CancellationToken.None);
            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task CreateCareerPageCultureVersionAsync_ExistingPageNotFound_ReturnsNotFound()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");
            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();
            var result = await service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

       

        #endregion

        #region DeleteCareerPageAsync

        [Test]
        public void DeleteCareerPageAsync_InvalidCulture_ThrowsNotFoundException()
        {
            _cultureValidationServiceMock
                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LanguageTag?)null);
            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteCareerPageAsync(1, Guid.NewGuid(), "invalid", false, CancellationToken.None));
        }

        [Test]
        public void DeleteCareerPageAsync_ChannelNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var service = CreateService();
            Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", false, CancellationToken.None));
        }

        [Test]
        public void DeleteCareerPageAsync_SingleCulture_PageNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");
            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();
            Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", false, CancellationToken.None));
        }

        [Test]
        public async Task DeleteCareerPageAsync_SingleCulture_PageExists_CallsDelete()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            var career = new Career { SystemFields = new WebPageFields { WebPageItemID = 42 } };
            SetupGetCareerPages(new[] { career });

            var service = CreateService();

            await service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", false, CancellationToken.None);

            _webPageManagerMock.Verify(
                x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DeleteCareerPageAsync_AllCultures_DeletesEachExistingVariant()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // Two site cultures -> the loop runs twice.
            _cultureServiceMock
                .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB, LanguageTag.GB });

            var career = new Career { SystemFields = new WebPageFields { WebPageItemID = 7 } };
            SetupGetCareerPages(new[] { career });

            var service = CreateService();

            await service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", true, CancellationToken.None);

            _webPageManagerMock.Verify(
                x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task DeleteCareerPageAsync_AllCultures_NoVariantFound_DoesNotDelete()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            _cultureServiceMock
                .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LanguageTag> { LanguageTag.GB });

            SetupGetCareerPages(Enumerable.Empty<Career>());   // no page for the culture

            var service = CreateService();

            await service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", true, CancellationToken.None);

            _webPageManagerMock.Verify(
                x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region GetCareerPageAsync

        [Test]
        public void GetCareerPageAsync_PageNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");
            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();
            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPageAsync(1, Guid.NewGuid(), "en", CancellationToken.None));
        }

        

        #endregion

        #region GetCareerPagesAsync

        [Test]
        public void GetCareerPagesAsync_NoResults_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");
            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();
            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPagesAsync(1, "en", true, CancellationToken.None));
        }

      

       

        #endregion

        #region UpdateCareerPageAsync

        [Test]
        public async Task UpdateCareerPageAsync_NullRequest_ReturnsBadRequest()
        {
            var service = CreateService();
            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", null!, CancellationToken.None);
            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task UpdateCareerPageAsync_PageNotFound_ReturnsNotFoundResult()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");
            SetupGetCareerPages(Enumerable.Empty<Career>());

            var service = CreateService();
            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateCareerPageAsync_ValidationException_ReturnsUnprocessableEntityObjectResult()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // ThrowIfThereAreErrors is an extension method and cannot be mocked directly.
            // Mock the underlying instance methods it calls instead.
            _validationStateMock.Setup(x => x.HasErrors()).Returns(false);
            _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

            var service = CreateService();
            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var objectResult = result.Result as UnprocessableEntityObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null);
                Assert.That(objectResult!.StatusCode, Is.EqualTo(422));
            }); 
           
        }

        [Test]
        public async Task UpdateCareerPageAsync_NotFoundExceptionThrown_ReturnsNotFoundObjectResult()
        {
            _cultureValidationServiceMock
                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Channel not found"));

            var service = CreateService();
            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var objectResult = result.Result as NotFoundObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null);
                Assert.That(objectResult!.StatusCode, Is.EqualTo(404));
                Assert.That(objectResult.Value, Is.EqualTo("Channel not found"));
            });
   
        }

        [Test]
        public async Task UpdateCareerPageAsync_UnexpectedException_Returns500ObjectResult()
        {
            _cultureValidationServiceMock
                .Setup(x => x.ValidateAndNormalizeSiteCultureAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

            var service = CreateService();
            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var objectResult = result.Result as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null);
                Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            });
        }




        #endregion

        #region Additional coverage

        [Test]
        public void GetCareerPagesAsync_ChannelNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPagesAsync(1, "en", true, CancellationToken.None));
        }

        [Test]
        public void CreateCareerPageAsync_ValidationHasErrors_ThrowsFromValidationState()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // Trigger the HasErrors branch of ThrowIfThereAreErrors (different from HasUnprocessableEntityErrors).
            _validationStateMock.Setup(x => x.HasErrors()).Returns(true);
            _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(false);

            var service = CreateService();

            Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateCareerPageAsync(1, 1, "en", new CareerPageContent(), CancellationToken.None));
        }

        [Test]
        public void GetCareerPageAsync_ChannelNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPageAsync(1, Guid.NewGuid(), "en", CancellationToken.None));
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_ChannelNotFound_ThrowsNotFoundException()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_ValidationErrors_ThrowsFromValidationState()
        {
            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            _validationStateMock.Setup(x => x.HasErrors()).Returns(false);
            _validationStateMock.Setup(x => x.HasUnprocessableEntityErrors()).Returns(true);

            var service = CreateService();

            Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
        }

        [Test]
        public async Task GetCareerPagesAsync_MapsMetadataRelevantContactPerson_WhenPresent()
        {
            // Bootstrap a minimal Kentico IoC container so the static ContentLanguageInfo.Provider.Get()
            // call inside MapCareerToPageOutput resolves. Safe to call more than once; guarded by a flag.
            var contentLanguageProvider = InitializeKenticoContainer();

            var languageQuery = new Mock<CMS.DataEngine.ObjectQuery<ContentLanguageInfo>>();
            languageQuery
                .Setup(q => q.GetEnumerator())
                .Returns(() => ((IEnumerable<ContentLanguageInfo>)Array.Empty<ContentLanguageInfo>()).GetEnumerator());
            contentLanguageProvider.Reset();
            contentLanguageProvider
                .Setup(p => p.Get())
                .Returns(languageQuery.Object);

            SetupValidCulture();
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("TestChannel");

            // Career itself implements both IWebPageFieldsSource and IMetadata, so it's a valid
            // element for MetadataRelevantContactPerson and exercises the (p as IMetadata) cast
            // on line 602 of CareerCrudServices.cs.
            var relevantContact = new Career
            {
                MetadataTitle = "Jane Doe",
                SystemFields = new WebPageFields { WebPageItemID = 200 }
            };

            var career = new Career
            {
                MetadataTitle = "Career title",
                MetadataRelevantContactPerson = new[] { relevantContact },
                SystemFields = new WebPageFields { WebPageItemID = 10 }
            };

            SetupGetCareerPages(new[] { career });

            var service = CreateService();

            var result = await service.GetCareerPagesAsync(1, "en", true, CancellationToken.None);

            Assert.That(result.Value, Is.Not.Null);
            var mapped = result.Value!.Single();


            Assert.Multiple(() =>
            {
                Assert.That(mapped.Content!.MetadataRelevantContactPerson, Is.Not.Null);
                Assert.That(mapped.Content.MetadataRelevantContactPerson, Has.Count.EqualTo(1));
                Assert.That(mapped.Content.MetadataRelevantContactPerson[0], Is.EqualTo("Jane Doe"));
            });
        }

        private static Mock<CMS.DataEngine.IInfoProvider<ContentLanguageInfo>> InitializeKenticoContainer()
        {
            lock (_kenticoInitLock)
            {
                if (_contentLanguageProvider is not null)
                {
                    return _contentLanguageProvider;
                }

                var contentLanguageProvider = new Mock<CMS.DataEngine.IInfoProvider<ContentLanguageInfo>>();

                CMS.Core.Service.Use<CMS.DataEngine.IInfoProvider<ContentLanguageInfo>>(contentLanguageProvider.Object);
                CMS.Core.Service.Use<CMS.Core.ILocalizationService>(new Mock<CMS.Core.ILocalizationService>().Object);
                CMS.Core.Service.Use<CMS.Core.IConversionService>(new Mock<CMS.Core.IConversionService>().Object);
                CMS.Core.Service.Use<CMS.Core.IEventLogService>(new Mock<CMS.Core.IEventLogService>().Object);
#pragma warning disable KXA0023 // Test fixture calls Service.InitializeContainer without inheriting from a managed test base class
                CMS.Core.Service.InitializeContainer();
#pragma warning restore KXA0023 // Test fixture calls Service.InitializeContainer without inheriting from a managed test base class

                _contentLanguageProvider = contentLanguageProvider;
                return contentLanguageProvider;
            }
        }

        #endregion

        #region Additional coverage

        private void SetupChannel(string? name = "TestChannel") =>
            _channelInfoServiceMock
                .Setup(x => x.GetChannelNameAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(name!);

        private void SetupParentPages(IEnumerable<Careers> pages) =>
            _contentRetrieverMock
                .Setup(x => x.GetPagesAsync<Careers>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);

        private void SetupCareerPagesSequence(params IEnumerable<Career>[] results)
        {
            var sequence = _contentRetrieverMock
                .SetupSequence(x => x.GetPagesAsync<Career>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()));

            foreach (var result in results)
            {
                sequence = sequence.ReturnsAsync(result);
            }
        }

        private void SetupValidCreatePrerequisites()
        {
            SetupValidCulture();
            SetupChannel();
            SetupParentPages(new[] { new Careers() });
        }

        private void SetupSuccessfulVariantCreation(int pageId)
        {
            _webPageManagerMock
                .Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(pageId, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private static string InvokePrivateStatic(string methodName, string? argument)
        {
            var method = typeof(CareerCrudService).GetMethod(
                methodName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found.");
            return (string)method!.Invoke(null, new object?[] { argument })!;
        }

        [TestCase("My Career Page", "my-career-page")]
        [TestCase("  Senior  Dev (C#) ", "senior-dev-c")]
        [TestCase("---Hello---World---", "hello-world")]
        [TestCase("ABC123", "abc123")]
        [TestCase("", "")]
        [TestCase("   ", "")]
        public void GenerateUrlSlug_ReturnsExpectedSlug(string input, string expected)
        {
            Assert.That(InvokePrivateStatic("GenerateUrlSlug", input), Is.EqualTo(expected));
        }

        [TestCase("/careers/my-job", "my-job")]
        [TestCase("/careers/my-job/", "my-job")]
        [TestCase("my-job", "my-job")]
        [TestCase("/", "")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void GetLastUrlPathSegment_ReturnsExpectedSegment(string? input, string expected)
        {
            Assert.That(InvokePrivateStatic("GetLastUrlPathSegment", input), Is.EqualTo(expected));
        }

        [Test]
        public void CreateCareerPageAsync_ParentPagesNull_ThrowsUnprocessableEntityException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupParentPages(null!);

            var service = CreateService();

            Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                service.CreateCareerPageAsync(1, 5, "en", new CareerPageContent(), CancellationToken.None));
            _validationStateMock.Verify(
                x => x.AddUnprocessableEntityError("nodeParentID", It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void CreateCareerPageAsync_SlugCollision_ThrowsUnprocessableEntityException()
        {
            SetupValidCreatePrerequisites();
            var sibling = new Career { SystemFields = new WebPageFields { WebPageUrlPath = "/careers/Senior-Developer" } };
            SetupGetCareerPages(new[] { sibling });

            var service = CreateService();

            Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                service.CreateCareerPageAsync(1, 5, "en", new CareerPageContent { MetadataTitle = "Senior Developer" }, CancellationToken.None));
            _validationStateMock.Verify(
                x => x.AddUnprocessableEntityError(nameof(CareerPageContent.MetadataTitle), It.IsAny<string>()),
                Times.Once);
            _webPageManagerMock.Verify(
                x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void CreateCareerPageAsync_UpdateDraftFails_ThrowsNotFoundException()
        {
            SetupValidCreatePrerequisites();
            SetupGetCareerPages(Enumerable.Empty<Career>());
            _webPageManagerMock
                .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(99, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageAsync(1, 5, "en", new CareerPageContent { MetadataTitle = "Job" }, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Failed to update page draft"));
        }

        [Test]
        public void CreateCareerPageAsync_PublishFails_ThrowsNotFoundException()
        {
            SetupValidCreatePrerequisites();
            SetupGetCareerPages(Enumerable.Empty<Career>());
            _webPageManagerMock
                .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(99, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryPublish(99, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageAsync(1, 5, "en", new CareerPageContent { MetadataTitle = "Job" }, CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Failed to publish updated page"));
        }

        [Test]
        public async Task CreateCareerPageAsync_CreatedPageNotRetrieved_ReturnsBadRequest()
        {
            SetupValidCreatePrerequisites();
            SetupGetCareerPages(Enumerable.Empty<Career>());
            _webPageManagerMock
                .Setup(x => x.Create(It.IsAny<CreateWebPageParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(99, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryPublish(99, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();

            var result = await service.CreateCareerPageAsync(1, 5, "en", new CareerPageContent { MetadataTitle = "Job" }, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
            _webPageManagerMock.Verify(
                x => x.Create(It.Is<CreateWebPageParameters>(p => p.ParentWebPageItemID == 5), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_CreateVariantFails_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 3 } } });
            _webPageManagerMock
                .Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Failed to create language variant"));
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_ZeroWebPageItemId_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 0 } } });
            _webPageManagerMock
                .Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Existing page does not have a valid WebPageItemID"));
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_UpdateDraftFails_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 3 } } });
            _webPageManagerMock
                .Setup(x => x.TryCreateLanguageVariant(It.IsAny<CMS.Websites.CreateLanguageVariantParameters>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(3, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Failed to update page draft"));
        }

        [Test]
        public void CreateCareerPageCultureVersionAsync_PublishFails_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 3 } } });
            SetupSuccessfulVariantCreation(3);
            _webPageManagerMock
                .Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None));
            Assert.That(ex!.Message, Is.EqualTo("Failed to publish updated page"));
        }

        [Test]
        public async Task CreateCareerPageCultureVersionAsync_VariantNotRetrieved_ReturnsBadRequest()
        {
            SetupValidCulture();
            SetupChannel();
            SetupCareerPagesSequence(
                new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 3 } } },
                Enumerable.Empty<Career>());
            SetupSuccessfulVariantCreation(3);
            _webPageManagerMock
                .Setup(x => x.TryPublish(3, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();

            var result = await service.CreateCareerPageCultureVersionAsync(1, Guid.NewGuid(), "en", "fr", new CareerPageContent(), CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task UpdateCareerPageAsync_SystemFieldsNull_ReturnsNotFoundObjectResult()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = null! } });

            var service = CreateService();

            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.Value, Is.EqualTo("Page system fields are missing"));
        }

        [Test]
        public async Task UpdateCareerPageAsync_ChannelNotFound_ReturnsNotFoundObjectResult()
        {
            SetupValidCulture();
            SetupChannel(string.Empty);

            var service = CreateService();

            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.Value, Is.EqualTo("Channel not found"));
        }

        [Test]
        public async Task UpdateCareerPageAsync_UpdateDraftFails_ReturnsNotFoundObjectResult()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 8 } } });
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.Value, Is.EqualTo("Failed to update page draft"));
            _webPageManagerMock.Verify(
                x => x.TryCreateDraft(8, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task UpdateCareerPageAsync_PublishFails_ReturnsNotFoundObjectResult()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 8 } } });
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = CreateService();

            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.Value, Is.EqualTo("Failed to publish updated page"));
        }

        [Test]
        public async Task UpdateCareerPageAsync_UpdatedPageNotRetrieved_ReturnsBadRequest()
        {
            SetupValidCulture();
            SetupChannel();
            SetupCareerPagesSequence(
                new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 8 } } },
                Enumerable.Empty<Career>());
            _webPageManagerMock
                .Setup(x => x.TryUpdateDraft(8, It.IsAny<string>(), It.IsAny<UpdateDraftData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _webPageManagerMock
                .Setup(x => x.TryPublish(8, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = CreateService();

            var result = await service.UpdateCareerPageAsync(1, Guid.NewGuid(), "en", new CareerPageContent(), CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void DeleteCareerPageAsync_SingleCulture_NullResult_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(null!);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteCareerPageAsync(1, Guid.NewGuid(), null, false, CancellationToken.None));
        }

        [Test]
        public async Task DeleteCareerPageAsync_SingleCulture_DeletesPermanentlyWithPageId()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(new[] { new Career { SystemFields = new WebPageFields { WebPageItemID = 42 } } });

            var service = CreateService();

            await service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", false, CancellationToken.None);

            _webPageManagerMock.Verify(
                x => x.Delete(It.Is<DeleteWebPageParameters>(p => p.WebPageItemID == 42 && p.Permanently), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DeleteCareerPageAsync_AllCultures_NoCultures_DoesNotQueryOrDelete()
        {
            SetupValidCulture();
            SetupChannel();
            _cultureServiceMock
                .Setup(x => x.GetSiteCulturesLanguageCodeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LanguageTag>());

            var service = CreateService();

            await service.DeleteCareerPageAsync(1, Guid.NewGuid(), "en", true, CancellationToken.None);

            _contentRetrieverMock.Verify(
                x => x.GetPagesAsync<Career>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _webPageManagerMock.Verify(
                x => x.Delete(It.IsAny<DeleteWebPageParameters>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void GetCareerPageAsync_NullResult_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(null!);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPageAsync(1, Guid.NewGuid(), null, CancellationToken.None));
        }

        [Test]
        public void GetCareerPagesAsync_NullResults_ThrowsNotFoundException()
        {
            SetupValidCulture();
            SetupChannel();
            SetupGetCareerPages(null!);

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPagesAsync(1, null, null, CancellationToken.None));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(null, false)]
        public void GetCareerPagesAsync_OnlyPublished_SetsPreviewFlag(bool? onlyPublished, bool expectedPreview)
        {
            SetupValidCulture();
            SetupChannel();
            RetrievePagesParameters? captured = null;
            _contentRetrieverMock
                .Setup(x => x.GetPagesAsync<Career>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                    (p, _, _, _) => captured = p)
                .ReturnsAsync(Enumerable.Empty<Career>());

            var service = CreateService();

            Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetCareerPagesAsync(1, "en", onlyPublished, CancellationToken.None));
            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.IsForPreview, Is.EqualTo(expectedPreview));
                Assert.That(captured.ChannelName, Is.EqualTo("TestChannel"));
                Assert.That(captured.LinkedItemsMaxLevel, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task GetCareerPageAsync_PageFound_MapsOutput()
        {
            var contentLanguageProvider = InitializeKenticoContainer();
            var languageQuery = new Mock<CMS.DataEngine.ObjectQuery<ContentLanguageInfo>>();
            languageQuery
                .Setup(q => q.GetEnumerator())
                .Returns(() => ((IEnumerable<ContentLanguageInfo>)Array.Empty<ContentLanguageInfo>()).GetEnumerator());
            contentLanguageProvider.Reset();
            contentLanguageProvider.Setup(p => p.Get()).Returns(languageQuery.Object);

            SetupValidCulture();
            SetupChannel();
            var pageGuid = Guid.NewGuid();
            var career = new Career
            {
                MetadataTitle = "Engineer",
                MetadataDescription = "Desc",
                HeroLayout = null!,
                CareerReference = "REF-1",
                IncludeInSitemap = true,
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 11,
                    WebPageItemParentID = 2,
                    WebPageItemGUID = pageGuid,
                    WebPageItemName = "engineer",
                    WebPageUrlPath = "/careers/engineer"
                }
            };
            SetupGetCareerPages(new[] { career });

            var service = CreateService();

            var result = await service.GetCareerPageAsync(1, pageGuid, "en", CancellationToken.None);

            var output = result.Value;
            Assert.That(output, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(output!.General!.Name, Is.EqualTo("engineer"));
                Assert.That(output.General.Culture, Is.EqualTo("en-gb"));
                Assert.That(output.General.WebPageItemID, Is.EqualTo(11));
                Assert.That(output.General.WebPageItemParentID, Is.EqualTo(2));
                Assert.That(output.General.PageGuid, Is.EqualTo(pageGuid));
                Assert.That(output.General.Path, Is.EqualTo("/careers/engineer"));
                Assert.That(output.Content!.MetadataTitle, Is.EqualTo("Engineer"));
                Assert.That(output.Content.HeroLayout, Is.EqualTo(string.Empty));
                Assert.That(output.Content.CareerReference, Is.EqualTo("REF-1"));
                Assert.That(output.Content.CareerJobTitle, Is.EqualTo(string.Empty));
                Assert.That(output.Content.CareerContactPeopleID, Is.Empty);
                Assert.That(output.Metadata!.PageTitle!.Value, Is.EqualTo("Engineer"));
                Assert.That(output.Navigation!.ShowInMenu, Is.False);
                Assert.That(output.Navigation.ShowInSitemap, Is.True);
            });
        }

        #endregion
    }

    /// <summary>
    /// Fluent builder for creating <see cref="Career"/> test instances
    /// without going through the Kentico content engine.
    /// </summary>
    internal sealed class CareerBuilder
    {
        private readonly Career _career = new();

        public CareerBuilder WithTitle(string title)
        {
            // TODO: map to the actual Career title property, e.g.:
            // _career.CareerTitle = title;
            return this;
        }

        public CareerBuilder WithUrlSlug(string slug)
        {
            // TODO: set the property the service reads for the slug.
            return this;
        }

        // Add more With* methods for the properties the tests need to assert on.

        public Career Build() => _career;
    }
}
