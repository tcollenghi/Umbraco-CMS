using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Middleware
{
    /// <summary>
    /// Ensures that the ApplicationUrl is set on the first request.
    /// </summary>
    public class ApplicationUrlMiddleware : IMiddleware
    {
        private readonly IRequestAccessor _requestAccessor;
        private bool _applicationUrlSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUrlMiddleware"/> class.
        /// </summary>
        /// <param name="requestAccessor">Request accessor to set the URL in</param>
        public ApplicationUrlMiddleware(IRequestAccessor requestAccessor)
        {
            _requestAccessor = requestAccessor;
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // We only want to se the url once here, this will ensure that the middleware doesn't use excessive resources.
            // It's not optimal that is is called for every request though, the problem with doing it in a service with an event though
            // is that it won't register the event until the service is created the first time.
            if (!_applicationUrlSet)
            {
                _requestAccessor.GetApplicationUrl();
                _applicationUrlSet = true;
            }

            await next(context);
        }
    }
}
