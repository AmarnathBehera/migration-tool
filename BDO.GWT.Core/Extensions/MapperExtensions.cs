using AutoMapper;
using AutoMapper.Internal;
using BDO.GWT.Core.Exceptions;
using System;

namespace BDO.GWT.Core.Extensions
{
    public static class MapperExtensions
    {
        public static TypeMap GetTypeMap<TEntity, TObject>(this IMapper mapper)
        {
            return mapper.GetTypeMap(typeof(TEntity), typeof(TObject));
        }

        public static TypeMap GetTypeMap(this IMapper mapper, Type entityType, Type objectType)
        {
            var globalConfiguration = mapper.ConfigurationProvider.Internal();

            var typeMap = globalConfiguration.FindTypeMapFor(entityType, objectType);

            if (typeMap == null)
            {
                throw new MissingTypeMapException($"Type map for '{entityType.Name}' -> '{objectType.Name}' is missing.");
            }

            return typeMap;
        }
    }
}
