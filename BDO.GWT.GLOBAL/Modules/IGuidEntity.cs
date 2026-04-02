using System;

namespace BDO.GWT.Domain.Entities;

public interface IGuidEntity : IEntity
{
    Guid GUID { get; set; }
}