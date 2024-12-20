using Agenciapp.Service.TranslateService.Models;
using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.TranslateService
{
    public interface ITranslateService
    {
        Task<string> Translate(SupportedLanguages languageCode, string text);
    }

    public class TranslateService: ITranslateService
    {
        private readonly SettingTranslate _setting;
        public TranslateService(IOptions<SettingTranslate> optionsSetting)
        {
            _setting = optionsSetting.Value;
        }

        public async Task<string> Translate(SupportedLanguages languageCode, string text)
        {
            using (IAmazonTranslate client = new AmazonTranslateClient(_setting.AccessKey, _setting.SecretAccessKey))
            {
                TranslateTextRequest request = new TranslateTextRequest()
                {
                    SourceLanguageCode = "auto",
                    TargetLanguageCode = languageCode.ToString(),
                    Text = text
                };

                var result = await client.TranslateTextAsync(request);
                return result.TranslatedText;
            }
        }
    }
}
