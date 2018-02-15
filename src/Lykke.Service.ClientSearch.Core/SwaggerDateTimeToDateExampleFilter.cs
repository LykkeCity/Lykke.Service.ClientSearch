using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch
{
    public class SwaggerDateTimeToDateExampleFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            foreach(var prop in model.Properties)
            {
                if (context.SystemType.GetProperty(prop.Key).PropertyType == typeof(DateTime))
                {
                    prop.Value.Example = DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
            }
        }
    }

}
