using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ArtifactOfPotential
{
	public static class Assets
	{
		public static AssetBundle AssetBundle;
		public const string bundleName = "aopassetbundle";
		public static string AssetBundlePath
		{
			get
			{
				return ArtifactOfPotential.PInfo.Location.Replace("ArtifactOfPotential.dll", bundleName);
			}
		}

		public static void Init()
		{
			AssetBundle = AssetBundle.LoadFromFile(AssetBundlePath);
			if (AssetBundle == null)
			{
				Log.LogInfo("Failed to load AssetBundle!");
				return;
			}
		}
	}
}
