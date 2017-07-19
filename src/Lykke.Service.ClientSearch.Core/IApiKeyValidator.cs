namespace Lykke.Service.ClientSearch.Core
{
    public interface IApiKeyValidator
    {
        bool Validate(string apiKey);
    }
}
