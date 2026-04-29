using CMS.ContentEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; 
namespace BDO.GWT.Core.Entities;
public interface IContentRetrieverWrapper
{
    Task<IEnumerable<IContentItemFieldsSource>> RetrieveButtonUrlAsync(
    IEnumerable<Guid> guids,
    string[] contentTypes);
}