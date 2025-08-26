using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.DIAlterra {

	public sealed class RichText {

		public readonly string baseText;

		private List<string> parts = new List<string>();

		public RichText(string text) {
			baseText = text;
			parts.Add(baseText);
		}

		public void addBold() {
			this.addFormatPair("b", "b");
		}

		public void addColor(Color c) {
			this.addColor(c.toARGB());
		}

		public void addColor(int c) {
			this.addFormatPair("color=#" + c.ToString("X").ToUpperInvariant(), "color");
		}
		/* not supported
		public void addUnderline() {
			addFormatPair("u", "u");
		}*/

		public void addItalic() {
			this.addFormatPair("i", "i");
		}

		private void addFormatPair(string pre, string post) {
			parts.Insert(0, "<" + pre + ">");
			parts.Add("</" + post + ">");
		}

		public override string ToString() {
			return baseText;
		}

		public string format() {
			return string.Join("", parts);
		}

	}
}
