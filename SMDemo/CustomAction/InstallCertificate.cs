namespace CustomActions
{
	using System.ComponentModel;
	using Microsoft.Deployment.WindowsInstaller;

	[RunInstaller(true)]
	public class InstallCertificate
	{

		[CustomAction]
		public static ActionResult InstallCertificateCustomActions(Session session)
		{
			return ActionResult.Success;
		}
	}
}
