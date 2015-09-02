using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using RestSharp;

namespace Moodle
{
	class Program
	{

		static void AskUser(string output, bool isNewline)
		{
			if (!isNewline)
			{
				Program.ConsoleSetColor(ConsoleColor.Cyan);
				Console.Write(output);
				Program.ConsoleSetColor(ConsoleColor.White);
				return;
			}
			Program.ConsoleSetColor(ConsoleColor.Cyan);
			Console.WriteLine(output);
			Program.ConsoleSetColor(ConsoleColor.White);
		}

		static void ConsoleSetColor(ConsoleColor consoleColor)
		{
			Console.ForegroundColor = consoleColor;
		}

		static void DownloadAllFilesLocal(Dictionary<string, string> downloadList, string chosenFilePath, string chosenFolderName, IRestResponse loginResponse)
		{
			string filePath;
			Console.WriteLine("");
			if (chosenFolderName == string.Empty)
			{
				filePath = string.Concat(chosenFilePath, "\\");
			}
			else
			{
				Directory.CreateDirectory(string.Concat(chosenFilePath, "\\", chosenFolderName));
				filePath = string.Concat(chosenFilePath, "\\", chosenFolderName, "\\");
			}
			foreach (KeyValuePair<string, string> keyValuePair in downloadList)
			{
				string tempFilePath = filePath;
				RestClient client = new RestClient(keyValuePair.Value);
				RestRequest request = new RestRequest(Method.GET);
				request.AddCookie(loginResponse.Cookies[0].Name, loginResponse.Cookies[0].Value);
				Uri uri = client.Execute(request).ResponseUri;
				tempFilePath = string.Concat(tempFilePath, uri.Segments.Last<string>());
				WebClient webClient = new WebClient();
				webClient.Headers.Add(HttpRequestHeader.Cookie, string.Concat(loginResponse.Cookies[0].Name, "=", loginResponse.Cookies[0].Value));
				webClient.DownloadFile(uri, tempFilePath);
				Program.ConsoleSetColor(ConsoleColor.Gray);
				Console.WriteLine(string.Concat(uri.Segments.Last<string>(), " complete"));
			}
		}

		static Dictionary<string, string> GetDownloadUrl(IRestResponse loginResponse, Dictionary<string, string> courseDictionary, int courseKeyValuePairIndex)
		{
			Dictionary<string, string> urlList = new Dictionary<string, string>();
			KeyValuePair<string, string> keyValuePair = courseDictionary.ElementAt<KeyValuePair<string, string>>(courseKeyValuePairIndex);
			RestClient client = new RestClient(keyValuePair.Value);
			RestRequest request = new RestRequest(Method.GET);
			request.AddCookie(loginResponse.Cookies[0].Name, loginResponse.Cookies[0].Value);
			IRestResponse response = client.Execute(request);
			HtmlDocument document = new HtmlDocument();
			document.Load(Utils.GenerateStreamFromString(response.Content));
			foreach (HtmlNode htmlNode in (IEnumerable<HtmlNode>)document.DocumentNode.SelectNodes("//*[@class=\"activityinstance\"]"))
			{
				if (urlList.ContainsKey(htmlNode.InnerText))
				{
					continue;
				}
				urlList.Add(htmlNode.InnerText, htmlNode.ChildNodes[0].GetAttributeValue("href", ""));
			}
			return urlList;
		}

		static IRestResponse LoginToMoodle(string userName, string password)
		{
			RestClient client = new RestClient("https://courses.iitm.ac.in/login/index.php")
			{
				CookieContainer = new CookieContainer()
			};
			RestRequest request = new RestRequest(Method.POST);
			request.AddParameter("username", userName, ParameterType.GetOrPost);
			request.AddParameter("password", password, ParameterType.GetOrPost);
			request.AddHeader("HTTPonly", "true");
			IRestResponse response = client.Execute(request);
			string xCookie = client.CookieContainer.GetCookieHeader(new Uri("http://courses.iitm.ac.in"));
			string[] parsedStrings = xCookie.Split(new char[] { '=' });
			IList<RestResponseCookie> cookies = response.Cookies;
			RestResponseCookie restResponseCookie = new RestResponseCookie()
			{
				Name = parsedStrings[0],
				Value = parsedStrings[1]
			};
			cookies.Add(restResponseCookie);
			return response;
		}

		static void Main(string[] args)
		{
			IRestResponse loginResponse;
			int chosenCourseIndex;
			do
			{
				Program.AskUser("\nRoll No  : ", false);
				string userName = Console.ReadLine();
				Program.AskUser("Password : ", false);
				string password = Utils.SecureStringToString(Utils.GetPassword());
				Program.ConsoleSetColor(ConsoleColor.Cyan);
				loginResponse = Program.LoginToMoodle(userName, password);
			}
			while (loginResponse.Content.Contains("Invalid login, please try again"));
			Console.WriteLine("\n");
			HtmlDocument homeDocument = new HtmlDocument();
			homeDocument.Load(Utils.GenerateStreamFromString(loginResponse.Content));
			Dictionary<string, string> courseDictionary = Program.ParseCourses(homeDocument);
			int i = 0;
			foreach (KeyValuePair<string, string> keyValuePair in courseDictionary)
			{
				Program.ConsoleSetColor(ConsoleColor.Gray);
				Console.Write(i + 1);
				Console.Write(") ");
				Program.ConsoleSetColor(ConsoleColor.White);
				Console.WriteLine(keyValuePair.Key);
				i++;
			}
			Program.AskUser("\nChoose a course for bulk downloading course contents -> ", false);
			int.TryParse(Console.ReadLine(), out chosenCourseIndex);
			chosenCourseIndex--;
			Program.ConsoleSetColor(ConsoleColor.Cyan);
			KeyValuePair<string, string> keyValuePair1 = courseDictionary.ElementAt<KeyValuePair<string, string>>(chosenCourseIndex);
			Program.AskUser(string.Concat("You have chosen ", keyValuePair1.Key), true);
			Console.WriteLine("");
			Program.AskUser("Enter the folder path for download -> ", false);
			string chosenFilePath = Console.ReadLine();
			Program.AskUser("Enter the folder name for download -> ", false);
			string chosenFolderName = Console.ReadLine();
			Dictionary<string, string> downloadList = Program.GetDownloadUrl(loginResponse, courseDictionary, chosenCourseIndex);
			Program.DownloadAllFilesLocal(downloadList, chosenFilePath, chosenFolderName, loginResponse);
		}

		static Dictionary<string, string> ParseCourses(HtmlDocument htmlDocument)
		{
			Dictionary<string, string> courseList = new Dictionary<string, string>();
			HtmlNodeCollection registeredCoursesCollection = htmlDocument.DocumentNode.SelectNodes("//*[@class=\"courses frontpage-course-list-enrolled\"]");
			HtmlDocument document = new HtmlDocument();
			document.Load(Utils.GenerateStreamFromString(registeredCoursesCollection[0].InnerHtml));
			foreach (HtmlNode htmlNode in document.DocumentNode.SelectNodes ("//*[@class=\"coursename\"]"))
			{
				if (courseList.ContainsKey (htmlNode.InnerText))
				{
					continue;
				}
				courseList.Add (htmlNode.InnerText, htmlNode.ChildNodes [0].GetAttributeValue ("href", ""));
			}
			return courseList;
		}
	}
}