using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public class StoryGoals {

		public const string EXIT_POD5 = "Goal_Lifepod2";
		public const string REPAIR_LIFEPOD = "RepairLifepod";
		public const string AURORA_BOUNCEBACK = "RadioBounceBack"; //9...9....9....9...
		public const string MAKE_SEAMOTH = "Goal_Seamoth";
		public const string MAKE_PRAWN = "Goal_Exo";
		public const string MAKE_CYCLOPS = "Goal_Cyclops";
		public const string AURORA_FIX = "AuroraRadiationFixed";
		public const string ROCKET_COMPLETE = "RocketComplete"; //added by DI
		public const string LAUNCH_ROCKET = "???";

		public const string POD12RADIO = "RadioKoosh26";
		public const string POD12 = "Lifepod2";

		public const string POD6RADIO = "RadioShallows22"; //RadioShallows22NoSignalAltDatabank
		public const string POD6A = "LifepodCrashZone1";
		public const string POD6B = "LifepodCrashZone2";

		public const string POD17RADIO = "RadioGrassy21";
		public const string POD17 = "LifepodSeaglide"; //ozzy

		public const string POD3RADIO = "RadioGrassy25";
		public const string POD3 = "Lifepod3";

		public const string POD2RADIO = "RadioBloodKelp29";
		public const string POD2 = "Lifepod1";

		public const string POD13RADIO = "RadioMushroom24";
		public const string POD13 = "Lifepod4"; //khasar

		public const string POD7RADIO = "RadioKelp28"; //RadioKelp28NoSignalAltDatabank
		public const string POD7 = "LifepodRandom";

		public const string POD19RADIO = "RadioSecondOfficer"; //keen
		public const string POD19AUDIO = "LifepodKeenDialog";
		public const string POD19RENDEZVOUS = "LifepodKeenLog";

		public const string POD4RADIO = "RadioRadiationSuit"; //RadioRadiationSuitNoSignalAltDatabank
		public const string POD4 = "LifepodDecoy";

		public const string ISLAND_RENDEZVOUS = "RendezvousFloatingIsland";

		public const string SUNBEAM_START = "RadioSunbeam1";
		public const string SUNBEAM_UPDATE = "RadioSunbeam2"; //I didn't know
		public const string SUNBEAM_FILLER = "RadioSunbeam3"; //no bad without the good
		public const string SUNBEAM_TIMER_START = "RadioSunbeam4";

		public const string SUNBEAM_DESTROY_START = "SunbeamCheckPlayerRange";
		public const string SUNBEAM_DESTROY_NEAR = "PDASunbeamDestroyEventInRange";
		public const string SUNBEAM_DESTROY_FAR = "PDASunbeamDestroyEventOutOfRange";

		public const string ALTERRA_HQ = "RadioCaptainsQuartersCode"; //The regular
		public const string ROCKET_INFO = "Aurora_RingRoom_Terminal3";


		public const string FIRST_DEGASI_LOG = "IslandsPDABase1bDesk";
		public const string PAUL_START_LOG = "IslandsPDABase1Interior";
		public const string ISLAND_JELLYSHROOM_PDA = "IslandsPDAExterior";
		public const string MAIDA_SEAMOTH_LOG = "JellyPDAExterior";
		public const string FINAL_DEGASI_LOG = "DeepPDA3";
		public const string PAUL_DEATH_LOG = "DeepPDA4";
		public const string BART_DEATH_LOG = "IslandsPDABase1a";


		public const string INFECTED_REJECTION = "Precursor_Gun_DisableDenied";
		public const string DRF_DESTROY_LOG = "Precursor_LostRiverBase_DataDownload3";
		public const string KHARAA_REVEAL = "Precursor_LostRiverBase_Log3";
		public const string INFECTED_CINEMATIC = "Precursor_LostRiverBase_DataDownload4";
		public const string EMPEROR_HATCH = "SeaEmperorBabiesHatched";
		public const string CURED = "Infection_Progress5";
		public const string DISABLE_GUN = "Goal_Disable_Gun";


		public const string LAVA_CASTLE_HINT = "Precursor_LavaCastle_Log2"; //this is not a mistake; they are backwards
		public const string LAVA_CASTLE_ENTRY = "Precursor_LavaCastle_Log1";

		public static string getRadioPlayGoal(string goal) {
			return "OnPlay" + goal;
		}

	}
}
