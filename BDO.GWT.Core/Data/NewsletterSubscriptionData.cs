using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Web.Core.Data;

public class NewsletterSubscriptionData
{
    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Company { get; set; }

    public string? JobTitle { get; set; }

    public string? Consent { get; set; }

    public Guid? CountryGuid { get; set; }

    public IEnumerable<Guid> NewsletterGuids { get; set; } = Enumerable.Empty<Guid>();
}
