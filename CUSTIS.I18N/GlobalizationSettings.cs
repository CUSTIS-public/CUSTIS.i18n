using System;
using System.Diagnostics.Contracts;

namespace CUSTIS.I18N
{
    /// <summary> Настройки глобализации <see cref="IResourceFallbackProcess"/>, <see cref="MultiCulturalString"/> </summary>
    public sealed class GlobalizationSettings
    {
        #region Публичное API

        /// <summary> Экземпляр настроек глобализации </summary>
        public static GlobalizationSettings Current
        {
            get { return Settings.Value; }
        }

        /// <summary> Fallback по умолчанию для <see cref="MultiCulturalString"/>. Не <c>null</c>. </summary>
        public IResourceFallbackProcess MultiCulturalStringResourceFallbackProcess
        {
            get
            {
                Contract.Ensures(Contract.Result<IResourceFallbackProcess>() != null);

                return _mcsFallback ?? GetDefaultFallback();
            }
            set
            {
                Contract.Requires(value != null);

                _mcsFallback = value;
            }
        }

        /// <summary> Возвращает fallback по умолчанию </summary>
        public static IResourceFallbackProcess GetDefaultFallback()
        {
            Contract.Ensures(Contract.Result<IResourceFallbackProcess>() != null);
            return DeafultNetResourceFallbackSeq;
        }

        #endregion

        #region Внутренности

        private IResourceFallbackProcess _mcsFallback;

        private static readonly Lazy<GlobalizationSettings> Settings =
            new Lazy<GlobalizationSettings>(() => new GlobalizationSettings());

        private static readonly IResourceFallbackProcess DeafultNetResourceFallbackSeq =
            new ChainsResourceFallbackProcess(new[] {new[] {"*", ""}});

        #endregion
    }
}