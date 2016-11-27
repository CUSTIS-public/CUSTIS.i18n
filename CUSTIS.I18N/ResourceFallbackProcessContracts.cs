using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace CUSTIS.I18N
{
    /// <summary> Контракты для <see cref="IResourceFallbackProcess"/>. </summary>
    [ContractClassFor(typeof(IResourceFallbackProcess))]
    [Serializable]
    internal abstract class ResourceFallbackProcessContracts : IResourceFallbackProcess
    {
        /// <summary> 
        /// Для заданной культуры возвращает цепочку культур в том порядке, в котором необходимо
        /// искать ресурсы. 
        /// </summary>
        /// <param name="initial">Начальная культура.</param>
        public IEnumerable<CultureInfo> GetFallbackChain(CultureInfo initial)
        {
            Contract.Requires(initial != null);

            throw new NotImplementedException();
        }
    }
}