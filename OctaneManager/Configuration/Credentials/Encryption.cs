/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration.Credentials
{
	public class Encryption
	{
		static readonly string EncryptionPrefix = "AES:";
		static readonly string PasswordHash = "heRWiCTi";
		static readonly string SaltKey = "iNEchoAH";
		static readonly string VIKey = "rATeRISmoretiCKE";

		public static string Encrypt(string plainText)
		{
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

			byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
			var encryptor = GetRijndaelManaged().CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

			byte[] cipherTextBytes;

			using (var memoryStream = new MemoryStream())
			{
				using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
				{
					cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
					cryptoStream.FlushFinalBlock();
					cipherTextBytes = memoryStream.ToArray();
				}
			}
			return EncryptionPrefix + Convert.ToBase64String(cipherTextBytes);
		}

		public static RijndaelManaged GetRijndaelManaged()
		{
			return new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
		}

		public static string Decrypt(string value)
		{
			if (value.StartsWith(EncryptionPrefix))
			{
				string encryptedValue = value.Substring(EncryptionPrefix.Length);
				byte[] cipherTextBytes = Convert.FromBase64String(encryptedValue);
				byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
				var decryptor = GetRijndaelManaged().CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
				using (var memoryStream = new MemoryStream(cipherTextBytes))
				{
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
					{
						byte[] plainTextBytes = new byte[cipherTextBytes.Length];
						int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
						return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
					}
				}
			}
			else
			{
				return value;
			}

		}

	}
}
