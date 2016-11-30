using System.Net.Http;
using EvokeDish.Abstractions;

namespace EvokeDish.Common.Droid
{
	public class HttpClientHandlerFactory : IHttpClientHandlerFactory
	{
		public HttpClientHandler GetHttpClientHandler()
		{
			// not needed on Android
			return null;
		}
	}
}

