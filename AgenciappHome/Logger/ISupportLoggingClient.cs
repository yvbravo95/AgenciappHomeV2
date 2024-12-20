using AgenciappHome.Logger.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgenciappHome.Logger
{
    public interface ISupportLoggingClient
    {
        Task<LoggingBaseResponse> LogIndex(IndexRequest index);
    }
}
