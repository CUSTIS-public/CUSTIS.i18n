using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CUSTIS.I18N
{
    /// <summary> Расширения для поддержки обычной строки, как многоязычной. </summary>
    public static class MultiCulturalStringExtensions
    {
        /// <summary> Объект - ссылка на многоязычную строку, т.к. сама строка иммутабельна. </summary>
        private class MultiCulturalStringHolder
        {
            public MultiCulturalStringHolder()
            {
                BackingString = new MultiCulturalString(null);
            }

            public MultiCulturalString BackingString { get; set; }
        }

        private static readonly ConditionalWeakTable<string, MultiCulturalStringHolder> _mcsHolders =
            new ConditionalWeakTable<string, MultiCulturalStringHolder>();

        /// <summary> Получить обычную строку в виде многоязычной. </summary>
        public static MultiCulturalString AsMultiCulturalString(this string source)
        {
            if (source == null)
            {
                return MultiCulturalString.Empty;
            }
            MultiCulturalStringHolder holder;
            if (_mcsHolders.TryGetValue(source, out holder))
            {
                return holder.BackingString;
            }
            else
            {
                holder = new MultiCulturalStringHolder
                {
                    BackingString = MultiCulturalString.Empty.SetLocalizedString(CultureInfo.CurrentUICulture, source)
                };
                _mcsHolders.Add(source, holder);
                return holder.BackingString;
            }
        }

        /// <summary> Предоставить строку, для которой можно получить многоязычные локализации. </summary>
        public static string AsString(this MultiCulturalString value)
        {
            Contract.Assert(value != null, "Нельзя задавать null многоязычное значение для строки");

            var weakTableKey = Unintern(value.GetString());
            _mcsHolders.GetOrCreateValue(weakTableKey).BackingString = value;
            return weakTableKey;
        }

        /// <summary> Установить значение строки в заданной культуре. </summary>
        public static string SetLocalizedValue(this string source, string value, CultureInfo culture)
        {
            var mcs = source.AsMultiCulturalString();
            mcs = mcs.SetLocalizedString(culture, value);
            return mcs.AsString();
        }

        /// <summary> Установить значение строки в текущей UI культуре. </summary>
        public static string SetLocalizedValue(this string source, string value)
        {
            return source.SetLocalizedValue(value, CultureInfo.CurrentUICulture);
        }

        /// <summary> 
        /// Получить значение в культуре <paramref name="culture"/> с использованием resourceFallbackProcess по умолчанию. 
        /// </summary>
        public static string ToLocalizedString(this string source, CultureInfo culture)
        {
            return source.AsMultiCulturalString().ToString(culture);
        }

        /// <summary> 
        /// Получить значение в культуре <paramref name="culture"/> с использованием <paramref name="resourceFallbackProcess"/>. 
        /// </summary>
        public static string ToLocalizedString(this string source, IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture)
        {
            return source.AsMultiCulturalString().GetString(resourceFallbackProcess, culture);
        }

        /// <summary> 
        /// Получить значение в культуре <paramref name="culture"/>. 
        /// Если <paramref name="useFallback"/> == <c>true</c>, то с использованием <paramref name="resourceFallbackProcess"/>. 
        /// </summary>
        public static string ToLocalizedString(this string source, IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture, bool useFallback)
        {
            return source.AsMultiCulturalString().ToString(resourceFallbackProcess, culture, useFallback);
        }

        /// <summary> 
        /// Получить значение в текущей UI культуре с использованием <paramref name="resourceFallbackProcess"/>. 
        /// </summary>
        public static string ToLocalizedString(this string source, IResourceFallbackProcess resourceFallbackProcess)
        {
            return source.AsMultiCulturalString().ToString(resourceFallbackProcess);
        }

        /// <summary> 
        /// Получить значение в текущей UI культуре. 
        /// Если <paramref name="useFallback"/> == <c>true</c>, то будет использован resourceFallbackProcess по умолчанию.
        /// </summary>
        public static string ToLocalizedString(this string source, bool useFallback)
        {
            return source.AsMultiCulturalString().ToString(useFallback);
        }

        /// <summary> 
        /// Получить значение в культуре <paramref name="culture"/>.
        /// Если <paramref name="useFallback"/> == <c>true</c>, то будет использован resourceFallbackProcess по умолчанию.
        /// </summary>
        public static string ToLocalizedString(this string source, CultureInfo culture, bool useFallback)
        {
            return source.AsMultiCulturalString().ToString(culture, useFallback);
        }

        /// <summary> 
        /// Получить значение в текущей UI культуре с использованием resourceFallbackProcess по умолчанию. 
        /// </summary>
        public static string ToLocalizedString(this string source)
        {
            return source.AsMultiCulturalString().ToString();
        }

        private static string Unintern(this string source)
        {
            source = source ?? string.Empty;
            return string.Copy(source);
        }
    }
}