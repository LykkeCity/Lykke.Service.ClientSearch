using Lykke.Service.ClientSearch.Core;

namespace Lykke.Service.ClientSearch
{
    /// <summary>
    /// Validator for auth api key
    /// </summary>
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly AppSettings _settings;

        public ApiKeyValidator(AppSettings settings)
        {
            _settings = settings;
        }

        public bool Validate(string apiKey)
        {
            return apiKey == _settings.ClientSearchService.ApiKey;
        }
    }
}
