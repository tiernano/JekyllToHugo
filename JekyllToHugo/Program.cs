﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace JekyllToHugo
{
	class Program
	{
		static void Main(string[] args)
		{
			string[] markdownFiles =
				Directory.GetFiles(args[0], "*");

			foreach (string s in markdownFiles)
			{
				string content = File.ReadAllText(s);

				int startOfymlBlock = content.IndexOf("---", StringComparison.Ordinal);
				int endOfymlBlock = content.IndexOf("---", startOfymlBlock +1, StringComparison.Ordinal);

				if (startOfymlBlock < 0 || endOfymlBlock < 0)
				{
					Console.WriteLine("Cant find Yaml Block");
					continue;
				}


				string ymlBlock = content.Substring(startOfymlBlock, endOfymlBlock+3);

				string markdown = content.Substring(endOfymlBlock + 3);
			
				var yaml = new YamlStream();
				yaml.Load(new StringReader(ymlBlock));

				var mapping =
					(YamlMappingNode)yaml.Documents[0].RootNode;

				if (!mapping.Children.ContainsKey("slug"))
				{
					string filename = Path.GetFileNameWithoutExtension(s);
					string filenameWithoutDate = filename.Substring(11);

					mapping.Children.Add(new KeyValuePair<YamlNode, YamlNode>("slug", filenameWithoutDate));

					var serializer = new SerializerBuilder().Build();

					string newYml = serializer.Serialize(mapping);

					File.WriteAllText(s, $"---\n{newYml}\n---\n {markdown}");
					Console.WriteLine(s);
				}

				//if you want to replace existing aliases with new ones use the following to remove old ones:

				mapping.Children.Remove("aliases");
				mapping.Children.Remove("disqus_identifier");
				mapping.Children.Remove("disqus_url");

				if (!mapping.Children.ContainsKey("aliases"))
				{

					string filename = Path.GetFileNameWithoutExtension(s);
					string filenameWithoutDate = filename.Substring(11);

					var dateString = mapping.Children["date"].ToString();

					DateTime date;
					if (!DateTime.TryParse(dateString, out date))
					{
						Console.WriteLine("Date not parsable");
						continue;
					}

					string dateFormat = "yyyy/MM/dd";

					string alias = $"{date.ToString(dateFormat)}/{filenameWithoutDate}.html";

					string[] aliases = {
						alias,
						alias.ToLower()
					};

					//mapping.Children.Add(new KeyValuePair<YamlNode, YamlNode>("aliases", aliases));

					mapping.Children.Add(
						new KeyValuePair<YamlNode, YamlNode>("disqus_identifier", $"https://www.tiernanotoole.ie/{alias}"));

					mapping.Children.Add(
						new KeyValuePair<YamlNode, YamlNode>("disqus_url", $"https://www.tiernanotoole.ie/{alias}"));


					var serializer = new SerializerBuilder().Build();

					string newYml = serializer.Serialize(mapping);

					File.WriteAllText(s, $"---\n{newYml}\n---\n {markdown.Trim()}");
					Console.WriteLine(s);
				}
				else
				{
					var aliases = mapping.Children["aliases"];
				}
				
			}

			Console.ReadLine();
		}
	}
}
