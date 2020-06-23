using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Com.Sample.Responses;

namespace Com.Sample.Interfaces
{
    public interface IAuthorizationService
    {
        Task<bool> IsAuthroziedUser(string accessToken);

        Task<PrincipalDataResponse> GetValidatedPrincipalData(string accessToken);
    }

}
