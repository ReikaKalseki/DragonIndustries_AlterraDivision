/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 7:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using UnityEngine;
using System.Collections.Generic;
using SMLHelper.V2.Handlers;

namespace ReikaKalseki.DIAlterra
{
	public class CommandHandler : MonoBehaviour
	{
		public static readonly CommandHandler instance = new CommandHandler();
		
		private readonly Dictionary<string, Action<string[]>> callbacks = new Dictionary<string, Action<string[]>>();
		
		private CommandHandler()
		{
			
		}
		
		public void registerCommand(string id, Action<string[]> call) {
			if (callbacks.ContainsKey(id)) {
				throw new Exception("Could not register command: string '"+id+"' is already taken!");
			}
			callbacks[id] = call;
			DevConsole.RegisterConsoleCommand(this, id);
		}
		
		public void OnConsoleCommand_buildprefab(string unknown) {
			SBUtil.log("Received build command with arg '"+unknown+"'");
		}
		
		public bool processCommand(string id, string argsRaw) {
			Action<string[]> call;
			bool flag = callbacks.TryGetValue(id, out call);
			if (flag) {
				call.Invoke(argsRaw.Split(' '));
			}
			return flag;
		}
	}
}
