using System;
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
				
			}

			Console.ReadLine();
		}
	}
}
