using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SplatNet2.Net.Api.Network
{
    public class LoggingHandler : DelegatingHandler
    {
        public string FirstRedirect { get; set; }

        private readonly bool loggingEnabled;

        public LoggingHandler(HttpMessageHandler innerHandler, bool loggingEnabled = false)
            : base(innerHandler)
        {
            this.loggingEnabled = loggingEnabled;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!this.loggingEnabled)
                return await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            this.FirstRedirect = request.RequestUri.AbsoluteUri;

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            return response;
        }
    }
}
