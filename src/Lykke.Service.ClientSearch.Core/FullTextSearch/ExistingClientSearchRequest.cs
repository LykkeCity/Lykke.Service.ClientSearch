using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Lykke.Service.ClientSearch.Core.FullTextSearch
{
    [SwaggerSchemaFilter(typeof(SwaggerDateTimeToDateExampleFilter))]
    public class ExistingClientSearchRequest
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
