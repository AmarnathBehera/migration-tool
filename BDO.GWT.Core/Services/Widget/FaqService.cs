using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget;

public interface IFaqService
{
    Task<FaqDto> GetFaqAsync(FaqData data, CancellationToken cancellationToken);
    FaqSchemaMarkupDto GetFaqSchemaOrgAsync(FaqDto faqDto);
}

public class FaqService : IFaqService
{
    private readonly IContentService _contentService;

    public FaqService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<FaqDto> GetFaqAsync(FaqData data, CancellationToken cancellationToken)
    {
        var faqPageDtos = await GetFaqPageDtosAsync(data.PageGuids, cancellationToken);

        return new FaqDto
        {
            Title = data.Title,
            Topics = faqPageDtos,
            Layout = data.Layout,
            HoverColour = data.HoverColour,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };
    }

    public FaqSchemaMarkupDto GetFaqSchemaOrgAsync(FaqDto faqDto)
    {
        var faqQuestions = faqDto.Topics.Select(t => new QuestionSchemaMarkupDto
        {
            Name = t.Title,
            AcceptedAnswer = new AnswerSchemaMarkupDto
            {
                Text = t.Description
            }
        });

        return new FaqSchemaMarkupDto
        {
            MainEntity = faqQuestions.ToList()
        };
    }

    private async Task<FaqPageDto[]> GetFaqPageDtosAsync(
        IEnumerable<Guid> pageGuids,
        CancellationToken cancellationToken)
    {
        var guidList = pageGuids.ToList();

        if (guidList.Count == 0)
        {
            return [];
        }
        var pages = await _contentService.GetPagesAsync<FaqPage>(
             new RetrievePagesParameters { LinkedItemsMaxLevel = 5 },
             query => query.Where(w => w.WhereIn(
                 nameof(FaqPage.SystemFields.ContentItemGUID),  
                 guidList)),
             RetrievalCacheSettings.CacheDisabled,
             cancellationToken);

        var orderedPages = pages
            .OfType<FaqPage>()
            .OrderBy(p => guidList.IndexOf(p.SystemFields.ContentItemGUID))
            .ToList();

        return orderedPages
            .Select(faqPage => new FaqPageDto
            {
                Title = faqPage.Question,        
                Description = faqPage.Answer,   
            })
            .ToArray();
    }
}