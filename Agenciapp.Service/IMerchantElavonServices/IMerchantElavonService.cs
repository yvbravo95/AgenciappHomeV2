using Agenciapp.Common.Request;
using Agenciapp.Service.IMerchantElavonServices.Models;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.IMerchantElavonServices
{
    public interface IMerchantElavonService
    {
        Task<Result<AuthenticateResponse>> Authenticate();
    }
    public class MerchantElavonService : IMerchantElavonService
    {
        private readonly ISettingMerchantElavon _settingMerchantElavon;
        public MerchantElavonService(ISettingMerchantElavon settingMerchantElavon)
        {
            _settingMerchantElavon = settingMerchantElavon;
        }
        public async Task<Result<AuthenticateResponse>> Authenticate()
        {
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["username"] = _settingMerchantElavon.username;
                parameters["password"] = _settingMerchantElavon.password;
                var response = await RequestApi.SendGetAsync("https://uat.gw.fraud.eu.elavonaws.com/token", parameters);
                if (response.IsSuccess)
                {
                    var data = JsonConvert.DeserializeObject<AuthenticateResponse>(response.Value);
                    return Result.Success(data);
                }
                else
                {
                    return Result.Failure<AuthenticateResponse>("No se ha podido autenticar.");
                }
            }
            catch(Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<AuthenticateResponse>("No se ha podido autenticar.");
            }
        }
    }
}
