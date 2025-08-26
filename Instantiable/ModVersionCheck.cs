using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public class ModVersionCheck : IComparable<ModVersionCheck> {

		private static readonly string VERSION_FILE = "current-version.txt";

		public readonly string modName;

		public readonly Func<ModVersion> remoteVersion;
		public readonly ModVersion currentVersion;

		private ModVersion fetchedRemoteVersion;

		private static readonly List<ModVersionCheck> modVersions = new List<ModVersionCheck>();

		public ModVersionCheck(string n, ModVersion cur, Func<ModVersion> rem) {
			modName = n;
			remoteVersion = rem;
			currentVersion = cur;
		}

		public void register() {
			modVersions.Add(this);
			modVersions.Sort();
			SNUtil.log("Registered version check " + this, SNUtil.tryGetModDLL(true));
		}

		public int CompareTo(ModVersionCheck other) {
			return string.Compare(modName, other.modName, StringComparison.InvariantCultureIgnoreCase);
		}

		public override string ToString() {
			return string.Format("[ModVersionCheck ModName={0}, RemoteVersion={1}, CurrentVersion={2}]", modName, fetchedRemoteVersion, currentVersion);
		}

		private void performRemoteCheck() {
			fetchedRemoteVersion = remoteVersion.Invoke();
		}

		public bool isOutdated() {
			if (fetchedRemoteVersion == null) //if not yet fetched for some reason block the thread until it is
				fetchedRemoteVersion = remoteVersion.Invoke();
			return !this.hasVersionError() && fetchedRemoteVersion.CompareTo(currentVersion) > 0;
		}

		public bool hasVersionError() {
			return fetchedRemoteVersion == ModVersion.ERROR || currentVersion == ModVersion.ERROR;
		}

		public static ModVersionCheck getFromGitVsInstall(string modName, Assembly a, string repo) {
			return new ModVersionCheck(modName, getFromInstall(a), () => getModifiedTimeFromGitFile(repo));
		}

		private static ModVersion getFromInstall(Assembly a) {
			try {
				string local = Path.Combine(Path.GetDirectoryName(a.Location), VERSION_FILE);
				string text = File.ReadAllLines(local)[0];
				return ModVersion.parse(text);
			}
			catch (Exception ex) {
				SNUtil.log("Failed to get local git version: " + ex.ToString());
				return ModVersion.ERROR;
			}
		}

		private static ModVersion getModifiedTimeFromGitFile(string repo) {
			try {
				DateTime prev = DateTime.Now;
				SNUtil.log("Fetching remote version from " + repo, SNUtil.diDLL);
				string url = "https://raw.githubusercontent.com/ReikaKalseki/"+repo+"/main/"+VERSION_FILE;
				string text = new WebClient().DownloadString(url);
				ModVersion mv = ModVersion.parse(text);
				DateTime after = DateTime.Now;
				SNUtil.log("Version check for " + repo + " done in " + (after - prev).TotalSeconds.ToString("0.000") + " seconds; version = " + mv, SNUtil.diDLL);
				return mv;
			}
			catch (Exception ex) {
				string str = ex.ToString();
				SNUtil.log("Failed to get remote " + repo + " git version: " + str, SNUtil.diDLL);
				if (str.StartsWith("System.Net.WebException", StringComparison.InvariantCultureIgnoreCase) && str.Contains("ConnectFailure")) {
					SNUtil.log("Could not connect to server!", SNUtil.diDLL);
					SNUtil.createPopupWarning("Could not connect to " + repo + " GitHub for versions. Check your internet/firewall/proxy settings.", false);
				}
				return ModVersion.ERROR;
			}
		}

		public static List<ModVersionCheck> getOutdatedVersions() {
			return modVersions.FindAll(mv => mv.isOutdated());
		}

		public static List<ModVersionCheck> getErroredVersions() {
			return modVersions.FindAll(mv => mv.hasVersionError());
		}

		public static void fetchRemoteVersions() {
			if (DIMod.config.getBoolean(DIConfig.ConfigEntries.THREADVER)) {
				Thread t = new Thread(doFetchRemoteVersions);
				t.Name = "Remote mod version checks";
				t.Start();
			}
			else {
				doFetchRemoteVersions();
			}
		}

		private static void doFetchRemoteVersions() {
			SNUtil.log("Fetching remote mod versions", SNUtil.diDLL);
			foreach (ModVersionCheck mv in modVersions) {
				mv.performRemoteCheck();
			}
		}

	}

	public class ModVersion : IComparable<ModVersion> {

		public static readonly string dateFormat = "dd/MM/yyyy HH:mm";

		internal static readonly ModVersion ERROR = new ModVersion(-1, DateTime.MinValue);

		public readonly int version;
		public readonly DateTime date;

		public ModVersion(int ver, DateTime d) {
			version = ver;
			date = d;
		}

		public override string ToString() {
			return this == ERROR ? "INVALID" : "v" + version + " @ " + date.ToString(dateFormat);
		}

		public override int GetHashCode() {
			return version;
		}

		public override bool Equals(object o) {
			return o is ModVersion && ((ModVersion)o).version == version;
		}

		public int CompareTo(ModVersion other) {
			return version.CompareTo(other.version);
		}

		public static ModVersion parse(string input) {
			if (string.IsNullOrEmpty(input))
				throw new Exception("No version information present!");
			input = input.Trim();
			if (input[0] == 'v' || input[0] == 'V')
				input = input.Substring(1);
			int idx = input.IndexOf('@');
			bool at = idx > 0;
			idx = input.IndexOf(' ', Math.Max(idx, 0));
			int idx2 = at ? input.IndexOf(' ') : idx;
			return new ModVersion(int.Parse(input.Substring(0, idx2)), DateTime.ParseExact(input.Substring(idx + 1), dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));
		}

	}
}
