using BDO.GWT.Domain.Core.Retrievers;
using BDO.GWT.Domain.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BDO.GWT.Domain.Core.Helpers;

public class WhereConditionParser
{
    private readonly ParameterExpression _entityParameterExpression;

    public WhereConditionParser(ParameterExpression entityParameterExpression)
    {
        _entityParameterExpression = entityParameterExpression;
    }

    public IWhereCondition ParseExpression(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => ParseBinaryExpression(binaryExpression),
            MemberExpression memberExpression => ParseMemberExpression(memberExpression),
            MethodCallExpression methodCallExpression => ParseMethodCallExpression(methodCallExpression),
            UnaryExpression unaryExpression => ParseUnaryExpression(unaryExpression),
            _ => throw new NotSupportedException($"{expression.GetType().FullName} is not supported."),
        };
    }

    private IWhereCondition ParseBinaryExpression(BinaryExpression binaryExpression)
    {
        return binaryExpression.NodeType switch
        {
            ExpressionType.AndAlso => ParseAndAlsoBinaryExpression(binaryExpression),
            ExpressionType.Equal => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.GreaterThan => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.GreaterThanOrEqual => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.LessThan => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.LessThanOrEqual => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.NotEqual => ParseRelationalBinaryExpression(binaryExpression),
            ExpressionType.OrElse => ParseOrElseBinaryExpression(binaryExpression),
            _ => throw new NotSupportedException($"Binary expression node type '{binaryExpression.NodeType}' is not supported."),
        };
    }

    private IWhereCondition ParseAndAlsoBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = ParseExpression(binaryExpression.Left);
        var right = ParseExpression(binaryExpression.Right);

        var queryCondition = new AndWhereCondition { left, right };

        return queryCondition;
    }

    private static IWhereCondition ParseMemberExpression(Expression memberExpression)
    {
        var propertyName = QueryExpressionHelper.GetPropertyName(memberExpression);

        var queryCondition = new RelationalWhereCondition(propertyName, QueryOperator.Equal, true);

        return queryCondition;
    }

    private IWhereCondition ParseMethodCallExpression(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Method.DeclaringType == null)
        {
            throw new InvalidOperationException($"If you see this error, change {nameof(WhereConditionParser)}.{nameof(ParseMethodCallExpression)} implementation!");
        }

        if (methodCallExpression.Method.DeclaringType == typeof(Enumerable))
        {
            if (methodCallExpression.Method.Name == nameof(Enumerable.Any))
            {
                var intersectExpression = (MethodCallExpression)methodCallExpression.Arguments.First();

                var propertyName = QueryExpressionHelper.GetPropertyName(intersectExpression.Arguments[0]);
                var values = QueryExpressionHelper.GetPropertyValue(intersectExpression.Arguments[1]);

                return values switch
                {
                    IEnumerable<string> stringValues => new ContainsAnyWhereCondition<string>(propertyName, typeof(string), stringValues.ToArray()),
                    IEnumerable<int> intValues => new ContainsAnyWhereCondition<int>(propertyName, typeof(int), intValues.ToArray()),
                    IEnumerable<long> longValues => new ContainsAnyWhereCondition<long>(propertyName, typeof(long), longValues.ToArray()),
                    IEnumerable<Guid> guidValues => new ContainsAnyWhereCondition<Guid>(propertyName, typeof(Guid), guidValues.ToArray()),
                    IEnumerable<LanguageTag> languageTagValues => new ContainsAnyWhereCondition<LanguageTag>(propertyName, typeof(LanguageTag), languageTagValues.ToArray()),
                    _ => throw new NotSupportedException($"Method call expression with collection type '{values?.GetType().Name}' is not supported."),
                };
            }
        }

        if (methodCallExpression.Method.Name == "Contains")
        {
            var declaringType = methodCallExpression.Method.DeclaringType;

            if (declaringType == typeof(Enumerable))
            {
                return ParseEnumerableContainsMethodCallExpression(methodCallExpression);
            }

            var genericTypeDefinition = declaringType.IsGenericType ? declaringType.GetGenericTypeDefinition() : null;

            if (genericTypeDefinition == typeof(HashSet<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(List<>))
            {
                return ParseEnumerableContainsMethodCallExpression(methodCallExpression);
            }
        }

        if (methodCallExpression.Method.DeclaringType == typeof(string))
        {
            var propertyName = QueryExpressionHelper.GetPropertyName(methodCallExpression.Object!);
            var value = QueryExpressionHelper.GetPropertyValue(methodCallExpression.Arguments[0]);

            return methodCallExpression.Method.Name switch
            {
                nameof(string.StartsWith) => new RelationalWhereCondition(propertyName, QueryOperator.Like, $"{value}%"),
                nameof(string.EndsWith) => new RelationalWhereCondition(propertyName, QueryOperator.Like, $"%{value}"),
                nameof(string.Contains) => new RelationalWhereCondition(propertyName, QueryOperator.Like, $"%{value}%"),
                _ => throw new NotSupportedException($"Method call expression method name '{methodCallExpression.Method.Name}' is not supported."),
            };
        }

        throw new NotSupportedException($"Method call expression method name '{methodCallExpression.Method.Name}' is not supported.");
    }

    private IWhereCondition ParseEnumerableContainsMethodCallExpression(MethodCallExpression methodCallExpression)
    {
        string propertyName;
        var propertyIsArray = false;
        object values;

        if (methodCallExpression.Arguments.Count > 1)
        {
            propertyName = QueryExpressionHelper.GetPropertyName(methodCallExpression.Arguments[1]);
            values = QueryExpressionHelper.GetPropertyValue(methodCallExpression.Arguments[0]);
        }
        else
        {
            if (methodCallExpression.Object is MemberExpression memberExpression && memberExpression.Expression == _entityParameterExpression)
            {
                propertyName = QueryExpressionHelper.GetPropertyName(memberExpression);
                propertyIsArray = true;
                var valueExpression = methodCallExpression.Arguments[0];
                var array = Array.CreateInstance(valueExpression.Type, 1);
                var value = QueryExpressionHelper.GetPropertyValue(methodCallExpression.Arguments[0]);
                array.SetValue(value, 0);
                values = array;
            }
            else
            {
                propertyName = QueryExpressionHelper.GetPropertyName(methodCallExpression.Arguments[0]);
                values = QueryExpressionHelper.GetPropertyValue(methodCallExpression.Object);
            }
        }

        return values switch
        {
            IEnumerable<string> stringValues => new InWhereCondition<string>(propertyName, propertyIsArray, stringValues.ToArray()),
            IEnumerable<int> intValues => new InWhereCondition<int>(propertyName, propertyIsArray, intValues.ToArray()),
            IEnumerable<long> longValues => new InWhereCondition<long>(propertyName, propertyIsArray, longValues.ToArray()),
            IEnumerable<Guid> guidValues => new InWhereCondition<Guid>(propertyName, propertyIsArray, guidValues.ToArray()),
            IEnumerable<LanguageTag> languageTagValues => new InWhereCondition<LanguageTag>(propertyName, propertyIsArray, languageTagValues.ToArray()),
            _ => throw new NotSupportedException($"Method call expression with collection type '{values?.GetType().Name}' is not supported."),
        };
    }

    private IWhereCondition ParseOrElseBinaryExpression(BinaryExpression binaryExpression)
    {
        var left = ParseExpression(binaryExpression.Left);
        var right = ParseExpression(binaryExpression.Right);

        var queryCondition = new OrWhereCondition { left, right };

        return queryCondition;
    }

    private static RelationalWhereCondition ParseRelationalBinaryExpression(BinaryExpression binaryExpression)
    {
        string? propertyName;

        QueryOperator queryOperator;

        var propertyValue = QueryExpressionHelper.GetPropertyValue(binaryExpression.Right);

        if (binaryExpression.Left is MemberExpression memberExpression && memberExpression.Member.DeclaringType == typeof(LanguageTag) && memberExpression.Member.Name == nameof(LanguageTag.Language) && memberExpression.Expression is not null)
        {
            propertyName = QueryExpressionHelper.GetPropertyName(memberExpression.Expression);

            queryOperator = QueryOperator.Like;

            propertyValue = $"{propertyValue}-%";
        }
        else
        {
            propertyName = QueryExpressionHelper.GetPropertyName(binaryExpression);

            queryOperator = QueryExpressionHelper.GetQueryOperator(binaryExpression);
        }

        var queryCondition = new RelationalWhereCondition(propertyName, queryOperator, propertyValue);

        return queryCondition;
    }

    private static IWhereCondition ParseUnaryExpression(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.Not)
        {
            var propertyName = QueryExpressionHelper.GetPropertyName(unaryExpression.Operand);

            var queryCondition = new NotWhereCondition(propertyName);

            return queryCondition;
        }

        throw new NotSupportedException($"Unary expression node type '{unaryExpression.NodeType}' is not supported.");
    }
}
