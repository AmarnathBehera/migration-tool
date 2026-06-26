using BDO.GWT.Common.Core.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BDO.GWT.Domain.Core.Helpers;

public static class QueryHelper
{
    public static string[] GetPropertyNames<TEntity, TProperty>(Expression<Func<TEntity, TProperty?>> expression)
    {
        var propertyNames = expression.Body switch
        {
            // This implementation is still partly incorrect because of how 'IQuery.Select' works
            NewExpression newExpression => newExpression.Arguments.Select(QueryExpressionHelper.GetPropertyName).ToArray(),
            MemberExpression memberExpression => new[] { memberExpression.Member.Name },
            UnaryExpression unaryExpression => new[] { QueryExpressionHelper.GetPropertyName(unaryExpression.Operand) },
            _ => throw new NotSupportedException($"Cannot parse property name from {expression.NodeType}."),
        };

        return propertyNames;
    }

    public static string GetForeignKeyColumnName(MemberInfo property)
    {
        return AttributeHelper.GetAttributeField<ForeignKeyAttribute, string>(property, nameof(ForeignKeyAttribute.Name));
    }
}
