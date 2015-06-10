using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace T101Sharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Irc i = new Irc ("188.165.3.31", 6667, true);
		
			i.OnData += (Data d) => {
				switch(d.type){
				case "NOTICE":{
						if(d.target == "Auth" && d.message.Contains("*** Looking")){
							i.Nick("v0-test");
							i.User("v0-test","v0-test");
						}
						break;
					}
				case "PING":{
						i.Pong(d.message);
						break;
					}
				case "376":{
						i.Join("#lobby");
						break;
					}
				}
			};
			i.Connect ();
			i.ReadLoop();
		}
	}
}
