using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace rifa_csharp.Utils;

public class SwashbuckleFormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParameterNames = new HashSet<string>();

            foreach (var parameter in context.ApiDescription.ActionDescriptor.Parameters)
            {
                if (parameter.ParameterType == typeof(IFormFile))
                {
                    formFileParameterNames.Add(parameter.Name);
                }
                else if (IsFormFileEnumerable(parameter.ParameterType))
                {
                    formFileParameterNames.Add(parameter.Name);
                }
            }

            if (!formFileParameterNames.Any())
                return;

            var parametersToRemove = operation.Parameters
                .Where(p => formFileParameterNames.Contains(p.Name))
                .ToList();

            foreach (var parameter in parametersToRemove)
            {
                operation.Parameters.Remove(parameter);
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "multipart/form-data",
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = formFileParameterNames.ToDictionary(
                                    name => name,
                                    name => new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                )
                            }
                        }
                    }
                }
            };
        }

        private static bool IsFormFileEnumerable(Type type)
        {
            return type.IsGenericType &&
                   (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                    type.GetGenericTypeDefinition() == typeof(List<>)) &&
                   type.GetGenericArguments()[0] == typeof(IFormFile);
        }
    }