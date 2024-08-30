﻿using ProjectCore.Shared.RequestContext;
using System.Security.Claims;
namespace KledijModule.Common.RequestContext
{
    public class RequestContextService : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

        public string RequestScheme => _httpContextAccessor.HttpContext?.Request.Scheme;
        public string RequestHost => _httpContextAccessor.HttpContext?.Request.Host.Value;
        public string RequestPath => _httpContextAccessor.HttpContext?.Request.Path;
        public string RequestQueryString => _httpContextAccessor.HttpContext?.Request.QueryString.Value;
    }
}
