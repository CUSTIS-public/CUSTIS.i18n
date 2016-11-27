using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace CUSTIS.I18N
{
    /// <summary> Обработка альтернативных ресурсов </summary>
    [ContractClass(typeof(ResourceFallbackProcessContracts))]
    public interface IResourceFallbackProcess
    {
        /// <summary> 
        /// Для заданной культуры возвращает цепочку культур в том порядке, в котором необходимо
        /// искать ресурсы. 
        /// </summary>
        /// <param name="initial">Начальная культура.</param>
        IEnumerable<CultureInfo> GetFallbackChain(CultureInfo initial);
    }
}