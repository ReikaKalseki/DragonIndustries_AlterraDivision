/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra {
	internal class PipeReconnection : ManipulationBase {

		private readonly Vector3 data;

		internal PipeReconnection(Vector3 vec) {
			data = vec;
		}

		public override void applyToObject(GameObject go) {
			go.EnsureComponent<PipeReconnector>().position = data;
		}

		public override void applyToObject(PlacedObject go) {
			this.applyToObject(go.obj);
		}

		public override void loadFromXML(XmlElement e) {

		}

		public override void saveToXML(XmlElement e) {

		}

		public override bool needsReapplication() {
			return true;
		}

	}

	class PipeReconnector : MonoBehaviour {

		IPipeConnection pipe;
		internal Vector3 position;
		IPipeConnection connection;

		void Update() {
			if (pipe == null)
				pipe = gameObject.GetComponent<IPipeConnection>();

			if (connection == null) {
				double dist = 9999;
				List<IPipeConnection> li = new List<IPipeConnection>();
				li.AddRange(UnityEngine.Object.FindObjectsOfType<OxygenPipe>());
				li.AddRange(UnityEngine.Object.FindObjectsOfType<BasePipeConnector>());
				SNUtil.log(string.Join(",", li.Select<IPipeConnection, string>(p => p + " @ " + ((MonoBehaviour)p).transform.position)));
				foreach (IPipeConnection conn in li) {
					Vector3 pos = ((MonoBehaviour)conn).transform.position;
					double dd = Vector3.Distance(pos, position);
					SNUtil.log("Pipe " + gameObject.transform.position + " check against " + pos + " @ dist=" + dd);
					if (connection == null || dd < dist) {
						connection = conn;
						dist = dd;
						SNUtil.log("Reconnected");
					}
				}
				if (connection != null)
					pipe.SetParent(connection);
			}
		}

	}
}
