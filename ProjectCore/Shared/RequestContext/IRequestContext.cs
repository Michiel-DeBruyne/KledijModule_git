﻿namespace ProjectCore.Shared.RequestContext
{
    public interface IRequestContext
    {
        string UserName { get; }
        string RequestScheme { get; }
        string RequestHost { get; }
        string RequestPath { get; }
        string RequestQueryString { get; }
    }
    //https://www.reddit.com/r/dotnet/comments/jpfgly/passing_logged_in_user_information_to_my/
}
