using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Expense.Tracker.Application.Extensions
{
    [ExcludeFromCodeCoverage]
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                schema.Type = "string";
                schema.Format = null;
                
                foreach (var enumName in Enum.GetNames(context.Type))
                {
                    schema.Enum.Add(new OpenApiString(enumName));
                }
            }
        }
    }
}
