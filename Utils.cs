using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace Moodle
{
	internal static class Utils
	{
		public static Stream GenerateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = (long)0;
			return stream;
		}

		public static SecureString GetPassword()
		{
			using (SecureString _pwd = new SecureString ())
			{
				while (true)
				{
					ConsoleKeyInfo i = Console.ReadKey (true);
					if (i.Key == ConsoleKey.Enter || i.Key == (ConsoleKey)0)
					{
						break;
					}
					if (i.Key != ConsoleKey.Backspace)
					{
						_pwd.AppendChar (i.KeyChar);
						Console.Write ("*");
					}
					else
						if (_pwd.Length > 0)
						{
							_pwd.RemoveAt (_pwd.Length - 1);
							Console.Write ("\b \b");
						}
				}
				return _pwd;
			}
		}

		public static string SecureStringToString(SecureString value)
		{
			string stringUni;
			IntPtr valuePtr = IntPtr.Zero;
			try
			{
				valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
				stringUni = Marshal.PtrToStringUni(valuePtr);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
			}
			return stringUni;
		}
	}
}