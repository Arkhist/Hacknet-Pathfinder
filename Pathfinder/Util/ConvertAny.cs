using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Pathfinder.Util
{
    /// <summary>
    /// Extended conversion utilities to convert any type to any other type provided the conversion is valid.
    /// </summary>
    public static class ConvertAny
    {

        #region Fields

        private static MethodInfo _genericConvertMethod;
        private static MethodInfo _genericDefaultMethod;

        #endregion

        #region Construction / Deconstruction

        /// <summary>
        /// Initializes static members of the <see cref="ConvertAny"/> class.
        /// </summary>
        static ConvertAny()
        {
            _genericConvertMethod = typeof(ConvertAny).GetMethod(
                "ConvertInternal", BindingFlags.Static | BindingFlags.NonPublic);

            _genericDefaultMethod = typeof(ConvertAny).GetMethod(
                "Default", BindingFlags.Static | BindingFlags.Public);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether this instance can convert the specified value.
        /// </summary>
        /// <typeparam name="ToType">The type of to type.</typeparam>
        /// <param name="value">From value.</param>
        /// <returns><c>true</c> if this instance can convert the specified from value; otherwise, <c>false</c>.</returns>
        [DebuggerStepThrough]
        public static bool CanConvert<ToType>(object value)
        {
            if (value.GetType() == typeof(ToType))
                return true;

            if (typeof(IConvertible).IsAssignableFrom(typeof(ToType)) &&
                typeof(IConvertible).IsAssignableFrom(value.GetType()))
            {
                return true;
            }

            try
            {
                //Casting a boxed object to a type even though the type supports an explicit cast to
                //that type will fail. The only way to do this is to try to find the explicit or
                //implicit type conversion operator on the to type that supports the from type.
                MethodInfo mi = typeof(ToType).GetMethods().FirstOrDefault(m =>
                    (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                    m.ReturnType == typeof(ToType) &&
                    m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == value.GetType()
                    );

                if (mi == null)
                {
                    //We can search for the reverse one on the original type too...
                    mi = value.GetType().GetMethods().FirstOrDefault(m =>
                        (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                        m.ReturnType == typeof(ToType) &&
                        m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == value.GetType()
                        );
                }

                if (mi != null)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified <paramref name="fromType"/> to the <paramref name="toType"/>.
        /// </summary>
        /// <param name="fromType">From type.</param>
        /// <param name="toType">To type.</param>
        /// <returns><c>true</c> if the conversion is valid; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This method checks 4 types of conversions:
        /// 
        /// 1. If they are the same type, returns true.
        /// 2. If they are both IConvertible types, return true.
        /// 3. If the toType is part of the inheritance chain of fromType.
        /// 4. Direct casting, only returns true if both types have default constructors.
        /// </remarks>
        [DebuggerStepThrough]
        public static bool CanConvert(Type fromType, Type toType)
        {
            //Same type
            if (fromType == toType)
                return true;

            //IConvertible types
            if (typeof(IConvertible).IsAssignableFrom(fromType) &&
                typeof(IConvertible).IsAssignableFrom(toType))
                return true;

            //Inheritance chain
            if (toType.IsAssignableFrom(fromType))
                return true;

            try
            {
                //Casting a boxed object to a type even though the type supports an explicit cast to
                //that type will fail. The only way to do this is to try to find the explicit or
                //implicit type conversion operator on the to type that supports the from type.
                MethodInfo mi = toType.GetMethods().FirstOrDefault(m =>
                    (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                    m.ReturnType == toType &&
                    m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == fromType
                    );

                if (mi == null)
                {
                    //We can search for the reverse one on the original type too...
                    mi = fromType.GetMethods().FirstOrDefault(m =>
                        (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                        m.ReturnType == toType &&
                        m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == fromType
                        );
                }

                if (mi != null)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert the value to the specified type. 
        /// </summary>
        /// <typeparam name="ToType">The type to convert to.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <param name="toValue">Converted value.</param>
        /// <returns><c>true</c> if conversion succeeds, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// If the conversion fails, the value of <paramref name="toValue"/> is the default
        /// of that particular type.
        /// </remarks>
        [DebuggerStepThrough]
        public static bool TryConvert<ToType>(object value, out ToType toValue)
        {
            toValue = default(ToType);

            if (value.GetType() == typeof(ToType))
            {
                toValue = (ToType)value;
                return true;
            }

            if (typeof(IConvertible).IsAssignableFrom(typeof(ToType)) &&
                typeof(IConvertible).IsAssignableFrom(value.GetType()))
            {
                toValue = (ToType)System.Convert.ChangeType(value, typeof(ToType));
                return true;
            }

            try
            {
                toValue = ConvertInternal<ToType>(value);

                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the specified from value.
        /// </summary>
        /// <typeparam name="ToType">The type to convert to.</typeparam>
        /// <param name="fromValue">The value to convert.</param>
        /// <returns>Converted value.</returns>
        /// <exception cref="InvalidCastException">Raised when the value cannot be converted to the specified type.</exception>
        [DebuggerStepThrough]
        public static ToType Convert<ToType>(object fromValue)
        {
            if (fromValue.GetType() == typeof(ToType))
                return (ToType)fromValue;

            if (typeof(IConvertible).IsAssignableFrom(typeof(ToType)) &&
                typeof(IConvertible).IsAssignableFrom(fromValue.GetType()))
            {
                return (ToType)System.Convert.ChangeType(fromValue, typeof(ToType));
            }

            return ConvertInternal<ToType>(fromValue);
        }

        /// <summary>
        /// Converts value to the specified type.
        /// </summary>
        /// <param name="fromValue">The value to convert.</param>
        /// <param name="toType">The type to convert to.</param>
        /// <returns>Object that represents the converted type.</returns>
        [DebuggerStepThrough]
        public static object ConvertByType(object fromValue, Type toType)
        {
            if (fromValue.GetType() == toType)
                return fromValue;

            if (typeof(IConvertible).IsAssignableFrom(toType) &&
                typeof(IConvertible).IsAssignableFrom(fromValue.GetType()))
            {
                object dVal = System.Convert.ChangeType(fromValue, toType);
                return dVal;
            }

            return ConvertByGeneric(fromValue, toType);
        }

        /// <summary>
        /// Creates a default instance of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type to create a default instance of.</typeparam>
        /// <returns>Default instance of <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// For primitive types this will create a value of 0 with the specified type. For
        /// objects, if the object has a default constructor it will return a default instance
        /// of the type. If the object does not have a default constructor, it will return
        /// null.
        /// </remarks>
        [DebuggerStepThrough]
        public static T Default<T>()
        {
            if (typeof(T).IsPrimitive)
                return default(T);

            ConstructorInfo cInfo = typeof(T).GetConstructor(Type.EmptyTypes);

            if (cInfo == null)
                return default(T);

            return (T)cInfo.Invoke(null);
        }

        /// <summary>
        /// Creates a default instance of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Instance of the specified type.</returns>
        /// <remarks>
        /// This method uses the generic <see cref="Default"/> method to create an instance
        /// of the type.
        /// </remarks>
        [DebuggerStepThrough]
        public static object DefaultByType(Type type)
        {
            MethodInfo generic = _genericDefaultMethod.MakeGenericMethod(type);

            return generic.Invoke(null, null);
        }

        #endregion

        #region Private Methods

        [DebuggerStepThrough]
        private static object ConvertByGeneric(object fromValue, Type toType)
        {
            MethodInfo generic = _genericConvertMethod.MakeGenericMethod(toType);

            dynamic outVal = generic.Invoke(null, new object[] { fromValue });

            return outVal;
        }

        [DebuggerStepThrough]
        private static ToType ConvertInternal<ToType>(object fromValue)
        {
            try
            {

                if (typeof(ToType).IsAssignableFrom(fromValue.GetType()))
                    return (ToType)fromValue;

                //Casting a boxed object to a type even though the type supports an explicit cast to
                //that type will fail. The only way to do this is to try to find the explicit or
                //implicit type conversion operator on the to type that supports the from type.
                MethodInfo mi = typeof(ToType).GetMethods().FirstOrDefault(m =>
                    (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                    m.ReturnType == typeof(ToType) &&
                    m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == fromValue.GetType()
                    );

                if (mi == null)
                {
                    //We can search for the reverse one on the original type too...
                    mi = fromValue.GetType().GetMethods().FirstOrDefault(m =>
                        (m.Name == "op_Explicit" || m.Name == "op_Implicit") &&
                        m.ReturnType == typeof(ToType) &&
                        m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == fromValue.GetType()
                        );
                }

                if (mi != null)
                    return (ToType)mi.Invoke(null, new object[] { fromValue });

                throw new InvalidCastException(string.Format("Cast was invalid {0} {1}", fromValue.GetType(), typeof(ToType)));
            }
            catch
            {
                throw;
            }
        }

        #endregion

    }
}
