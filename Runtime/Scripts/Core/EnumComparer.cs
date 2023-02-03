using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

/// <summary>
/// A fast and efficient implementation of IEqualityComparer for Enum types.
/// Useful for dictionaries that use Enums as their keys.
/// </summary>
/// <typeparam name="TEnum"></typeparam>
public sealed class EnumComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct, IComparable, IConvertible, IFormattable
{
    private static readonly Func<TEnum, TEnum, bool> equals;
    private static readonly Func<TEnum, int> getHashCode;
    /// <summary>
    /// The singleton accessor.
    /// </summary>
    public static readonly EnumComparer<TEnum> Instance;

    /// <summary>
    /// Initializes the class.
    /// </summary>
    static EnumComparer() {
        getHashCode = GenerateGetHashCode();
        equals = GenerateEquals();
        Instance = new EnumComparer<TEnum>();
    }

    /// <summary>
    /// A private constructor to prevent user instantiation.
    /// </summary>
    private EnumComparer() {
        AssertTypeIsEnum();
        AssertUnderlyingTypeIsSupported();
    }

    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type <typeparamref name="TEnum"/> 
    /// to compare.</param>
    /// <param name="y">The second object of type <typeparamref name="TEnum"/> 
    /// to compare.</param>
    /// <returns>
    /// true if the specified objects are equal; otherwise, false.
    /// </returns>
    public bool Equals(TEnum x, TEnum y) {
        return equals(x, y);
    }

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> 
    /// for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// The type of <paramref name="obj"/> is a reference type and 
    /// <paramref name="obj"/> is null.
    /// </exception>
    public int GetHashCode(TEnum obj) {
        return getHashCode(obj);
    }

    private static void AssertTypeIsEnum() {
        if (typeof(TEnum).IsEnum)
            return;
        var message = string.Format("The type parameter {0} is not an Enum. LcgEnumComparer supports Enums only.", typeof(TEnum));
        throw new NotSupportedException(message);
    }
    private static void AssertUnderlyingTypeIsSupported() {
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        ICollection<Type> supportedTypes =
            new[]
                {
                    typeof (byte), typeof (sbyte), typeof (short), typeof (ushort),
                    typeof (int), typeof (uint), typeof (long), typeof (ulong)
                };
        if (supportedTypes.Contains(underlyingType))
            return;
        var message =
          string.Format("The underlying type of the type parameter {0} is {1}. " +
                        "LcgEnumComparer only supports Enums with underlying type of " +
                        "byte, sbyte, short, ushort, int, uint, long, or ulong.",
                        typeof(TEnum), underlyingType);
        throw new NotSupportedException(message);
    }

    private static Func<TEnum, TEnum, bool> GenerateEquals() {
        var xParam = Expression.Parameter(typeof(TEnum), "x");
        var yParam = Expression.Parameter(typeof(TEnum), "y");
        var equalExpression = Expression.Equal(xParam, yParam);
        return Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, new[]
                            { xParam, yParam }).Compile();
    }

    private static Func<TEnum, int> GenerateGetHashCode() {
        var objParam = Expression.Parameter(typeof(TEnum), "obj");
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        var convertExpression = Expression.Convert(objParam, underlyingType);
        var getHashCodeMethod = underlyingType.GetMethod("GetHashCode");
        var getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);
        return Expression.Lambda<Func<TEnum, int>>(getHashCodeExpression, new[]
                            { objParam }).Compile();
    }
}
