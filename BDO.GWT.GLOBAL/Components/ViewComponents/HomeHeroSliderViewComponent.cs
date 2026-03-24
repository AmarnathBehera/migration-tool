using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Global.Components.ViewComponents;

public class HomeHeroSliderViewComponent : ViewComponent
{



    public HomeHeroSliderViewComponent()
    {


    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var homeheroSlide = new HomeHeroSlideDto
        {
            Title = "Welcome to Our Platform",
            Description = "This is a sample description used for testing metadata rendering.",

            Button = new LinkItemDto
            {
                Title = "Learn More",
                URL = "/learn-more",
                Target = "_self"
            },

            ImageURL = string.Empty,
            ImageAltText = "Alt Test",

            //MobileImageURL = ,
            MobileImageAltText = "Alt Test",

            ThumbnailImageURL = "~//wwwroot//Hippo.jpeg",
            ThumbnailImageAltText = "Alt Test"


        };

        return View( homeheroSlide);

    }
}
