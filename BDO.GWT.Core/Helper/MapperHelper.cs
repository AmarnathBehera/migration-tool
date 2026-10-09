using AutoMapper;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using BDO.GWT.Core.Exceptions;
using System.Linq;

namespace BDO.GWT.Core.Helpers
{
    public static class MapperHelper
    {
        public static string MapToColumnName<TEntity, TObject>(TypeMap typeMap, string entityPropertyName, string? columnPrefix = null)
        {
            return MapToColumnName(typeof(TEntity), typeof(TObject), typeMap, entityPropertyName, columnPrefix);
        }

        public static string MapToColumnName(Type entityType, Type objectType, TypeMap typeMap, string entityPropertyName, string? columnPrefix = null)
        {
            var entityProperty = entityType.IsInterface
                ? entityType.GetInheritedProperty(entityPropertyName)
                : entityType.GetProperty(entityPropertyName);

            if (entityProperty is null)
            {
                throw new MissingPropertyMapException(
                    $"Property '{entityPropertyName}' does not exist on type '{entityType.Name}'.");}

            var propertyMap = typeMap.PropertyMaps.FirstOrDefault(x => x.SourceMember?.Name == entityProperty.Name);

            if (propertyMap == null)
            {
                throw new MissingPropertyMapException($"Property '{entityProperty.Name}' map is missing on type map for '{entityType.Name}' -> '{objectType.Name}'.");
            }

            return $"{columnPrefix}{propertyMap.DestinationMember.Name}";
        }
    }
}
