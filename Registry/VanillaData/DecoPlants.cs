using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra {
	public class DecoPlants : PrefabReference {

		private static readonly Dictionary<string, DecoPlants> lookup = new Dictionary<string, DecoPlants>();

		public static readonly DecoPlants PINK_BULB_STACK = new DecoPlants("6d9e37de-f808-4621-a762-e0d6340b30dc"); //rotated 90 - rotate x to -90 if using
		public static readonly DecoPlants DUNES_GRASS = new DecoPlants("214977c9-c4e5-49c6-a32f-f99cb0e0695c");
		public static readonly DecoPlants DUNES_BUSH = new DecoPlants("7ecc9cdd-3afc-4005-bff7-01ba62e95a03");
		public static readonly DecoPlants CRAG_GRASS = new DecoPlants("22bf7b03-8154-410b-a6fb-8ba315f68987"); //purple with white dot ends	
		public static readonly DecoPlants CRAG_GRASS_2 = new DecoPlants("450bf7b5-b6cf-4139-921f-3cb9ea505d5f"); //pale with white dot ends	
		public static readonly DecoPlants SPIKY_BUSH = new DecoPlants("26940e53-d3eb-4770-ae99-6ce4335445d3");
		public static readonly DecoPlants MUSHROOM_FLOWER = new DecoPlants("3dbe5ecd-0c60-46f5-a310-506817b02670");
		public static readonly DecoPlants FLAT_FAN = new DecoPlants("598c95d8-7420-4907-8f70-ba18b4e6adcb");
		public static readonly DecoPlants FLAT_FAN_2 = new DecoPlants("c71f41ce-b586-4e85-896e-d25e8b5b9de0");
		public static readonly DecoPlants VIOLET_DOT = new DecoPlants("7c7e0e95-8311-4ee0-80dd-30a61b151161");
		public static readonly DecoPlants BRANCHING_BUSH = new DecoPlants("a71da66c-6d43-45c1-bc7f-a789cfc61e46");
		public static readonly DecoPlants BRANCHING_BUSH_2 = new DecoPlants("aa1abbb9-716c-44b8-a2b8-cb4d9d0f22bb");
		public static readonly DecoPlants SEASHELL_FAN = new DecoPlants("be8b0e66-0cde-428b-be78-9d6bf06eaef4");
		public static readonly DecoPlants TANGLE_BUSH = new DecoPlants("c87e584c-7e38-4589-b408-8eca51f474c1"); //blue with forked spiraling red ends
		public static readonly DecoPlants MUSHROOM_VASE_STRANDS = new DecoPlants("898efb6d-b57b-41a3-9d3e-753fdc537651"); //https://i.imgur.com/3l00bw4.jpeg

		public static readonly DecoPlants CORAL_BRANCH = new DecoPlants(""); //forking flat branch with a "border"

		public static readonly DecoPlants RED_TIP_FERN = new DecoPlants("559fe0c7-1754-40f5-9453-b537900b3ac4");
		public static readonly DecoPlants RED_TIP_FERN_TALL = new DecoPlants("83f68b50-b037-4654-91db-2b378b67adeb");
		public static readonly DecoPlants FINGER_FERN = new DecoPlants("3b332e41-8d1b-4c7d-a132-3c98ab41c63d");
		public static readonly DecoPlants LAND_MIDDLE_8 = new DecoPlants("05c3df2b-8710-4aec-b2cb-242846e040a5"); //like dockleaf
		public static readonly DecoPlants LAND_MIDDLE_9 = new DecoPlants("e2be8784-75d6-4b86-941e-9aac73e0b72b"); //even more like dockleaf

		public static readonly DecoPlants VOXEL_FLOWER = new DecoPlants("2cab613d-2fc0-4012-ae6e-99f42d4262fd"); //voxel shrub bush
		public static readonly DecoPlants VOXEL_BUSH = new DecoPlants("e97c72ec-4999-48fa-b8b2-6d3f8791a7e8"); //voxel leaves only;
		public static readonly DecoPlants WALL_PINK_CAP = new DecoPlants("bfd64386-670a-44d6-9218-3835afec1042"); //wall mounted pink cap

		public static readonly DecoPlants CELERY_TREE = new DecoPlants("1cc51be0-8ea9-4730-936f-23b562a9256f");
		public static readonly DecoPlants VINE_TREE = new DecoPlants("abe4426a-5968-40b0-9d99-b06207984aa8");
		public static readonly DecoPlants VINE_TREE_2 = new DecoPlants("98b3ffc5-5497-49ad-8155-3608826ad373");

		public static readonly DecoPlants FERN_TREE = new DecoPlants("05400893-7eda-48d0-bd25-3977932f509c");
		public static readonly DecoPlants FERN_TREE_MULTI = new DecoPlants("154a88c1-6c7f-44e4-974e-c52d2f48fa28");
		public static readonly DecoPlants FERN_TREE_3 = new DecoPlants("a90e9c2f-97e3-4628-8e28-df909331b8ee");
		public static readonly DecoPlants HANGING_LEAF_CLUMP = new DecoPlants("6eae94e5-8fc8-4aef-ae41-ad8c081bcf4b");
		public static readonly DecoPlants LEAF_CROWN = new DecoPlants("7518da03-0e05-4d11-b154-8b192a9eab38");
		public static readonly DecoPlants LEAF_CROWN_2 = new DecoPlants("8798f4c7-f13d-4a8e-9947-b4f7fc1f1bae");
		public static readonly DecoPlants LEAF_CLUMP = new DecoPlants("75ab087f-9934-4e2a-b025-02fc333a5c99");
		public static readonly DecoPlants FAN_FERN = new DecoPlants("7d36a1fd-8aa1-4b32-9e05-23176e119f5f");
		public static readonly DecoPlants FAN_FERN_2 = new DecoPlants("8b28a530-120d-4bd4-8861-975b48b01570");
		public static readonly DecoPlants FERN_TREE_2 = new DecoPlants("8861b7cf-3c7b-481e-b4ff-83b49206acb8");
		public static readonly DecoPlants BANANA_LEAF = new DecoPlants("a4be67bb-f6e1-4d15-bf08-9d9a3fae4bfa");
		public static readonly DecoPlants BANANA_LEAF_2 = new DecoPlants("faf96875-22aa-401b-a144-4a4c856239d1");

		public static readonly DecoPlants LOST_BRANCHES_1 = new DecoPlants("200be3e2-dd25-4288-81b6-54476c1e210c"); //blue-tipped trunks
		public static readonly DecoPlants LOST_BRANCHES_2 = new DecoPlants("f5ebac74-4099-4af7-9b64-a1b1fad3fb1e");
		public static readonly DecoPlants LOST_BRANCHES_3 = new DecoPlants("1e6dc864-4259-485f-873e-0b65a1c20b15");
		public static readonly DecoPlants LOST_BRANCHES_4 = new DecoPlants("ecb2a647-95bb-4660-8380-20f2a5b76ec4"); //bigger versions
		public static readonly DecoPlants LOST_BRANCHES_5 = new DecoPlants("295c1c89-85c8-4ee0-995a-17db668f4fd9");
		public static readonly DecoPlants LOST_BRANCHES_6 = new DecoPlants("8aa03869-6224-4975-9b09-f6f449450caf"); //"giant", seen in ghost forest

		public static readonly DecoPlants LOST_ROOT_1 = new DecoPlants("04a2d0ec-8036-4945-812b-5dc51d17c5f6"); //aux LR roots
		public static readonly DecoPlants LOST_ROOT_2 = new DecoPlants("9dafed34-133e-43e4-9234-f012ec3872e2");
		public static readonly DecoPlants LOST_ROOT_3 = new DecoPlants("690e2455-05db-4c69-a48a-288b0a49082a");
		public static readonly DecoPlants LOST_ROOT_4 = new DecoPlants("41a08c65-ad37-4095-bd48-a8025fe4d016");

		private static readonly Dictionary<string, DecoPlants> names = new Dictionary<string, DecoPlants>();

		public static DecoPlants getByName(string n) {
			populateNames();
			return names.ContainsKey(n) ? names[n] : null;
		}

		public static List<DecoPlants> getAll() {
			populateNames();
			return new List<DecoPlants>(names.Values);
		}

		private static void populateNames() {
			if (names.Count == 0) {
				foreach (FieldInfo f in typeof(DecoPlants).GetFields()) {
					if (f.IsStatic && f.FieldType == typeof(DecoPlants)) {
						DecoPlants vf = (DecoPlants)f.GetValue(null);
						names[f.Name] = vf;
						vf.name = f.Name;
					}
				}
			}
		}

		public readonly string prefab;

		private string name;

		private DecoPlants(string id) {
			prefab = id;
			lookup[id] = this;
		}

		public string getName() {
			if (name == null)
				populateNames();
			return name;
		}

		public string getPrefabID() {
			return prefab;
		}

		public static DecoPlants getFromID(string pfb) {
			return lookup.ContainsKey(pfb) ? lookup[pfb] : null;
		}

		public override string ToString() {
			return this.getName();
		}
	}
}
