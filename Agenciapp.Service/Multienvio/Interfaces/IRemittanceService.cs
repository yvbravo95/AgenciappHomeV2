using Agenciapp.Service.Multienvio.Implementation;
using Agenciapp.Service.Multienvio.Models;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Multienvio.Interfaces
{
    public interface IRemittanceService
    {
        string AuthorizationId { get; set; }
        /// <summary>
        /// Crear un tramite en multienvio
        /// </summary>
        /// <param name="remittance">Transaction Id</param>
        /// <returns></returns>
        Result<string> Create(CreateRemittanceModel remittance);
        Result Edit(EditRemittanceModel remittance);
        Result Cancel(string transactionId);
    }
}
