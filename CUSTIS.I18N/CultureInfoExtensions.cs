using System.Diagnostics.Contracts;
using System.Globalization;

namespace CUSTIS.I18N
{
    /// <summary> Методы-расширения для культур. </summary>
    public static class CultureInfoExtensions
    {
        /// <summary> Вычисляет, является ли культура инвариантной. </summary>
        /// <param name="cultureInfo">Культура.</param>
        /// <returns>Если культура инвариантна, <c>true</c>, иначе - <c>false</c>.</returns>
        public static bool IsInvariant(this CultureInfo cultureInfo)
        {
            Contract.Requires(cultureInfo != null);

            return cultureInfo.Name == CultureInfo.InvariantCulture.Name;
        }
    }
}