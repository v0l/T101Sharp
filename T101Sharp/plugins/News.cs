using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Timers;
using Newtonsoft.Json;

namespace T101Sharp
{
	public class News
	{
		public Irc irc { get; set; }
		public dynamic setting { get; set; }

		private Timer t;
		private int pollTime;

		public void Init(){
			pollTime = 10 * 1000;
			t = new Timer ();
			t.Interval = pollTime;
			t.Elapsed += (sender, e) => {
				try{
					HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=8&q=http%3A%2F%2Fnews.google.com%2Fnews%3Fcf%3Dall%26ned%3Den_ie%26hl%3Den%26topic%3Dn%26output%3Drss");
					using(StreamReader r = new StreamReader(req.GetResponse().GetResponseStream())){
						GoogleNews gn = JsonConvert.DeserializeObject<GoogleNews>(r.ReadToEnd());

						if(gn.responseStatus == 200){
							foreach(Entry ent in gn.responseData.feed.entries){
								DateTime posted = new DateTime();
								DateTime.TryParse(ent.publishedDate,out posted);

								if((DateTime.Now - posted).TotalMilliseconds <= pollTime){
									irc.Notice("#news", string.Format("[ {0} ] - {1}", ent.title, ent.link));
								}
							}
						}
					}
				}catch(Exception ex) { Console.WriteLine(string.Format("News error: {0}", ex)); }
			};
			t.Start ();
		}

		public void OnData(Data d){
			//do nothing
		}

		public string GetHelp(){
			return string.Format ("News feed script");
		}
	}

	internal class Entry
	{
		public string title { get; set; }
		public string link { get; set; }
		public string author { get; set; }
		public string publishedDate { get; set; }
		public string contentSnippet { get; set; }
		public string content { get; set; }
		public List<string> categories { get; set; }
	}

	internal class Feed
	{
		public string feedUrl { get; set; }
		public string title { get; set; }
		public string link { get; set; }
		public string author { get; set; }
		public string description { get; set; }
		public string type { get; set; }
		public List<Entry> entries { get; set; }
	}

	internal class ResponseData
	{
		public Feed feed { get; set; }
	}

	internal class GoogleNews
	{
		public ResponseData responseData { get; set; }
		public object responseDetails { get; set; }
		public int responseStatus { get; set; }
	}
}
