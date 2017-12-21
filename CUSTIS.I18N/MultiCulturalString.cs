using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace CUSTIS.I18N
{
    /// <summary> Многоязычная строка. </summary>    
    /// <remarks> Этот класс предназначен для хранения различных вариантов строки для разных культур. 
    /// Строки с <see langword="null"/>-значениями не сохраняются.
    /// </remarks>    
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    public sealed class MultiCulturalString : IFormattable, ISerializable
    {

        #region Поля

        private readonly Dictionary<string, string> _localizedStrings;

        #endregion


        #region Конструкторы

        /// <summary> Многоязычная строка. Ctor. </summary>
        private MultiCulturalString()
            : this(null)
        {
        }

        /// <summary> Многоязычная строка. Ctor. </summary>  
        /// <param name="localizedStrings">Список локализованных строк с указанием культуры.</param>
        /// <remarks>
        /// В списке не должно быть значений культур с одинаковыми именами. 
        /// Записи для <see langword="null"/>-значений не сохраняются.
        /// </remarks>
        public MultiCulturalString(IEnumerable<KeyValuePair<CultureInfo, string>> localizedStrings)
        {
            var sourceStrings = localizedStrings ?? new KeyValuePair<CultureInfo, string>[] { };

            _localizedStrings = sourceStrings
                .Where(kvp => kvp.Value != null)
                .ToDictionary(_ => _.Key.Name, _ => _.Value);
        }

        /// <summary> Многоязычная строка. Ctor. </summary>  
        /// <param name="localizedStrings">Список локализованных строк с указанием культуры.</param>
        /// <remarks>
        /// В списке не должно быть значений культур с одинаковыми именами. 
        /// Записи для <see langword="null"/>-значений не сохраняются.
        /// </remarks>
        public MultiCulturalString(IDictionary<CultureInfo, string> localizedStrings)
            : this(localizedStrings.AsEnumerable())
        {
        }

        /// <summary> Многоязычная строка. Ctor. 
        /// Создает многоязычную строку со значением для единственной культуры. </summary>
        /// <param name="culture">Культура.</param>
        /// <param name="value">Значение.</param>
        /// <remarks>
        /// Записи для <c><paramref name="value"/>==<see langword="null"/></c> в экземпляре класса не сохраняются.
        /// </remarks>
        public MultiCulturalString(CultureInfo culture, string value)
        {
            Contract.Requires(culture != null);

            _localizedStrings = value != null
                                    ? new Dictionary<string, string>
                                        {
                                            {culture.Name, value}
                                        }
                                    : new Dictionary<string, string>();
        }

        #endregion


        #region Сериализация

        private MultiCulturalString(SerializationInfo info, StreamingContext context)
        {
            _localizedStrings = new Dictionary<string, string>();
            foreach (var kvp in info)
            {
                _localizedStrings[kvp.Name] = (string) kvp.Value;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var kvp in _localizedStrings)
            {
                info.AddValue(kvp.Key, kvp.Value);
            }
        }

        #endregion


        #region Строковые методы

        /// <summary> 
        /// Имеет ли <paramref name="value"/> значение <c>null</c> или 
        /// <see cref="MultiCulturalString"/> только лишь с пустыми значениями?
        /// </summary>
        /// <returns>
        /// <c>true</c>, если параметр <paramref name="value"/> равен <c>null</c> или 
        /// для каждой культуры значение <paramref name="value"/> отсутствует или пустое; 
        /// в противном случае — <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(MultiCulturalString value)
        {
            return value == null || value.IsEmpty;
        }

        /// <summary> 
        /// Имеет ли <paramref name="value"/> значение <c>null</c> или 
        /// <see cref="MultiCulturalString"/> только лишь с пустыми или непечатаемыми значениями?
        /// </summary>
        /// <returns>
        /// <c>true</c>, если параметр <paramref name="value"/> равен <c>null</c> или 
        /// для каждой культуры значение <paramref name="value"/> отсутствует, пустое или состоит только из
        /// непечатаемых символов;
        /// в противном случае — <c>false</c>.
        /// </returns>
        public static bool IsNullOrWhiteSpace(MultiCulturalString value)
        {
            return value == null || value.IsWhiteSpace;
        }

        /// <summary>Форматирование мультикультурной строки. </summary>
        /// <param name="formatString">Мультикультурная строка формата.</param>
        /// <param name="args">Аргументы.</param>
        /// <returns>Отформатированная мультикультурная строка.</returns>
        /// <remarks>
        /// Результирующая строка имеет те же культуры, что и исходная строка формата <paramref name="formatString"/>. 
        /// Для форматирования аргументов в каждой культуре <c>culture</c> используется <c>new LocalizationFormatInfo(culture, null)</c>.
        /// Это значит, что объекты, поддерживающие для форматирования <see cref="LocalizationFormatInfo"/> (многоязычные строки, возможно, другие типы) будут представлены 
        /// в культуре <c>culture</c>, а стандартные типы (числа, даты) будут форматироваться по умолчанию.
        /// </remarks>
        public static MultiCulturalString Format(MultiCulturalString formatString, params object[] args)
        {
            Contract.Requires(formatString != null);
            Contract.Requires(args != null);
            Contract.Ensures(Contract.Result<MultiCulturalString>() != null);

            return Format(null, formatString, args);
        }

        /// <summary>Форматирование мультикультурной строки. </summary>
        /// <param name="formatProvider"> Провайдер форматирования. </param>
        /// <param name="formatString">Мультикультурная строка формата.</param>
        /// <param name="args">Аргументы.</param>
        /// <returns>Отформатированная мультикультурная строка.</returns>
        /// <remarks>
        /// Результирующая строка имеет те же культуры, что и исходная строка формата <paramref name="formatString"/>. 
        /// Для форматирования аргументов в каждой культуре <c>culture</c> используется <c>new LocalizationFormatInfo(culture, <paramref name="formatProvider"/>)</c>.
        /// Это значит, что объекты, поддерживающие для форматирования <see cref="LocalizationFormatInfo"/> (многоязычные строки, возможно, другие типы) будут представлены 
        /// в культуре <c>culture</c>, а стандартные типы (числа, даты) будут форматироваться <paramref name="formatProvider"/>.
        /// </remarks>
        public static MultiCulturalString Format(IFormatProvider formatProvider, MultiCulturalString formatString, params object[] args)
        {
            Contract.Requires(formatString != null);
            Contract.Requires(args != null);
            Contract.Ensures(Contract.Result<MultiCulturalString>() != null);

            var result = Empty;
            foreach (var culture in formatString.Cultures)
            {
                var lfi = new LocalizationFormatInfo(culture, true, formatProvider);
                result = result.SetLocalizedString(culture, string.Format(lfi, formatString.GetString(culture), args));
            }

            return result;
        }

        /// <summary>
        /// Объединение нескольких элементов в мультикультурную строку.
        /// </summary>
        /// <param name="separator">Разделитель.</param>
        /// <param name="args">Элементы.</param>
        /// <returns>Объединенная мультикультурная строка.</returns>
        public static MultiCulturalString Join(MultiCulturalString separator, params object[] args)
        {
            Contract.Requires(args != null);
            Contract.Ensures(Contract.Result<MultiCulturalString>() != null);

            var result = Empty;
            foreach (var culture in separator.Cultures)
            {
                var lfi = new LocalizationFormatInfo(culture);
                var newArgs = args.Select(a => string.Format(lfi, "{0}", a));
                result = result.SetLocalizedString(culture, string.Join(separator.GetString(culture), newArgs));
            }

            return result;
        }

        /// <summary> Возвращает новый экземпляр многоязычной строки со строкой <paramref name="localizedString"/> для культуры <paramref name="culture"/></summary>
        /// <param name="culture">Культура заменямой строки</param>
        /// <param name="localizedString">Строка</param>
        /// <returns></returns>
        public MultiCulturalString SetLocalizedString(CultureInfo culture, string localizedString)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<MultiCulturalString>() != null);
            
            var newData = _localizedStrings
                .ToDictionary(pair => CultureInfo.GetCultureInfo(pair.Key), pair => pair.Value);
            if (localizedString != null)
            {
                newData[culture] = localizedString;
            }
            else
            {
                newData.Remove(culture);
            }

            return new MultiCulturalString(newData);
        }

        /// <summary> Слияние двух многоязычных строк с приоритетом данной </summary>
        /// <param name="other">Другая строка, данные которой будут объединены с данной</param>
        /// <remarks>
        /// Приоритет имеет данная строка. Если значения для какой-либо культуры установлено
        /// как для <c>this</c>, так и для <paramref name="other"/>, будет использовано значение <c>this</c>.
        /// </remarks>
        /// <returns></returns>
        public MultiCulturalString MergeWith(MultiCulturalString other)
        {
            Contract.Requires(other != null);
            Contract.Ensures(Contract.Result<MultiCulturalString>() != null);

            var thisDictionary = _localizedStrings;
            var otherDictionary = other._localizedStrings;
            var resultDictionary = new Dictionary<string, string>();
            foreach (var kvp in otherDictionary)
            {
                resultDictionary[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in thisDictionary)
            {
                resultDictionary[kvp.Key] = kvp.Value;
            }
            return new MultiCulturalString(resultDictionary.Select(kvp =>
                                                                  new KeyValuePair<CultureInfo, string>(
                                                                      CultureInfo.GetCultureInfo(kvp.Key), kvp.Value)));
        }

        /// <summary> Присутствует ли в данном экземпляре строка с заданной культурой. </summary>
        /// <param name="culture">Культура.</param>
        /// <returns><true/>, если строка для заданной культуры присутствует.</returns>
        public bool ContainsCulture(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            return _localizedStrings.ContainsKey(culture.Name);
        }

        /// <summary> 
        /// Возвращает копию данной строки, приведенную к нижнему регистру. 
        /// Для каждой культуры преобразование производится по правилам этой культуры.
        /// </summary>
        public MultiCulturalString ToLower()
        {
            var result = Empty;
            foreach (var kvp in _localizedStrings)
            {
                var culture = CultureInfo.GetCultureInfo(kvp.Key);
                var lowerValue = kvp.Value.ToLower(culture);
                result = result.SetLocalizedString(culture, lowerValue);
            }
            return result;
        }

        /// <summary> 
        /// Возвращает копию данной строки, приведенную к верхнему регистру. 
        /// Для каждой культуры преобразование производится по правилам этой культуры.
        /// </summary>
        public MultiCulturalString ToUpper()
        {
            var result = Empty;
            foreach (var kvp in _localizedStrings)
            {
                var culture = CultureInfo.GetCultureInfo(kvp.Key);
                var upperValue = kvp.Value.ToUpper(culture);
                result = result.SetLocalizedString(culture, upperValue);
            }
            return result;
        }

        /// <summary> 
        /// Возвращает новую строку, в которой знаки данного экземпляра выровнены 
        /// по правому краю путем добавления слева символов-заполнителей до указанной общей длины.
        /// </summary>
        /// <param name="totalWidth">Минимальная длина результирующей строки.</param>
        /// <param name="paddingChar">Опциональный символ-заполнитель, по умолчанию - пробел.</param>
        /// <see cref="string.PadLeft(int,char)"/>
        /// <returns>
        /// Строка длины <paramref name="totalWidth"/> или более, полученная из 
        /// исходной добавлением символов-заполнителей слева.
        /// </returns>
        public MultiCulturalString PadLeft(int totalWidth, char paddingChar = ' ')
        {
            var res = Empty;
            foreach (var cultureInfo in Cultures)
            {
                res = res.SetLocalizedString(cultureInfo, GetString(cultureInfo).PadLeft(totalWidth, paddingChar));
            }
            return res;
        }

        /// <summary> 
        /// Возвращает новую строку, в которой знаки данного экземпляра выровнены 
        /// по правому краю путем добавления справа символов-заполнителей до указанной общей длины.
        /// </summary>
        /// <param name="totalWidth">Минимальная длина результирующей строки.</param>
        /// <param name="paddingChar">Опциональный символ-заполнитель, по умолчанию - пробел.</param>
        /// <see cref="string.PadRight(int,char)"/>
        /// <returns>
        /// Строка длины <paramref name="totalWidth"/> или более, полученная из 
        /// исходной добавлением символов-заполнителей справа.
        /// </returns>
        public MultiCulturalString PadRight(int totalWidth, char paddingChar = ' ')
        {
            var res = Empty;
            foreach (var cultureInfo in Cultures)
            {
                res = res.SetLocalizedString(cultureInfo, GetString(cultureInfo).PadRight(totalWidth, paddingChar));
            }
            return res;
        }

        #endregion


        #region Перегрузки ToString(), GetString()
        
        private string DebuggerDisplay
        {
            get
            {
                if (_localizedStrings != null && _localizedStrings.Any())
                {
                    const string separator = ", ";
                    const string mainFormat = "{0}: {1}";
                    const string countFormat = "... (Count = {0})";
                    const int threshold = 3;
                    const int maxLength = 10;
                    var formattedStrings = _localizedStrings
                        .Select(p => string.Format(mainFormat, p.Key,
                                                   p.Value != string.Empty
                                                       ? p.Value.Substring(0, Math.Min(maxLength, p.Value.Length))
                                                       : "(string.Empty)"));

                    if (_localizedStrings.Count > threshold)
                    {
                        return string.Join(separator,
                                           formattedStrings
                                               .Take(threshold)
                                               .Concat(new[]
                                                   {string.Format(countFormat, _localizedStrings.Count)}));
                    }
                    else
                    {
                        return string.Join(separator,
                                           formattedStrings);
                    }
                }
                else
                {
                    return "(Empty)";
                }
            }
        }

        /// <summary> 
        /// Возвращает строку для UI-культуры потока с использованием
        /// resourceFallbackProcess по умолчанию.
        /// </summary>
        /// <returns>
        /// Строка для UI-культуры потока 
        /// с использованием resourceFallbackProcess по умолчанию. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString()
        {
            return GetString(true);
        }

        /// <summary> Возвращает строку в UI-культуре потока. </summary>
        /// <param name="useFallback">Использовать ли resourceFallbackProcess по умолчанию.</param>
        /// <returns>
        /// Строка, локализованная в UI-культуре потока 
        /// с использованием resourceFallbackProcess по умолчанию. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(bool useFallback)
        {
            return GetString(CultureInfo.CurrentUICulture, useFallback);
        }

        /// <summary> 
        /// Возвращает строку в указанной культуре
        /// с использованием resourceFallbackProcess по умолчанию.
        /// </summary>
        /// <param name="culture">Культура.</param>
        /// <returns>
        /// Строка, локализованная в указанной культуре, 
        /// с использованием resourceFallbackProcess по умолчанию. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            return GetString(culture, true);
        }

        /// <summary> Возвращает строку в указанной культуре. </summary>
        /// <param name="culture">Культура.</param>
        /// <param name="useFallback">Использовать ли resourceFallbackProcess по умолчанию.</param>
        /// <returns>
        /// Строка, локализованная в UI-культуре потока. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(CultureInfo culture, bool useFallback)
        {
            Contract.Requires(culture != null);

            return GetString(null, culture, useFallback);
        }

        /// <summary> 
        /// Возвращает строку в UI-культуре потока, используя указанный resourceFallbackProcess.
        /// </summary>
        /// <param name="resourceFallbackProcess"> Используемый при локализации resourceFallbackProcess. 
        /// Если <c>null</c>, то используется resourceFallbackProcess по умолчанию. </param>
        /// <returns>
        /// Строка, локализованная в UI-культуре потока с использованием resourceFallbackProcess. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(IResourceFallbackProcess resourceFallbackProcess)
        {
            return GetString(resourceFallbackProcess, CultureInfo.CurrentUICulture);
        }

        /// <summary> Возвращает строку. </summary>
        /// 
        /// <param name="resourceFallbackProcess">
        /// Используемый при локализации resourceFallbackProcess. Если <c>null</c>, то используется resourceFallbackProcess по умолчанию.
        /// </param>
        /// <param name="culture">Культура локализации.</param>
        /// <returns>
        /// Строка, локализованная в культуре culture с 
        /// использованием resourceFallbackProcess. 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture)
        {
            Contract.Requires(culture != null);

            return GetString(resourceFallbackProcess, culture, true);
        }

        /// <summary> Возвращает строку. </summary>
        /// 
        /// <param name="resourceFallbackProcess">
        /// Используемый при локализации resourceFallbackProcess. Если <c>null</c>, то используется resourceFallbackProcess по умолчанию.
        /// </param>
        /// <param name="culture">Культура локализации.</param>
        /// <param name="useFallback">Использовать ли при локализации resourceFallbackProcess.</param>
        /// <returns>
        /// Строка, локализованная в культуре culture с 
        /// использованием resourceFallbackProcess (в зависимости от значения <paramref name="useFallback"/>). 
        /// Если не найдена локализованная строка,
        /// то возвращаемое значение равно <see langword="null"/>.
        /// </returns>
        public string GetString(IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture, bool useFallback)
        {
            Contract.Requires(culture != null);

            if (!useFallback)
            {
                return GetStringByCulture(culture);
            }

            resourceFallbackProcess = resourceFallbackProcess ?? DefaultResourceFallbackProcess;

            var cultures = Cultures.ToList();

            // В resourceFallbackProcess-цепочке находим первую культуру, которая содержится в строке и
            // возвращаем значение из многоязычной строки для этой культуры
            var bestCulture = resourceFallbackProcess
                .GetFallbackChain(culture)
                .FirstOrDefault(cultures.Contains);

            return bestCulture != null ? GetStringByCulture(bestCulture) : null;
        }

        /// <summary> 
        /// Возвращает строку в UI-культуре потока с использованием
        /// resourceFallbackProcess по умолчанию.
        /// </summary>
        /// <returns>
        /// Строка, локализованная в UI-культуре потока 
        /// с использованием resourceFallbackProcess по умолчанию. 
        /// </returns>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString() ?? string.Empty;
        }

        /// <summary> Возвращает строку в UI-культуре потока. </summary>
        /// <param name="useFallback">Использовать ли resourceFallbackProcess по умолчанию.</param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public string ToString(bool useFallback)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(useFallback) ?? string.Empty;
        }

        /// <summary> 
        /// Возвращает строку в указанной культуре
        /// с использованием resourceFallbackProcess по умолчанию.
        /// </summary>
        /// <param name="culture">Культура.</param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public string ToString(CultureInfo culture)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(culture) ?? string.Empty;
        }

        /// <summary> Возвращает строку в указанной культуре. </summary>
        /// <param name="culture">Культура.</param>
        /// <param name="useFallback">Использовать ли resourceFallbackProcess по умолчанию.</param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public string ToString(CultureInfo culture, bool useFallback)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(culture, useFallback) ?? string.Empty;
        }

        /// <summary> 
        /// Возвращает строку в UI-культуре потока, используя указанный resourceFallbackProcess.
        /// </summary>
        /// <param name="resourceFallbackProcess"> Используемый при локализации resourceFallbackProcess. 
        /// Если <c>null</c>, то используется resourceFallbackProcess по умолчанию. </param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public string ToString(IResourceFallbackProcess resourceFallbackProcess)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(resourceFallbackProcess) ?? string.Empty;
        }

        /// <summary> Возвращает строку. </summary>
        /// 
        /// <param name="resourceFallbackProcess">
        /// Используемый при локализации resourceFallbackProcess. Если <c>null</c>, то используется resourceFallbackProcess по умолчанию.
        /// </param>
        /// <param name="culture">Культура локализации.</param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo)"/>
        public string ToString(IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(resourceFallbackProcess, culture) ?? string.Empty;
        }

        /// <summary> Возвращает строку. </summary>
        /// 
        /// <param name="resourceFallbackProcess">
        /// Используемый при локализации resourceFallbackProcess. Если <c>null</c>, то используется resourceFallbackProcess по умолчанию.
        /// </param>
        /// <param name="culture">Культура локализации.</param>
        /// <param name="useFallback">Использовать ли resourceFallbackProcess.</param>
        /// <remarks> При помощи методов <see cref="ToString()"/> невозможно
        /// определить, задана ли для культуры пустая строка или значение 
        /// для культуры не задано в принципе.
        /// </remarks>
        /// <seealso cref="GetString(CultureInfo, bool)"/>
        /// <seealso cref="GetString(IResourceFallbackProcess, CultureInfo, bool)"/>
        public string ToString(IResourceFallbackProcess resourceFallbackProcess, CultureInfo culture, bool useFallback)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return GetString(resourceFallbackProcess, culture, useFallback) ?? string.Empty;
        }

        private string GetStringByCulture(CultureInfo culture)
        {
            string localizedString;
            return _localizedStrings.TryGetValue(culture.Name, out localizedString)
                ? localizedString
                : null;
        }

        /// <summary> Строковое представление, реализация <see cref="IFormattable"/>. </summary>
        /// <param name="format">Строка пользовательского формата. Не используется. </param>
        /// <param name="formatProvider">Провайдер форматирования. Используется для получения культуры строки, resourceFallbackProcess. 
        /// Информация м.б. получена из <see cref="LocalizationFormatInfo"/> или <see cref="CultureInfo"/>. </param>
        /// <returns></returns>
        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            var formatInfo = LocalizationFormatInfo.GetInstance(formatProvider);

            return ToString(formatInfo.Culture ?? CultureInfo.CurrentUICulture, formatInfo.UseFallback);
        }

        #endregion


        #region Свойства

        private static readonly MultiCulturalString _empty = new MultiCulturalString();

        /// <summary> Fallback, используемый в многоязычных строках по умолчанию. </summary>
        private static IResourceFallbackProcess DefaultResourceFallbackProcess
        {
            get
            {
                return GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess;
            }
        }

        /// <summary> Возвращает многоязычную строку, которая не содержит значения ни для какой культуры.</summary>
        public static MultiCulturalString Empty
        {
            get { return _empty; }
        }

        /// <summary> Получает список культур, на которые локализована данная строка. </summary>
        /// <remarks> В экземпляре хранятся лишь строки с не-<see langword="null"/> значениями. </remarks>
        public IEnumerable<CultureInfo> Cultures
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<CultureInfo>>() != null);

                return _localizedStrings.Select(_ => CultureInfo.GetCultureInfo(_.Key));
            }
        }

        /// <summary> Является ли мультикультурная строка пустой? </summary>
        public bool IsEmpty
        {
            get
            {
                return _localizedStrings.All(pair => string.IsNullOrEmpty(pair.Value));
            }
        }

        /// <summary> Содержит ли строка только пустые или непечатаемые значения? </summary>
        public bool IsWhiteSpace
        {
            get
            {
                return _localizedStrings.All(pair => string.IsNullOrWhiteSpace(pair.Value));
            }
        }

        #endregion


        #region Сравнение

        /// <summary> Сравнивает на равенство </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var otherMcs = obj as MultiCulturalString;
            if (otherMcs == null)
                return false;

            return _localizedStrings.Count == otherMcs._localizedStrings.Count
                   && _localizedStrings.All(p => otherMcs._localizedStrings.ContainsKey(p.Key)
                                                 && otherMcs._localizedStrings[p.Key] == p.Value);
        }

        /// <summary> Serves as a hash function for a multicultural string. </summary>
        public override int GetHashCode()
        {
            return _localizedStrings
                .Aggregate(0, (current, localizedString) =>
                              current ^ localizedString.Key.GetHashCode() + localizedString.Value.GetHashCode());
        }

        #endregion


        #region Проверки

        [ContractInvariantMethod]
        private void CheckInvariants()
        {
            Contract.Invariant(_localizedStrings.Values.All(s => s != null));
        }

        #endregion
    }
}
