using ProjectCore.Shared.RequestContext;
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
        //public string Zone
        //{
        //    get
        //    {
        //        var nameClaim = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "PZ Polder";
        //        if (nameClaim == null)
        //        {
        //            throw new InvalidOperationException("Name claim is not available");
        //        }

        //        // Extract tenant from name, e.g. "De Bruyne Michiel (PZ Polder)" -> "PZ Polder"
        //        var start = nameClaim.IndexOf('(') + 1;
        //        var end = nameClaim.IndexOf(')', start);
        //        return nameClaim.Substring(start, end - start);
        //    }
        //}

        public string RequestScheme => _httpContextAccessor.HttpContext?.Request.Scheme;
        public string RequestHost => _httpContextAccessor.HttpContext?.Request.Host.Value;
        public string RequestPath => _httpContextAccessor.HttpContext?.Request.Path;
        public string RequestQueryString => _httpContextAccessor.HttpContext?.Request.QueryString.Value;
    }
}
