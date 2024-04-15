using FrameworkAspNetExtended.Entities;
using System.Net.Http;

namespace FrameworkAspNetExtended.Core
{
    public interface IApplicationApiManagerCustomOperations
    {
        bool ValidateBasicAuthentication(UserAutenticatedInfo user, HttpRequestMessage request);
    }
}
