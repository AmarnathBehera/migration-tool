using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;


namespace BDO.GWT.GLOBAL.Tests.Utilities;

public class AssertUtility
{
    public static void IsInstanceOf<TType>(object? actual, out TType castedActual)
    {
        Assert.That(actual, Is.InstanceOf<TType>());

        castedActual = (TType)actual!;
    }

    public static void IsNotNull<TActual>([NotNull] TActual? actual)
    {
        Assert.That(actual, Is.Not.Null);

        if (actual is null)
        {
            throw new AssertionException("Value is null");
        }
    }

    public static void IsTypeOf<TType>(object? actual, out TType castedActual)
    {
        Assert.That(actual, Is.TypeOf<TType>());

        castedActual = (TType)actual!;
    }
}
