using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using CUSTIS.I18N.Properties;

namespace CUSTIS.I18N
{
    /// <summary> Fallback-менеджер для списка цепочек </summary>
    /// <remarks> При вызове <see cref="GetFallbackChain"/> 
    /// среди цепоцек находится та, чья стартовая культура наиболее "близка" к заданной. 
    /// 
    /// Не допускаются цепочки, начинающиеся с одной и той же культуры.
    /// Первой культурой в одной из цепочек должно быть специальное значение <value>"*"</value>, 
    /// которое обозначает любую культуру, не перечисленную в других цепочках. 
    /// Пример цепочек со <value>"*"</value>:
    /// например <value>"*,ru"</value>value>, или <value>"*,ru,"</value>, или <value>"*,"</value>.
    /// Последняя соответсвует стандартному Resource Fallback Process .NET.
    /// Порядок цепочек несущественен. 
    /// </remarks>
    [Serializable]
    public sealed class ChainsResourceFallbackProcess : IResourceFallbackProcess
    {
        /// <summary> Символ дял обоpначения любой культуры </summary>
        private const string AnyCultureSymbol = "*";

        private readonly Dictionary<CultureInfo, CultureInfo[]> _cultureChains;
        private readonly CultureInfo[] _defaultChain;

        /// <summary> Fallback-менеджер для списка цепочек </summary>
        /// <param name="cultureChains">Набор цепочек культур</param>
        public ChainsResourceFallbackProcess(string[][] cultureChains)
        {
            Contract.Requires(cultureChains != null);

            InitializeChains(cultureChains, out _cultureChains, out _defaultChain);
        }

        #region IResourceFallbackProcess implementation

        /// <summary> 
        /// Для заданной культуры возвращает цепочку культур в том порядке, в котором необходимо
        /// искать ресурсы. 
        /// </summary>
        /// <param name="initial">Начальная культура.</param>
        public IEnumerable<CultureInfo> GetFallbackChain(CultureInfo initial)
        {
            var stopCultures = new HashSet<string>(_cultureChains.Keys.Select(ci => ci.Name));

            var stdFallbackPart = new List<CultureInfo>();
            var current = initial;
            stdFallbackPart.Add(current);
            if (!stopCultures.Contains(current.Name))
            {
                current = current.Parent;
                while (!stopCultures.Contains(current.Name) && !current.IsInvariant())
                {
                    stdFallbackPart.Add(current);
                    current = current.Parent;
                }
                if (!current.IsInvariant())
                {
                    stdFallbackPart.Add(current);
                }
            }

            if (stopCultures.Contains(current.Name))
            {
                return stdFallbackPart.Concat(_cultureChains[current]).Distinct().ToArray();
            }
            else
            {
                if (_defaultChain.Any())
                {
                    return stdFallbackPart.Concat(_defaultChain).Distinct().ToArray();
                }
                else
                {
                    return new [] { initial };
                }
            }
        }

        #endregion


        #region Object implementation

        /// <summary> Проверка на равенство другому объекту. </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ChainsResourceFallbackProcess;
            if (other == null)
                return false;

            return EqualTo(_defaultChain, other._defaultChain)
                   && EqualTo(_cultureChains, other._cultureChains);
        }

        /// <summary> Вычисление уникального хэш-кода. </summary>
        public override int GetHashCode()
        {
            return _cultureChains.GetHashCode() ^ _defaultChain.GetHashCode();
        }

        private static bool EqualTo<TKey, TValue>(Dictionary<TKey, TValue[]> a, Dictionary<TKey, TValue[]> b)
        {
            TValue[] tmp;
            foreach (var pair in a)
            {
                if (!b.TryGetValue(pair.Key, out tmp))
                    return false;
                if (!EqualTo(pair.Value, tmp))
                    return false;
            }
            return true;
        }

        private static bool EqualTo<TValue>(TValue[] a, TValue[] b)
        {
            if (a.Length != b.Length) return false;
            for (var index = 0; index < a.Length; index++)
            {
                if (!a[index].Equals(b[index]))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion


        #region Внутренние методы

        private void InitializeChains(IEnumerable<string[]> cultureChains, out Dictionary<CultureInfo, CultureInfo[]> chains, out CultureInfo[] defaultChain)
        {
            // Проверяем уникальность первых культур 
            var allChains = cultureChains
                .Select(ch => new KeyValuePair<string, string[]>(ch.FirstOrDefault(), ch.Skip(1).ToArray()))
                .Where(c => !string.IsNullOrEmpty(c.Key))
                .ToArray();
            var distinctCultures = allChains.Select(ch => ch.Key).Distinct();
            if (allChains.Count() != distinctCultures.Count())
            {
                throw new InvalidOperationException(Resources.ChainsFallbackFirstCultureMustBeUniqueMessage);
            }

            // Проверяем уникальность "*"
            var anyCultureChains = allChains.Where(c => c.Key == AnyCultureSymbol).ToArray();
            if (anyCultureChains.Count() > 1)
            {
                throw new InvalidOperationException(Resources.ChainsFallbackAnyCultureChainMustbeUniqueMessage);
            }
            if (anyCultureChains.Count() == 0)
            {
                throw new InvalidOperationException(Resources.ChainsFallbackAnyCultureChainNotExistsMessage);
            }

            chains = allChains.Where(ch => ch.Key != AnyCultureSymbol)
                              .ToDictionary(c => CultureInfo.GetCultureInfo(c.Key),
                                            c => c.Value.Select(CultureInfo.GetCultureInfo).ToArray());

            defaultChain = anyCultureChains.Any()
                               ? anyCultureChains.SingleOrDefault().Value
                                                 .Select(CultureInfo.GetCultureInfo)
                                                 .ToArray()
                               : null;
        }

        #endregion
    }
}