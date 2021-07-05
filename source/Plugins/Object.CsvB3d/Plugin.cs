﻿using System;
using System.IO;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
    public partial class Plugin : ObjectInterface
    {
		private static HostInterface currentHost;
		private static CompatabilityHacks enabledHacks;
	    private static string CompatibilityFolder;

	    public override string[] SupportedStaticObjectExtensions => new[] { ".b3d", ".csv" };

	    public override void Load(HostInterface host, FileSystem fileSystem) {
		    currentHost = host;
		    CompatibilityFolder = fileSystem.GetDataFolder("Compatibility");
	    }

	    public override void SetCompatibilityHacks(CompatabilityHacks EnabledHacks)
	    {
		    enabledHacks = EnabledHacks;
	    }


	    public override bool CanLoadObject(string path)
	    {
		    path = path.ToLowerInvariant();
		    if (path.EndsWith(".b3d") || path.EndsWith(".csv"))
		    {
			    if (System.IO.File.Exists(path) && FileFormats.IsNautilusFile(path))
			    {
				    return false;
			    }
			    using (StreamReader fileReader = new StreamReader(path))
			    {
				    for (int i = 0; i < 100; i++)
				    {
					    try
					    {
						    string readLine = fileReader.ReadLine();
						    if (!string.IsNullOrEmpty(readLine) && readLine.IndexOf("meshbuilder", StringComparison.InvariantCultureIgnoreCase) != -1)
						    {
							    //We have found the MeshBuilder statement within the first 100 lines
							    //This must be an object (we hope)
								//Use a slightly larger value than for routes, as we're hoping for a positive match
							    return true;
						    }
					    }
					    catch
					    {
						    //ignored
					    }
						
				    }
			    }
		    }
			return false;
	    }

	    public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject unifiedObject)
	    {
		    try
		    {
			    unifiedObject = ReadObject(path, Encoding);
			    return true;
		    }
		    catch
		    {
			    unifiedObject = null;
				currentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following object: " + path);
		    }
		    return false;
	    }
    }
}
