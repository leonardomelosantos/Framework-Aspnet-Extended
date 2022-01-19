using System.Net.Http;
using FrameworkAspNetExtended.Entities;

namespace FrameworkAspNetExtended.Core
{
    public interface IApplicationApiManagerCustomOperations
	{
		bool ValidateBasicAuthentication(UserAutenticatedInfo user, HttpRequestMessage request);
	}
}
