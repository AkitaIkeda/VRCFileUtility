using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRChatAPI.Implementations;
using VRChatAPI.Interfaces;

namespace VRCFileUtility.Utillity
{
	static class TokenUtillity
	{
		public const string defaultPath = @".\AuthToken.xml";

		public static bool TryGetSavedCredential(out ITokenCredential credential, string path = defaultPath)
		{
			try
			{
				using StreamReader textReader = new StreamReader(path ?? defaultPath);
				credential = new XmlSerializer(typeof(TokenCredential)).Deserialize(textReader) as ITokenCredential;
				return !string.IsNullOrEmpty(credential.AuthToken);
			}
			catch
			{
				credential = null;
				return false;
			}
		}
		public static void SaveCredential(ITokenCredential credential, string path = defaultPath)
		{
			if(!string.IsNullOrEmpty(credential.AuthToken))
			{
				using StreamWriter textWriter = new StreamWriter(path ?? defaultPath);
				new XmlSerializer(credential.GetType()).Serialize(textWriter, credential);
			}
		}
	}
}
