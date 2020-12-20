using System;
using System.Collections.Generic;
using System.Text;

namespace SplatNet2.Net.Api.Exceptions
{
    public class ExpiredCookieException : Exception
    {
        public string ReAuthUrl { get; }

        public ExpiredCookieException(string message, string reAuth)
            : base(message)
        {
            this.ReAuthUrl = reAuth;
        }
    }
}
