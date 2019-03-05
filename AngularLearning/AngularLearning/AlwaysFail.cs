using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AngularLearning
{
	public class AlwaysFailRequirement :
		AuthorizationHandler<AlwaysFailRequirement>,
		IAuthorizationRequirement
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			AlwaysFailRequirement requirement)
		{
			context.Fail();
			return Task.FromResult(0);
		}
	}
}