using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace T101Sharp
{
	public class NickNotes
	{ 
	}

	public class Utils
	{
		public bool Nazi { get; set; }
	}

	public class ServerSettings
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public bool useTLS { get; set; }
		public string Nick { get; set; }
		public List<string> DefaultChans { get; set; }
		public bool Oper { get; set; }
		public List<string> OperDetails { get; set; }
		public NickNotes NickNotes { get; set; }
		public Utils Utils { get; set; }
		public string RawPW { get; set; }
		public string YoutubeApiKey { get; set; }
		public string TwitterAppKey { get; set; }
		public string TwitterAppSecret { get; set; }
		public string TwitterAuthKey { get; set; }
		public string TwitterAuthSecret { get; set; }
		public string TropoToken { get; set; }
		public string TropoTokenMsg { get; set; }
	}
}

