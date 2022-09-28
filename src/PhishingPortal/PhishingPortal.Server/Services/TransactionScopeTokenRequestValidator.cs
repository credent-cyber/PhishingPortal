using Duende.IdentityServer.Validation;
using System.Security.Claims;

namespace PhishingPortal.Server.Services
{
    public class TransactionScopeTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            var transaction = context
                    .Result
                    .ValidatedRequest
                    .ValidatedResources
                    .ParsedScopes.FirstOrDefault(x => x.ParsedName == "transaction");

            // transaction scope has been requested
            if (transaction?.ParsedParameter != null)
            {
                // emit transaction id as a claim
                context.Result.ValidatedRequest.ClientClaims.Add(
                    new Claim(transaction.ParsedName, transaction.ParsedParameter));

                // also shorten token lifetime
                context.Result.ValidatedRequest.AccessTokenLifetime = 10;
            }

            return Task.CompletedTask;
        }
    }

}
