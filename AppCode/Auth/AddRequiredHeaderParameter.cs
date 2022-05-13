using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoundpayFinTech.AppCode.Auth
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation?.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();
            if (context != null && context.ApiDescription.RelativePath.Contains("PlanServices"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Jwt Bearer Token",
                    Schema = new OpenApiSchema { Type = "string",Format= "Bearer " },
                    AllowEmptyValue = true,
                    Required = true // set to false if this is optional
                });
            }

            //Do delete this code
            //if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
            //{
            //    // If [AllowAnonymous] is not applied or [Authorize] or Custom Authorization filter is applied on either the endpoint or the controller
            //    if (!context.ApiDescription.CustomAttributes().Any((a) => a is AllowAnonymousAttribute)
            //        && (context.ApiDescription.CustomAttributes().Any((a) => a is AuthorizeAttribute)
            //            || descriptor.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>() != null))
            //    {
            //        if (operation.Security == null)
            //            operation.Security = new List<OpenApiSecurityRequirement>();

            //        operation.Security.Add(new OpenApiSecurityRequirement{
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Name = "Authorization",
            //                In = ParameterLocation.Header,
            //                BearerFormat = "Bearer token",
            //                Reference = new OpenApiReference
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                },
            //            },
            //            new string[]{ }
            //        }
            //        });
            //    }
            //}
        }
    }
}
