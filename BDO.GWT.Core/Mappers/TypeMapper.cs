using BDO.GWT.Core.Mappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services
{
    public interface ITypeMapper
    {
        Type GetTypeFromPageClassName(string className);

        Type GetTypeFromPageClassNameInvariant(string className);

        string GetClassNameFromPageType(Type type);
    }
    public class TypeMapper : ITypeMapper
    {
        private readonly IKenticoTypeMapper _kenticoTypeMapper;

        public TypeMapper(IKenticoTypeMapper kenticoTypeMapper)
        {
            _kenticoTypeMapper = kenticoTypeMapper;
        }

        public Type GetTypeFromPageClassName(string className)
        {
            return _kenticoTypeMapper.GetTypeFromPageClassName(className);
        }

        public Type GetTypeFromPageClassNameInvariant(string className)
        {
            return _kenticoTypeMapper.GetTypeFromPageClassNameInvariant(className);
        }

        public string GetClassNameFromPageType(Type type)
        {
            return _kenticoTypeMapper.GetClassNameFromPageType(type);
        }
    }
}
