using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using NATUPNPLib;

namespace UPnP
{
	[ApiVersion(1, 17)]
	public class Plugin : TerrariaPlugin
	{
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public override string Name
		{
			get { return "UPnP-COM"; }
		}

		public override string Author
		{
			get { return "Simon311"; }
		}

		public override string Description
		{
			get { return "Adds UPnP."; }
		}

		public Plugin(Main game)
			: base(game)
		{
			Order = -1;
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, Start);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, Start);
				Stop();
			}
		}

		#region UPnP

		public static UPnPNAT upnpnat = (UPnPNAT)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("AE1E00AA-3FD5-403C-8A27-2BBDC30CD0E1")));
		public static IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
		public static string portForwardIP;
		public static int portForwardPort = 7777;
		public static bool portForwardOpen = false;

		public static bool openPort()
		{
			portForwardIP = Netplay.LocalIPAddress();
			portForwardPort = Netplay.serverPort;
			if (mappings == null)
			{
				Console.WriteLine("(UPnP) Your UPnP discovery is down.");
				TShock.Log.Info("(UPnP) Your UPnP discovery is down.");
				return false;
			}
			try
			{
				foreach (IStaticPortMapping staticPortMapping in mappings)
				{
					if (staticPortMapping.InternalPort == portForwardPort && staticPortMapping.InternalClient == portForwardIP && staticPortMapping.Protocol == "TCP")
					{
						portForwardOpen = true;
					}
				}
				if (!portForwardOpen)
				{
					mappings.Add(portForwardPort, "TCP", portForwardPort, portForwardIP, true, "Terraria Server");
					portForwardOpen = true;
				}
				return true;
			}
			catch { }
			return false;
		}

		public static bool closePort()
		{
			try
			{
				if (portForwardOpen) mappings.Remove(portForwardPort, "TCP");
				return true;
			}
			catch { }
			return false;
		}
		#endregion UPnP

		private void Start(EventArgs args)
		{
			if (openPort())
			{
				Console.WriteLine("(UPnP) Port Forward succesful.");
				TShock.Log.Info("(UPnP) Port Forward succesful.");
			}
			else
			{
				Console.WriteLine("(UPnP) Port Forward failed.");
				TShock.Log.Error("(UPnP) Port Forward failed.");
			}
		}

		private void Stop()
		{
			if (closePort())
			{
				
				Console.WriteLine("(UPnP) Port Dispose succesful.");
				TShock.Log.Info("(UPnP) Port Dispose succesful.");
			}
			else
			{
				Console.WriteLine("(UPnP) Port Dispose failed. (WTF?)");
				TShock.Log.Error("(UPnP) Port Dispose failed. (WTF?)");
			}
		}
	}
}