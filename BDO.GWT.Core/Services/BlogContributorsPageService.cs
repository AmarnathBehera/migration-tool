using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using HotChocolate.Data.Filters;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.CodeDom;
using System.Linq.Expressions;
namespace BDO.GWT.Core.Services
{
    public interface IBlogContributorsPageService
    {
        Task<BlogContributorDto> GetBlogContributorsPageDtoAsync(BlogContributors page, CancellationToken cancellationToken);

        Task<PaginationDto<PersonCardDto>> GeBlogContributorsPaginationDtoAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
    }
    public class BlogContributorsPageService : IBlogContributorsPageService
    {
        #region Filtering constants

        private const string OrderFilterName = "Order";
        private const string LastNameAtoZOptionValue = "asc";
        private const string LastNameZtoAOptionValue = "desc";
        private const string NodeOrderOptionValue = "nodeorder";

        #endregion
        private readonly ICardsService _cardsService;
        private readonly IContentService _contentService;
        private readonly IHeroService _heroService;

        public BlogContributorsPageService(ICardsService cardsService, IContentService contentService, IHeroService heroService)
        {
            _cardsService = cardsService;
            _contentService = contentService;
            _heroService = heroService;
        }

        public Task<PaginationDto<PersonCardDto>> GeBlogContributorsPaginationDtoAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<BlogContributorDto> GetBlogContributorsPageDtoAsync(BlogContributors currentPage, CancellationToken cancellationToken)
        {
            var options = new[]
              {
                FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameAtoZ, LastNameAtoZOptionValue),
                FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortLastNameZtoA, LastNameZtoAOptionValue),
                FilterDtoUtility.CreateOptionDto(ResourceStringKey.LiveSite.People.SortNodeOrder, NodeOrderOptionValue),
              };

            var bgImageUrl = currentPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var mobileImageUrl = currentPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var ctaUrlPath = currentPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

            var heroData = new HeroData
            {
                Layout = currentPage.HeroLayout,
                Title = currentPage.HeroTitle,
                Description = currentPage.HeroDescription,
                BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
                BackgroundImageAltText = currentPage.HeroBackgroundImageAltText,
                MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
                MobileBackgroundImageAltText = currentPage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = currentPage.HeroCallToActionButtonType,
                CallToActionButtonText = currentPage.HeroCallToActionButtonText,
                CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
            };

            // Construct the API endpoint URL using the page's GUID which is passed as url parameter to the BlogPostsAPIController to filter blog posts related to the current page
            var fullApiEndpoint = $"BlogContributors/{currentPage.SystemFields.WebPageItemID}/Get";
            
            var dto = new BlogContributorDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                SortBy = FilterDtoUtility.CreateFilterItemDto("select",
                    OrderFilterName,
                    OrderFilterName,
                    currentPage.PeopleListDefaultOrder,
                    options),
                PageSize = 20,
                ApiEndpoint = fullApiEndpoint,
            };

            return dto;
        }
    }
}
