using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace StaffPortal.Service.Configuration
{
    public class GenericListTypeConverter<T> : TypeConverter
    {
        protected readonly TypeConverter typeConverter;

        public GenericListTypeConverter()
        {
            typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter == null)
                throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType != typeof(string))
                return base.CanConvertFrom(context, sourceType);

            var items = GetStringArray(sourceType.ToString());
            return items.Any();
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string) && value != null)
                return base.ConvertFrom(context, culture, value);

            var items = GetStringArray((string)value);
            var result = new List<T>();
            Array.ForEach(items, s =>
            {
                var item = typeConverter.ConvertFromInvariantString(s);
                if (item != null)
                {
                    result.Add((T)item);
                }
            });

            return result;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            var result = string.Empty;
            if (value == null)
                return result;

            //we don't use string.Join() because it doesn't support invariant culture
            for (var i = 0; i < ((IList<T>)value).Count; i++)
            {
                var str1 = Convert.ToString(((IList<T>)value)[i], CultureInfo.InvariantCulture);
                result += str1;
                //don't add comma after the last element
                if (i != ((IList<T>)value).Count - 1)
                    result += ",";
            }

            return result;
        }

        protected string[] GetStringArray(string input)
        {
            return string.IsNullOrEmpty(input) ? new string[0] : input.Split(',').Select(x => x.Trim()).ToArray();
        }
    }
}
