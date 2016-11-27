using System;
using System.Globalization;

namespace CUSTIS.I18N
{
    /// <summary> Информация о локализации объектов. </summary>
    [Serializable]
    public sealed class LocalizationFormatInfo : IFormatProvider, ICloneable
    {
        private static readonly LocalizationFormatInfo Default = new LocalizationFormatInfo(null);

        /// <summary> Информация о локализации объектов. </summary>
        /// <param name="culture">Культура отображения форматируемого объекта.</param>
        /// <param name="useFallback">Использовать ли fallback.</param>
        /// <param name="provider">Провайдер других форматов.</param>
        public LocalizationFormatInfo(CultureInfo culture, bool useFallback = true, IFormatProvider provider = null)
        {
            _culture = culture;
            _provider = provider;
            _useFallback = useFallback;
        }

        /// <summary> Получить объект с информацией о каком-либо формате по типу этого объекта. </summary>
        public object GetFormat(Type formatType)
        {
            if (formatType == GetType())
            {
                return this;
            }

            if (Provider != null)
            {
                // Если есть другой провайдер форматирования, то запрашиваем сведения у него.
                return Provider.GetFormat(formatType);
            }

            return null;
        }

        /// <summary> Культура отображения форматируемого объекта. Может быть <see langword="null"/>. </summary>
        public CultureInfo Culture
        {
            get { return _culture; }
        }
        private readonly CultureInfo _culture;

        /// <summary> Провайдер других форматов. Может быть <see langword="null"/>. </summary>
        public IFormatProvider Provider
        {
            get { return _provider; }
        }
        private readonly IFormatProvider _provider;

        /// <summary> Использовать ли fallback process. По умолчанию - <see langword="true"/>. </summary>
        public bool UseFallback
        {
            get { return _useFallback; }
        }
        private readonly bool _useFallback;

        /// <summary> 
        /// Получить из <paramref name="provider"/> экземпляр <see cref="LocalizationFormatInfo"/>.
        /// </summary>
        /// <param name="provider">Провайдер объектов форматирования. Может быть <see langword="null"/>.</param>
        /// <returns>Экземпляр <see cref="LocalizationFormatInfo"/>.</returns>
        public static LocalizationFormatInfo GetInstance(IFormatProvider provider)
        {
            LocalizationFormatInfo lfi = null;
            // Сначала пытаемся получить из провайдера
            if (provider != null)
            {
                lfi = provider.GetFormat(typeof(LocalizationFormatInfo)) as LocalizationFormatInfo;
            }

            return lfi ?? Default;
        }

        /// <summary> Создает копию текущего экземпляра.</summary>
        public object Clone()
        {
            return (LocalizationFormatInfo)MemberwiseClone();
        }
    }
}