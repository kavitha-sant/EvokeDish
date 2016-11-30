using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EvokeDish.Abstractions
{
    /// <summary>
    /// A factory that produces HttpClientHandlers.
    /// </summary>
    public interface IHttpClientHandlerFactory
    {
        /// <summary>
        /// Gets a HttpClientHandler.
        /// </summary>
        /// <returns>A HttpClientHandler.</returns>
        HttpClientHandler GetHttpClientHandler();
    }
}
