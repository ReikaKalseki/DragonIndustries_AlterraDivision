using System;
using System.IO;
using System.Reflection;
using System.Linq;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;

using UnityEngine;
using UnityEngine.Serialization;

namespace ReikaKalseki.DIAlterra
{
	public class VanillaFlora : PrefabReference, Flora {
		
		private static readonly Dictionary<string, VanillaFlora> lookup = new Dictionary<string, VanillaFlora>();
		
		//median 4.81
		public static readonly VanillaFlora ANCHOR_POD_SMALL1 = new VanillaFlora("228e5af5-a579-4c99-9fb0-04b653f73cd3", 2.78, 1.36).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
		public static readonly VanillaFlora ANCHOR_POD_SMALL2 = new VanillaFlora("1645f35d-af23-4b98-b1e4-44d430421721", 5.08, 6.48).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
		public static readonly VanillaFlora ANCHOR_POD_MED1 = new VanillaFlora("1cafd118-47e6-48c4-bfd7-718df9984685", 4.81, 7.26).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
		public static readonly VanillaFlora ANCHOR_POD_MED2 = new VanillaFlora("7444baa0-1416-4cb6-aa9a-162ccd4b98c7", 4.48, 18.19).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
		public static readonly VanillaFlora ANCHOR_POD_LARGE = new VanillaFlora("c72724f3-125d-4e87-b82f-a91b5892c936", 4.56, 27.1).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
		
		public static readonly VanillaFlora ACID_MUSHROOM = new VanillaFlora(6, "31834aae-35ce-49c1-b5ba-ac4227750679", "61a5e0e6-01d5-4ae2-aea6-1186cd769025", "99cdec62-302b-4999-ba49-f50c73575a4d", "fc7c1098-13af-417a-8038-0053b65498e5").markResources().addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.REDGRASS, true);
		public static readonly VanillaFlora BLUE_PALM = new VanillaFlora(6, "210fdf87-54e0-4c83-9bf3-31bbc06f38a6", "50ebde28-dcd9-46be-bafd-9e2b483a1d22", "57a31bf5-5b86-4bf6-9a14-9291c6e8a79c").markHarvestable().addBiome(VanillaBiomes.SHALLOWS).addBiome(VanillaBiomes.MOUNTAINS, true);
		public static readonly VanillaFlora VEINED_NETTLE = new VanillaFlora("e80b22ff-064d-46ca-b71e-456d6b3426ab", 6).markHarvestable().addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaFlora WRITHING_WEED = new VanillaFlora(6, "3dbab1b9-cc52-4da4-8633-89b33add18f4", "7c6d23d1-4d59-49f8-ac12-b12dfa530beb", "e8047056-e202-49b3-829f-7458615103ac").markHarvestable().addBiome(VanillaBiomes.SHALLOWS);
		
		public static readonly VanillaFlora VIOLET_BEAU = new VanillaFlora("36fcb5c8-07f6-4d20-b026-f8c41b8e2358", 6).markHarvestable().addBiome(VanillaBiomes.REDGRASS, true).addBiome(VanillaBiomes.JELLYSHROOM).addBiome(VanillaBiomes.TREADER);
		public static readonly VanillaFlora CAVE_BUSH = new VanillaFlora("4856ff40-43d2-4b15-acdc-d6a45f85c157", 6, 0.1).markHarvestable().addBiome(VanillaBiomes.JELLYSHROOM).addBiome(VanillaBiomes.TREADER).addBiome(VanillaBiomes.MOUNTAINS, true);
		public static readonly VanillaFlora PAPYRUS = new VanillaFlora("4601400c-5e12-4e4a-9e45-4cab5f06a598", 6).markHarvestable().addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.JELLYSHROOM).addBiome(VanillaBiomes.DUNES, true).addBiome(VanillaBiomes.TREADER, true).addBiome(VanillaBiomes.MOUNTAINS, true);
		public static readonly VanillaFlora TIGER = new VanillaFlora("84794dd0-2c70-4239-9536-230d56811ad4", 5.9).markHarvestable().addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.CRAG);
		public static readonly VanillaFlora REDWORT = new VanillaFlora("b707aa52-1a27-43c4-9500-f346befb8251", 6).markHarvestable().addBiome(VanillaBiomes.REDGRASS).addBiome(VanillaBiomes.DUNES, true).addBiome(VanillaBiomes.CRASH).addBiome(VanillaBiomes.UNDERISLANDS);
		public static readonly VanillaFlora PINECONE = new VanillaFlora("d8838f12-2e24-40c9-a7c5-24fb9c08e934", -999).markResources().addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaFlora PYGMY_FAN = new VanillaFlora("ae0a831e-0f90-43bd-8183-c2002c528e9e", 6.5, 0.3).markHarvestable().addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaFlora PYGMY_FAN_HUGE = new VanillaFlora("e65b36c8-3b85-463c-8f30-95859401b2cb", 6, 1.8).addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaFlora ROUGE_CRADLE = new VanillaFlora(6.1, "4525e0f3-9c9a-449f-8d6c-48088711ac99", "99bbd145-d50e-4afb-bff0-27b33243642b").markHarvestable().addBiome(VanillaBiomes.REDGRASS, true);
		public static readonly VanillaFlora TREE_LEECH = new VanillaFlora(6, "2e57e9d2-ddda-4063-9540-ca2f0fae775e", "1dc87b04-84d4-42e1-afbf-ee8c2a9a236f", "2f97d40e-4ca0-44c7-9f8d-2e2111375c66").addBiome(VanillaBiomes.MUSHROOM); 
		
		public static readonly VanillaFlora KOOSH = new VanillaFlora("f90ba94f-326b-4cbd-bc95-4dc39addbf33", 6, 0.1).markEdible().addBiome(VanillaBiomes.KOOSH);
		public static readonly VanillaFlora LARGE_KOOSH = new VanillaFlora(6, 1.8, "a9958cbb-72eb-4a1d-af7b-13fbc947d8f3", "fcf04278-bfbb-409d-bada-a6f22564efde").addBiome(VanillaBiomes.KOOSH);
		public static readonly VanillaFlora EYE_STALK = new VanillaFlora(6.1, 0.3, "0089035b-4717-4975-b437-5b87cc3e2f8e", "11ea0dd6-015f-4528-bed7-18de03f54911", "320c9798-9e57-4055-8daa-d73a055c0d28").markHarvestable().addBiome(VanillaBiomes.CRAG).addBiome(VanillaBiomes.SPARSE).addBiome(VanillaBiomes.KOOSH, true);
		public static readonly VanillaFlora GELSACK = new VanillaFlora(6.1, "74f368f4-b08f-4b0c-ab96-c97e37911ff0", "bd4d4fa1-d10e-40e5-8ec6-67efd0ba03af", "dd037903-eb47-47f5-9d4f-83100aca4ec4").markResources().addBiome(VanillaBiomes.SPARSE).addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND).addBiome(VanillaBiomes.MOUNTAINS).addBiome(VanillaBiomes.LOSTRIVER).addBiome(VanillaBiomes.COVE).addBiome(VanillaBiomes.TREADER, true);
		public static readonly VanillaFlora MEMBRAIN = new VanillaFlora(6, "d0f7a4be-19e3-42ce-8bd0-e9c220a932f4", "0e2a3f36-881b-4c84-8a02-5bb1da4b9f29").markHarvestable().addBiome(VanillaBiomes.GRANDREEF);
		public static readonly VanillaFlora REGRESS = new VanillaFlora("1a806d20-dc8f-4e6e-9281-f353ed155abf", 6).markHarvestable().addBiome(VanillaBiomes.TREADER, true).addBiome(VanillaBiomes.DUNES, true).addBiome(VanillaBiomes.MOUNTAINS, true).addBiome(VanillaBiomes.BLOODKELPNORTH);
		public static readonly VanillaFlora SEACROWN = new VanillaFlora(6.05, 0.1, "2d422d6b-3c1f-484d-84ee-a07b5b8e32a4", "a1040915-abcf-4843-a16f-39a10d6a1c2d").markHarvestable().addBiome(VanillaBiomes.REDGRASS, true).addBiome(VanillaBiomes.KOOSH, true);
		public static readonly VanillaFlora HORNGRASS = new VanillaFlora("fec5bf85-8e70-48bb-9e9d-939d694632a5", 6.1, 0.1).markHarvestable().addBiome(VanillaBiomes.CRASH).addBiome(VanillaBiomes.TREADER).addBiome(VanillaBiomes.SPARSE);
		public static readonly VanillaFlora SPOTTED_DOCKLEAF = new VanillaFlora("b2636f23-f764-41ec-bfcf-f33d35d79641", 5.95).markHarvestable().addBiome(VanillaBiomes.MOUNTAINS, true).addBiome(VanillaBiomes.KOOSH).addBiome(VanillaBiomes.UNDERISLANDS, true);
		
		public static readonly VanillaFlora DEEP_MUSHROOM = new VanillaFlora(6, "29ab9e04-a045-413b-886b-e03fa6b86aee", "60fdf752-bc74-4f85-8a9c-72f86031a52f", "a6dac068-6f8d-4e32-b5e7-2e34a9f97d11", "e4ea0e38-7baa-49ce-b85c-89a22935574f").markResources().addBiome(VanillaBiomes.TREADER, true).addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.LOSTRIVER).addBiome(VanillaBiomes.COVE).addBiome(VanillaBiomes.ILZ);
		public static readonly VanillaFlora GHOSTWEED = new VanillaFlora("1bb43d52-19ee-4a3a-85ef-f85a152cc334", 6.1, 0.1).markHarvestable().addBiome(VanillaBiomes.BLOODKELP, true).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaFlora GABE_FEATHER = new VanillaFlora(6, 0.2, "79134868-2f8e-4f43-a99f-a6fb8ce60b48", "8409a079-a96c-43d3-a891-af500b04e0af").markHarvestable().addBiome(VanillaBiomes.BLOODKELP).addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.TREADER, true);
		public static readonly VanillaFlora BRINE_LILY = new VanillaFlora("f97bf790-a5bd-4e7f-a5e8-9fca1b37f81c", 6.5, 0.8).addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaFlora AMOEBOID = new VanillaFlora("375a4ade-a7d9-401d-9ecf-08e1dce38d6b", 6.3).addBiome(VanillaBiomes.LOSTRIVER);
		
		public static readonly VanillaFlora MING = new VanillaFlora(6, "1d5877a7-bc56-46c8-a27c-f9d0ab99cc80", "ce650c66-355c-4b77-ad4e-a2bea7e36c95").markHarvestable();
		public static readonly VanillaFlora BULBO = new VanillaFlora("4626f3eb-23c3-4e04-b9df-829cb051758a", 5.9).markEdible();
		public static readonly VanillaFlora JAFFA_CUP = new VanillaFlora("35056c71-5da7-4e73-be60-3c22c5c9e75c", 6, 0.3).markHarvestable();
		public static readonly VanillaFlora VOXEL = new VanillaFlora("28ec1137-da13-44f3-b76d-bac12ab766d1", 6.1).markHarvestable();
		public static readonly VanillaFlora POTATO = new VanillaFlora("533d54b0-e54a-4aec-8dd0-a9eb89868c59", 5.9, 0.2).markEdible();
		public static readonly VanillaFlora MARBLEMELON = new VanillaFlora("e9445fdf-fbae-49dc-a005-48c05bf9f401", 5.8, 0).markEdible();
		public static readonly VanillaFlora LARGE_MARBLEMELON = new VanillaFlora("a966a14f-d188-4de4-a488-f2c0302ca250", 6.2, 0.2).markEdible();
		public static readonly VanillaFlora FERN_PALM = new VanillaFlora(6, 0.3, "1d6d89dd-3e49-48b7-90e4-b521fbc3d36f", "523879d5-3241-4a94-8588-cb3b38945119").markHarvestable();
		public static readonly VanillaFlora GRUB_BASKET = new VanillaFlora("28c73640-a713-424a-91c6-2f5d4672aaea", 6).markHarvestable();
		public static readonly VanillaFlora LANTERN = new VanillaFlora("8fa4a413-57fa-47a3-828d-de2255dbce4f", 6.1, 0.5).markEdible();
		public static readonly VanillaFlora PINK_CAP = new VanillaFlora(5.9, "a7aef01f-0dc0-4d03-913d-d47d8d2ba407", "7f9a765d-0b4e-4b3f-81b9-38b38beedf55", "c7faff7e-d9ff-41b4-9782-98d2e09d29c1", "e88e7a23-2a99-41c5-aed9-a2bfaca3619d").markHarvestable();
		public static readonly VanillaFlora SPECKLED_RATTLER = new VanillaFlora(6, "98be0944-e0b3-4fba-8f08-ca5d322c22f6", "28818d8a-5e50-41f0-8e14-44cb89a0b611").markHarvestable();
		
		//blood kelp mini vines
		//2ab96dc4-5201-4a41-aa5c-908f0a9a0da8
		//2bfcbaf4-1ae6-4628-9816-28a6a26ff340
		//7bfe0629-a008-43b8-bd16-d69ad056769f
		//e291d076-bf95-4cdd-9dd9-6acd37566cf6
		
		//kelp grass tuft
		//880b59b7-8fd6-412f-bbcb-a4260b263124
		//bac42c90-8995-439f-be2f-29a6d164c82a
		
		//algae fern 9ec9e154-f265-4534-8657-69342454e9cd
		
		//large leafy land e2be8784-75d6-4b86-941e-9aac73e0b72b", "faf96875-22aa-401b-a144-4a4c856239d1", "05c3df2b-8710-4aec-b2cb-242846e040a5
		
		//trees
		//154a88c1-6c7f-44e4-974e-c52d2f48fa28
		//05400893-7eda-48d0-bd25-3977932f509c
		
		//clover 3b332e41-8d1b-4c7d-a132-3c98ab41c63d
		
		//yellow leafy thing 7ce83863-b4f2-4d46-9f46-1830799f3e5f 4d0c8cbd-6127-4681-9d86-d9175e6df722
		
		//red tipped leaf bush, land 559fe0c7-1754-40f5-9453-b537900b3ac4 83f68b50-b037-4654-91db-2b378b67adeb
		
		//big land leaf clump 6eae94e5-8fc8-4aef-ae41-ad8c081bcf4b
		//75ab087f-9934-4e2a-b025-02fc333a5c99
		//
		
		//large land ferns
		//7518da03-0e05-4d11-b154-8b192a9eab38
		//7d36a1fd-8aa1-4b32-9e05-23176e119f5f
		//8798f4c7-f13d-4a8e-9947-b4f7fc1f1bae
		//8861b7cf-3c7b-481e-b4ff-83b49206acb8
		//8b28a530-120d-4bd4-8861-975b48b01570
		//a4be67bb-f6e1-4d15-bf08-9d9a3fae4bfa
		//a90e9c2f-97e3-4628-8e28-df909331b8ee
		
		
		public static readonly VanillaFlora CREEPVINE = new VanillaFlora(6, "1fd4d86f-3b06-4369-945c-ca65f50b4800", "9bfe02bd-60a3-401b-b7a0-627c3bdc4451", "de0e28a2-7a17-4254-b520-5f0e28355059", "ee1baf03-0560-4f4d-ad29-13a337bef0d7").markEdible().addBiome(VanillaBiomes.KELP);
		public static readonly VanillaFlora CREEPVINE_FERTILE = new VanillaFlora(6, "9f9ab512-9ad6-4a84-80ea-6322a432bf80", "7329db6b-7385-4e77-8afa-71830ead9350", "a17ef178-6952-4a91-8f66-44e1d8ca0575", "de972f1f-daab-41d6-b274-5173b0dd23d8").markEdible().addBiome(VanillaBiomes.KELP);
		//public static readonly VanillaFlora CREEPVINE_SEEDS = new VanillaFlora("2a37dd2f-ee5e-4c3c-a3fe-4f5973055651", -999, -999).markResources();
		public static readonly VanillaFlora JELLYSHROOM_SMALL = new VanillaFlora("3e199d12-2d75-4c58-a819-d78beeb24e2c", 6).addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaFlora JELLYSHROOM_TINY = new VanillaFlora(6, "0642b532-9433-4f65-aa39-7757d954b7d2", "159a22bd-8ab9-479b-95c0-35b09ecdd8b7", "234a33e5-693f-4458-a916-5b1108c33fc2", "463e0571-9599-4d74-81dc-fc7932004554", "556b2ac6-1e6e-4597-bd1b-b0819ed82c3e", "580d12a7-9964-425d-adb0-f971a5aaa59b", "8ab168d7-dce9-4a2f-bbbc-79c3b632776f", "98ee9a60-3b80-426c-a181-d4b7883854f3", "c15ba497-90a0-41df-ba4b-a34a2dfbd6aa", "d00efe9c-3412-4592-9c85-866be52d34cf", "def10d70-f1ff-4f5d-8923-060a03a70fc0", "fcae5fbd-2e40-4946-a1bf-8b7109546019").addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaFlora JELLYSHROOM = new VanillaFlora("400fa668-152d-4b81-ad8f-a3cef16efed8", 5.9, 0.5).markHarvestable().addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaFlora JELLYSHROOM_LIVE = new VanillaFlora(6, 0.5, "8d0b24b7-c71f-42ab-8df9-7bfe05616ab4", "a3d11348-e589-4867-ac60-1fa122145615", "d586a247-122a-427d-9032-f42e898df17f").markHarvestable().addBiome(VanillaBiomes.JELLYSHROOM); //COMES WITH CRABSNAKE ;; 
		public static readonly VanillaFlora CRASHFISH = new VanillaFlora("eb38de5d-c3df-4446-a37a-d770fb0f92bb", 6.3, 0.5).addBiome(VanillaBiomes.SHALLOWS, true).addBiome(VanillaBiomes.KELP, true); //THIS IS A FULL CRASHFISH
		public static readonly VanillaFlora MUSHROOM_DISK = new VanillaFlora(0, "2613c023-4e16-4989-860d-ce81648f471c", "775b6835-bd08-40d2-b80e-ab0ddc539c45", "79527fc2-7037-41c0-9e3d-e003f3cd0b06", "d13bd79a-343f-496e-96dd-8e9c3fd3f3bb", "d551d2e9-e581-4dfc-b41c-1343ab8c1337", "e0d415d9-1bc6-4c8b-b3c0-69f5e5fa6b08").addBiome(VanillaBiomes.MUSHROOM);
		//public static readonly VanillaFlora BLOOD_ROOT = new VanillaFlora("4bfe1877-1b83-4d5d-9470-3bb2d5f389cc", -999, -999, 5beba896-bccf-4993-8bcb-1cdabb68e706", "a0c5b949-22a4-4899-9c51-64ccce6956bc", "a0cbac2e-f86d-4ab0-a090-8115f5196f7c", "abe572e9-126b-43eb-bf5c-4edf9ec365c1", "b0cae640-b155-4bac-9ed5-29ba64a1ee9f", "cd004d89-f798-40d0-bf65-ee4c1c48700c", "d8efe522-5355-48b8-b4fb-4d077bbc621d", "da7341c3-e6a3-4cd3-ad57-49a4dc732ac9", "db79ee0b-65e9-4ea1-8b8b-948bbae128f7", "e3fd373d-6ecc-497a-b396-816f3cb5f9b6", "e40daa31-8eb8-463a-b91a-d3aedb631744", "f0a54d9a-7717-473f-8450-5ff24824ed7e);
		public static readonly VanillaFlora BLOOD_KELP = new VanillaFlora(6, 0.1, "1c28891f-df08-4eee-a081-118955b0d303", "461487ff-aea5-426e-b473-a378dca662b9", "66f2188b-b537-49ac-b6e7-08f446eca9e8", "8c4ba581-e392-41ab-80a9-a4a2745dcfdb", "a4912ba2-5643-46ee-bd69-6be53dd55d45", "d0811984-35bb-435f-acad-3abcf4fb5d32", "d69d04e9-bef6-4229-9bea-a76378cb0018", "e0ae8532-a6d5-436f-bdc0-846061d91686").markResources().addBiome(VanillaBiomes.BLOODKELPNORTH).addBiome(VanillaBiomes.BLOODKELP);
		
		public static readonly VanillaFlora COVE_TREE = new VanillaFlora("0e7cc3b9-cdf2-42d9-9c1f-c11b94277c19", 5, 4).addBiome(VanillaBiomes.COVE);
		
		public static readonly VanillaFlora CLAW_KELP = new VanillaFlora(6, 1, "04d69bba-6c65-414d-bdaf-cc9b53fb9f3b", "1fd81ec0-16be-4667-a818-0ebfcc74170b", "b628d104-dcad-4fac-8a12-d0c4ef473d93").addBiome(VanillaBiomes.LOSTRIVER); //blue-tipped lost river 
		public static readonly VanillaFlora BLOOD_GRASS = new VanillaFlora("ae210dd4-68f0-4c77-9025-ef7d116948b3", 6).addBiome(VanillaBiomes.REDGRASS);
		public static readonly VanillaFlora STINGERS = new VanillaFlora(9, "46d0473e-d366-4644-8c9c-5fdb65cbacb8", "7935a15e-a9ab-4fc6-90ef-58a65b30a4bd", "8914acde-168e-438f-9b2b-6b9332d8c1a1").addBiome(VanillaBiomes.KELP, true).addBiome(VanillaBiomes.REDGRASS, true).addBiome(VanillaBiomes.JELLYSHROOM);
		public static readonly VanillaFlora GRUE_CLUSTER = new VanillaFlora(6, "71498905-2ce2-4622-8d6f-40212f6202df", "6f5c4850-b8bd-461a-999d-1c49d69ffe3a", "94d7ed83-abb8-49af-9f27-10771dcd1485", "82287160-87eb-4fdd-ae33-945ba666ae60").addBiome(VanillaBiomes.MUSHROOM).addBiome(VanillaBiomes.TREADER, true);
		public static readonly VanillaFlora MUSHROOM_BUMP = new VanillaFlora(0, "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5", "961194a9-e88b-40d7-900d-a48c5b739352", "a1f8e7cf-83ae-438c-9197-3321374eca56", "f3de21af-550b-4901-a6e8-e45e31c1509d", "fe145621-5b25-4000-a3dd-74c1aaa961e2").addBiome(VanillaBiomes.MUSHROOM);
		
		public static readonly VanillaFlora TABLECORAL_ORANGE = new VanillaFlora(0, "f0713f3d-586b-4c71-88a3-18dd6c3dd2a4", "9a643563-9278-4c77-8bd2-f9b4b1a1053a", "70eb6270-bf5e-4d6a-8182-484ffcfd8de6", "4e31161e-c812-4c8c-bfd4-00cf4b743884").addBiome(VanillaBiomes.SHALLOWS);
		public static readonly VanillaFlora TABLECORAL_BLUE = new VanillaFlora(0, "e0608e57-e9df-4f43-bb3a-8c56a42d2c1f", "48d6184a-320e-41d2-abca-5b96a94e72e0", "3d4d3892-e43a-45b1-85b8-4a6462257c79", "1edd7411-8f1d-4e7a-8378-0ce7ccb6ea82").addBiome(VanillaBiomes.MUSHROOM);
		public static readonly VanillaFlora TABLECORAL_GREEN = new VanillaFlora(0, "34b59c1d-876e-4962-a8f7-e205d189d2be", "0719b0fa-95df-4b37-a581-4f1e07424c62", "1f384257-9d4a-4307-829f-024c0e1ce1c0", "28fb4ab7-e1eb-4de3-89a9-98f54394e0f6").addBiome(VanillaBiomes.LOSTRIVER);
		public static readonly VanillaFlora TABLECORAL_PURPLE = new VanillaFlora(0, "eb6634e5-3a58-4a0d-ae4e-b673e1fa51ea", "df03263c-ebfb-4e7c-b002-1ec3d67c1215", "c197a6ca-f910-43db-92ab-2e35e423a6f1", "2d970c98-6f77-4270-8be2-91dc863d15d5").addBiome(VanillaBiomes.GRANDREEF).addBiome(VanillaBiomes.DEEPGRAND);
	
		private static readonly Dictionary<string, VanillaFlora> names = new Dictionary<string, VanillaFlora>();
		
		public static VanillaFlora getByName(string n) {
			populateNames();
			return names.ContainsKey(n) ? names[n] : null;
		}
		
		public static List<VanillaFlora> getAll() {
			populateNames();
			return new List<VanillaFlora>(names.Values);
		}
		
		private static void populateNames() {
			if (names.Count == 0) {
				foreach (FieldInfo f in typeof(VanillaFlora).GetFields()) {
					if (f.IsStatic && f.FieldType == typeof(VanillaFlora)) {
						VanillaFlora vf = (VanillaFlora)f.GetValue(null);
						names[f.Name] = vf;
						vf.name = f.Name;
					}
				}
			}
		}
		
		private readonly List<string> prefabs = new List<string>();
		private readonly List<string> prefabsLit = new List<string>();
		public readonly double baseOffset; //amount needed to rise to only just embed, always > 0
		public readonly double maximumSink; //further sinkability from @ vineBaseOffset, always > 0
		
		private readonly List<BiomeBase> biomeSetSurface = new List<BiomeBase>();
		private readonly List<BiomeBase> biomeSetCave = new List<BiomeBase>();
		
		public bool isHarvestable {get; private set;}
		public bool hasResources {get; private set;}
		public bool isEdible {get; private set;}
		
		private string name;
		
		private VanillaFlora(string id, double y, double ym) : this(y, ym, id) {
			
		}
		
		private VanillaFlora(string id, double y) : this(y, 0, id) {
			
		}
		
		private VanillaFlora(double y, params string[] ids) : this(y, 0, ids) {
			
		}
				
		private VanillaFlora(double y, double ym, params string[] ids) {
			foreach (string id in ids) {
				string path = PrefabData.getPrefab(id);
				if (path.Contains("Coral_reef_Light")) {
					prefabsLit.Add(id);
				}
				else {
					prefabs.Add(id);
				}
				lookup[id] = this;
			}
			baseOffset = y;
			maximumSink = ym;
		}
		
		public string getName() {
			if (name == null)
				populateNames();
			return name;
		}
		
		private VanillaFlora markHarvestable() {
			isHarvestable = true;
			return this;
		}
		
		private VanillaFlora markResources() {
			markHarvestable();
			hasResources = true;
			return this;
		}
		
		private VanillaFlora markEdible() {
			markResources();
			isEdible = true;
			return this;
		}
		
		public VanillaFlora addBiome(BiomeBase biome, bool caveOnly = false) {
			biomeSetCave.Add(biome);
			if (!caveOnly)
				biomeSetSurface.Add(biome);
			return this;
		}
		
		public string getRandomPrefab(bool preferLit) {
			List<string> check = preferLit ? prefabsLit : prefabs;
			if (check.Count == 0) {
				check = !preferLit ? prefabsLit : prefabs;
			}
			if (check.Count == 0)
				throw new Exception("No prefabs!");
			return check[UnityEngine.Random.Range(0, check.Count)];
		}
		
		public int litCount() {
			return prefabsLit.Count;
		}
		
		public int unlitCount() {
			return prefabs.Count;
		}
		
		public IEnumerable<string> getPrefabs(bool lit, bool unlit) {
			List<string> li = new List<string>();
			if (unlit || prefabsLit.Count == 0)
				li.AddRange(prefabs);
			if (lit || prefabs.Count == 0)
				li.AddRange(prefabsLit);
			return li;
		}
		
		public bool includes(string pfb) {
			return prefabsLit.Contains(pfb) || prefabs.Contains(pfb);
		}
		
		public string getPrefabID() {
			return getRandomPrefab(false);
		}
		
		public static VanillaFlora getFromID(string pfb) {
			return lookup.ContainsKey(pfb) ? lookup[pfb] : null;
		}
		
		public override string ToString() {
			return getName();
		}
		
		public bool isNativeToBiome(Vector3 pos) {
			return isNativeToBiome(BiomeBase.getBiome(pos), WorldUtil.isInCave(pos));
		}
		
		public bool isNativeToBiome(BiomeBase b, bool cave) {
			return (cave ? biomeSetCave : biomeSetSurface).Contains(b);
		}
	}
}
