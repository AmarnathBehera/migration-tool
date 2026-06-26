using BDO.GWT.Domain.Core.Retrievers;
using System;
using System.Linq.Expressions;

namespace BDO.GWT.Domain.Core;

public static class QueryExpressionHelper
{
    public static string GetPropertyName(Expression expression)
    {
        var propertyName = expression switch
        {
            BinaryExpression binaryExpression => GetPropertyName(binaryExpression.Left),
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression => GetPropertyName(unaryExpression.Operand),
            _ => throw new NotSupportedException($"Getting property name from {expression.GetType().Name} is not supported"),
        };

        return propertyName;
    }

    public static QueryOperator GetQueryOperator(BinaryExpression binaryExpression)
    {
        var queryOperator = binaryExpression.NodeType switch
        {
            ExpressionType.Equal => QueryOperator.Equal,
            ExpressionType.GreaterThan => QueryOperator.GreaterThan,
            ExpressionType.GreaterThanOrEqual => QueryOperator.GreaterThanOrEqual,
            ExpressionType.LessThan => QueryOperator.LessThan,
            ExpressionType.LessThanOrEqual => QueryOperator.LessThanOrEqual,
            ExpressionType.NotEqual => QueryOperator.NotEqual,
            _ => throw new NotImplementedException($"Please add a map \"{binaryExpression.NodeType}\" -> QueryOperator."),
        };

        return queryOperator;
    }

    public static object? GetPropertyValue(Expression expression)
    {
        return Expression.Lambda(expression).Compile().DynamicInvoke();
    }
}
