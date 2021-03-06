﻿using Bit.OData.ODataControllers;
using Bit.OData.Serialization;
using Bit.WebApi.Contracts;
using Bit.WebApi.Implementations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Bit.OData.ActionFilters
{
    [SwaggerIgnoreAuthorizeAttribute] // It's just for async reading of request body which is possible in AuthorizeActionFilter. Following codes will gets executed before OData DeSerializers.
    public class ReadRequestContentStreamAsyncActionFilter : AuthorizeAttribute, IWebApiConfigurationCustomizer
    {
        public virtual void CustomizeWebApiConfiguration(HttpConfiguration webApiConfiguration)
        {
            webApiConfiguration.Filters.Add(this);
        }

        /// <summary>
        /// <see cref="ExtendedODataDeserializerProvider.GetODataDeserializer(System.Type, HttpRequestMessage)"/>
        /// <see cref="DefaultODataActionCreateUpdateParameterDeserializer.Read(Microsoft.OData.ODataMessageReader, System.Type, Microsoft.AspNet.OData.Formatter.Deserialization.ODataDeserializerContext)"/>
        /// </summary>
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpActionDescriptor actionDescriptor = actionContext.Request.GetActionDescriptor();

            if (actionDescriptor != null && (actionDescriptor.GetCustomAttributes<ActionAttribute>().Any() ||
                actionDescriptor.GetCustomAttributes<CreateAttribute>().Any() ||
                actionDescriptor.GetCustomAttributes<UpdateAttribute>().Any() ||
                actionDescriptor.GetCustomAttributes<PartialUpdateAttribute>().Any()))
            {
                using (StreamReader requestStreamReader = new StreamReader(await actionContext.Request.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                {
                    actionContext.Request.Properties["ContentStreamAsJson"] = await requestStreamReader.ReadToEndAsync().ConfigureAwait(false);
                }
            }

            await base.OnAuthorizationAsync(actionContext, cancellationToken).ConfigureAwait(false);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return true;
        }
    }
}
