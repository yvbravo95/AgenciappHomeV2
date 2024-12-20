using System;

namespace AgenciappHome.Models.Exceptions
{
    public class AppException : Exception
    {
        public AppException(Exception ex, dynamic responseData) : base(ex.Message,ex.InnerException)
        {
            ResponseData = responseData;
        }

        public object ResponseData { get; }
    }
}
