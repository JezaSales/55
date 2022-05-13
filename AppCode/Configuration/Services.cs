using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RoundpayFinTech.AppCode.Auth;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RoundpayFinTech.AppCode.Configuration
{
    public static class Services
    {
        public static void AddService(this IServiceCollection service)
        {
            service.AddScoped<IUnitOfWork, UnitOfWork>();
            service.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fintech APIs",
                    Version = "v1.1",
                    Contact = new OpenApiContact
                    {
                        Name = "Amit Singh",
                        Email = "np4652@gmail.com",
                        Url = new Uri("https://github.com/np4652")
                    }
                });
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ApiDoc.xml");
                //var filePath = Path.Combine(System.AppContext.BaseDirectory, "ApiDoc.xml");
                option.IncludeXmlComments(filePath);
                option.OperationFilter<AddRequiredHeaderParameter>();
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                option.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                option.AddSecurityRequirement(
                    new OpenApiSecurityRequirement{
                        { jwtSecurityScheme, Array.Empty<string>() }
                    });
                option.UseAllOfToExtendReferenceSchemas();
            });
        }
    }
}
