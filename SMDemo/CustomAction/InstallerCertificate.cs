namespace CustomActions
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Configuration.Install;
	using System.Security.Cryptography.X509Certificates;
	
	[RunInstaller(true)]
	public partial class InstallerCertificate : Installer
	{
		public InstallerCertificate()
		{
			InitializeComponent();
		}

		private static void InstallCertificate(StringDictionary parametrs)
		{
			try
			{
				string[] param = parametrs["assemblypath"].Split('\\');
				string certPath = String.Empty;

				for (int i = 0; i < param.Length - 1; i++)
				{
					certPath += param[i] + '\\';
				}
				certPath += "certificate.pfx";

				var cert = new X509Certificate2(certPath, "",
				  X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

				var store = new X509Store(StoreName.AuthRoot, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadWrite);
				store.Add(cert);
				store.Close();
			}
			catch (Exception ex)
			{
				throw new Exception("Certificate appeared to load successfully but also seems to be null.", ex);
			}
		}
		
		public override void Install(IDictionary stateSaver)
        {
			try
			{
				InstallCertificate(this.Context.Parameters);
				base.Install(stateSaver);
			}
			catch (Exception ex) 
			{
				throw new InstallException(ex.ToString());
			}
		}
		
	}

	
}
