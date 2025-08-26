using System;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public abstract class SpecialDrillable : MonoBehaviour {

		public abstract bool canBeMoved();
		public abstract bool allowAutomatedGrinding();

	}
}
