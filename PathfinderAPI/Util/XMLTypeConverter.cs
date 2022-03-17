using Hacknet;
using Microsoft.Xna.Framework;

namespace Pathfinder.Util;

public delegate object ToTypeConverter(string s);
public delegate string FromTypeConverter(object o);

public static class XMLTypeConverter
{
    private class ConversionFailureException : Exception
    {
        public ConversionFailureException(string val, Type t, Exception inner) : base($"The value \"{(val is null ? "is null and" : $"\"{val}\"")}\" could not be converted to {t.Name}", inner) { }
        public ConversionFailureException(object o, Exception inner) : base($"The object {(o is null ? "is null and" : $"\"{o.ToString()}\"")} could not be converted to a string", inner) { }
    }

    private class TypeConverter
    {
        public ToTypeConverter ToType;
        public FromTypeConverter FromType;

        public TypeConverter(ToTypeConverter toType, FromTypeConverter fromType)
        {
            ToType = toType;
            FromType = fromType;
        }
    }
        
    private static readonly Dictionary<Type, TypeConverter> TypeConverters = new Dictionary<Type, TypeConverter>()
    {
        { typeof(string), new TypeConverter(s => s, s => (string)s) },
        { typeof(byte), new TypeConverter(s => byte.Parse(s), b => b.ToString()) },
        { typeof(int), new TypeConverter(s => int.Parse(s), i => i.ToString()) },
        { typeof(uint), new TypeConverter(s => uint.Parse(s), i => i.ToString()) },
        { typeof(long), new TypeConverter(s => long.Parse(s), i => i.ToString()) },
        { typeof(ulong), new TypeConverter(s => ulong.Parse(s), i => i.ToString()) },
        { typeof(float), new TypeConverter(s => float.Parse(s), f => f.ToString()) },
        { typeof(double), new TypeConverter(s => double.Parse(s), f => f.ToString()) },
        { typeof(bool), new TypeConverter(s => bool.Parse(s), b => b.ToString()) },
        { typeof(Computer), new TypeConverter(s => ComputerLookup.Find(s), c => ((Computer)c).idName) },
        { typeof(Color), new TypeConverter(s => Utils.convertStringToColor(s), c => Utils.convertColorToParseableString((Color)c)) }
    };

    public static object ConvertToType(Type t, string s)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            t = t.GenericTypeArguments[0];
        }

        try
        {
            return TypeConverters[t].ToType(s);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundException($"No type converter exists for Type {t.Name}", e);
        }
        catch (Exception e)
        {
            throw new ConversionFailureException(s, t, e);
        }
    }

    public static string ConvertToString(object o, Type t = null)
    {
        if (t == null)
        {
            if (o == null)
                return null; //Should this error instead?

            t = o.GetType();
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (o == null)
                return null;

            t = t.GenericTypeArguments[0];
        }

        try
        {
            return TypeConverters[t].FromType(o);
        }
        catch (KeyNotFoundException e)
        {
            throw new KeyNotFoundException($"No type converter exists for Type {t.Name}", e);
        }
        catch (Exception e)
        {
            throw new ConversionFailureException(o, e);
        }
    }

    public static void AddTypeConverter(Type t, ToTypeConverter to, FromTypeConverter from)
    {
        TypeConverters.Add(t, new TypeConverter(to, from));
    }
}