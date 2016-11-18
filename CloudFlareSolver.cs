/**
 *   A tool that allows to Solve CloudFlare JavaScript challenge without the use of any external Library
 *   Copyright (C) 2016  Etor Madiv
 *
 *   CloudFlareSolver is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   CloudFlareSolver is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace CloudFlareSolverClient
{
	public class Program
	{
		public static void Main()
		{
			try
			{
				//Create CloudFlareSolver object and allow up to 5 seconds
				var cfs = new CloudFlareSolver("http://hbeta.net/");
				
				//Solve JavaScript Challenge
				cfs.Solve();
				
				//Send challenge request
				cfs.SendChallengeRequest();
				
				//Create a HttpWebRequest
				HttpWebRequest hwr = cfs.CreateWebRequest("http://hbeta.net/");
				
				using(var hwResponse = hwr.GetResponse())
				{
					using(var stream = hwResponse.GetResponseStream())
					{
						using(var reader = new StreamReader(stream))
						{
							File.WriteAllText("hbeta.html", reader.ReadToEnd());
						}
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
			
			Console.ReadLine();
		}
	}
	
	public class CloudFlareSolver
	{
		/// <summary>
		/// CloudFlare challenge Page
		/// </summary>
		private string rawPageContent;
		
		/// <summary>
		/// An Index that is used to locate important data
		/// </summary>
		private int    stopBreakingIndex;
		
		/// <summary>
		/// The name of the target JavaScript object
		/// </summary>
		private string objectName;
		
		/// <summary>
		/// The name of the member of the target JavaScript object
		/// </summary>
		private string objectMemberName;
		
		/// <summary>
		/// The concatenation of objectName, '.', and objectMemberName
		/// </summary>
		private string objectCombinedName;
		
		/// <summary>
		/// Holds the value of jschl_vc
		/// </summary>
		private string jschl_vc;
		
		/// <summary>
		/// Holds the value of pass
		/// </summary>
		private string pass;
		
		/// <summary>
		/// Holds the value of jschl_answer
		/// </summary>
		private int    challengeValue;
		
		/// <summary>
		/// Holds the value of the Last Operation (valid operations are '+=', '-=', '*=', and '/=')
		/// </summary>
		private string lastOpeartion;
		
		/// <summary>
		/// A boolean value indicating if the Last Operation is a valid Operation
		/// </summary>
		private bool   isLastOperationUnknown = false;
		
		/// <summary>
		/// The user agent used to perform the request
		/// </summary>
		private const string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36";
		
		/// <summary>
		/// A CookieContainer that holds the Cookies
		/// </summary>
		private CookieContainer cookieContainer;
		
		/// <summary>
		/// Holds the value of the target URL
		/// </summary>
		private string targetUrl;
		
		/// <summary>
		/// Create a CloudFlareSolver object to solve CloudFlare JavaScript Challenge
		/// </summary>
		/// <param name="url"> The target URL that we want to solve its JavaScript Challenge </param>
		public CloudFlareSolver(string url)
		{
			targetUrl = url;
			
			rawPageContent = GetChallengePage(url);
			
			stopBreakingIndex = rawPageContent.IndexOf("s,t,o,p,b,r,e,a,k,i,n,g,f,") + 26;
			
			if(stopBreakingIndex < 26)
			{
				throw new Exception("[!] Can not find magic \"s,t,o,p,b,r,e,a,k,i,n,g,f,\" keyword.");
			}
			
			Console.WriteLine("[+] Magic keyword \"s,t,o,p,b,r,e,a,k,i,n,g,f,\" found.");
			
			string firstExpression = GetExpression(stopBreakingIndex);
			
			challengeValue = ParseObject(firstExpression);
			
			Console.WriteLine("[+] Object parsed successfully, challengeValue = " + challengeValue);
			
			stopBreakingIndex = rawPageContent.IndexOf(objectCombinedName, stopBreakingIndex);
			
			if(stopBreakingIndex < 0)
			{
				throw new Exception("[!] Can not find expression.");
			}
			
			int myIndex    = rawPageContent.IndexOf("name=\"jschl_vc\" value=\"", stopBreakingIndex) + 23;
			
			if(myIndex < 23)
			{
				throw new Exception("[!] Can not find jschl_vc value.");
			}
			
			int myEndIndex = rawPageContent.IndexOf("\"", myIndex);
			
			if(myEndIndex < 0)
			{
				throw new Exception("[!] Can not find jschl_vc value closing quote.");
			}
			
			jschl_vc       = rawPageContent.Substring(myIndex, myEndIndex - myIndex);
			
			myIndex        = rawPageContent.IndexOf("name=\"pass\" value=\"", myEndIndex) + 19;
			
			if(myIndex < 19)
			{
				throw new Exception("[!] Can not find pass value.");
			}
			
			myEndIndex     = rawPageContent.IndexOf("\"", myIndex);
			
			if(myEndIndex < 0)
			{
				throw new Exception("[!] Can not find pass value closing quote.");
			}
			
			pass           = rawPageContent.Substring(myIndex, myEndIndex - myIndex);
			
			Console.WriteLine("[+] Please allow up to 5 seconds...");
			
			Thread.Sleep(5000);
		}
		
		/// <summary>
		/// Performs a HTTP request to get CloudFlare JavaScript Challenge Page
		/// </summary>
		/// <param name="url"> The target URL. </param>
		/// <returns>
		/// A string containing the CloudFlare JavaScript Challenge page HTML body
		/// </returns>
		private string GetChallengePage(string url)
		{
			var hwr       = (HttpWebRequest) WebRequest.Create(url);
			hwr.UserAgent = userAgent;
			
			cookieContainer = new CookieContainer();
			
			hwr.CookieContainer = cookieContainer;
			
			try
			{
				using(var hwResponse = hwr.GetResponse())
				{
					
				}
				return null;
			}
			catch(WebException e)
			{
				using(var hwResponse = e.Response)
				{
					using(var stream = hwResponse.GetResponseStream())
					{
						using(var reader = new StreamReader(stream))
						{
							string s = reader.ReadToEnd();
							return s;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Solve CloudFlare JavaScript Challenge
		/// </summary>
		public void Solve()
		{
			Uri u = new Uri(targetUrl);
			
			int domainLength = u.Host.Length;
			
			while(!isLastOperationUnknown)
			{
				string expression = GetExpression(stopBreakingIndex);
			
				Console.WriteLine("[+] Found expression, expression = \"" + expression + "\"");
				
				PerformOpeartion(ParseExpression(expression));
				
				Console.WriteLine("[+] Expression parsed successfully, challengeValue = \"" + challengeValue + "\"");
			}
			
			challengeValue += domainLength;
			
			Console.WriteLine("[+] Challenge value is: challengeValue = \"" + challengeValue + "\"");
		}
		
		/// <summary>
		/// Parses the Target JavaScript Object to get the inital value
		/// </summary>
		/// <param name="objectExpression"> The target JavaScript object </param>
		/// <returns>
		/// An integer that is representing the inital value of the target member of the JavaScript object
		/// </returns>
		private int ParseObject(string objectExpression)
		{
			int nameEndIndex = objectExpression.IndexOf("=");
			
			if(nameEndIndex < 0)
			{
				throw new Exception("[!] Error: Can not find initialization operator '='.");
			}
			
			objectName = objectExpression.Substring(0, nameEndIndex);
			
			Console.WriteLine("[+] Object name: \"" + objectName + "\"");
			
			int memberNameIndex = objectExpression.IndexOf("\"", nameEndIndex) + 1;
			
			if(memberNameIndex < 1)
			{
				throw new Exception("[!] Error: Can not find opening quote '\"'.");
			}
			
			int memberNameEndIndex = objectExpression.IndexOf("\"", memberNameIndex);
			
			if(memberNameEndIndex < 0)
			{
				throw new Exception("[!] Error: Can not find closing quote '\"'.");
			}
			
			objectMemberName = objectExpression.Substring(memberNameIndex, memberNameEndIndex - memberNameIndex);
			
			Console.WriteLine("[+] Object member name: \"" + objectMemberName + "\"");
			
			objectCombinedName = objectName + "." + objectMemberName;
			
			int initalValueIndex = objectExpression.IndexOf(":", memberNameEndIndex) + 1;
			
			if(initalValueIndex < 1)
			{
				throw new Exception("[!] Error: Can not find colon opeartor ':'.");
			}
			
			int initalValueEndIndex = objectExpression.IndexOf("}", initalValueIndex);
			
			if(initalValueEndIndex < 0)
			{
				throw new Exception("[!] Error: Can not find closing curly bracket '}'.");
			}
			
			return ParseExpression( objectExpression.Substring(initalValueIndex, initalValueEndIndex - initalValueIndex) );
		}
		
		/// <summary>
		/// Gets a JavaScript expression that is modifing the value of the Target Object member
		/// </summary>
		/// <param name="expressionIndex"> An index to the JavaScript expression </param>
		/// <returns>
		/// A string representing the desired JavaScript expression
		/// </returns>
		private string GetExpression(int expressionIndex)
		{
			stopBreakingIndex = rawPageContent.IndexOf(";", expressionIndex) + 1;
			
			return rawPageContent.Substring(expressionIndex, stopBreakingIndex - expressionIndex).Trim();
		}
		
		/// <summary>
		/// Parses a JavaScript expression that is modifing the value of the Target Object member,
		/// and sets the value of the Last Operation
		/// </summary>
		/// <param name="expression"> A JavaScript expression </param>
		/// <returns>
		/// An integer that is representing the operand value
		/// </returns>
		private int ParseExpression(string expression)
		{
			string newExpression = expression.Replace(objectCombinedName, "")
											 .Replace("!+[]", "1")
											 .Replace("!![]", "1")
											 .Replace("+[]", "");
			
			lastOpeartion = newExpression[0] + "" + newExpression[1];
			
			if(newExpression.Contains("("))
			{
				return ParseParenthesisExpression(newExpression);
			}
			
			return ParseUniqueExpression(newExpression);
		}
		
		/// <summary>
		/// Parses a JavaScript expression that is made of Parenthesis
		/// </summary>
		/// <param name="expression"> A JavaScript expression </param>
		/// <returns>
		/// An integer that is representing the value of the Parenthesis expression
		/// </returns>
		private int ParseParenthesisExpression(string expression)
		{
			string[] elements = expression.Split( new string[]{")+("}, StringSplitOptions.None);
			
			int[] numArray = new int[elements.Length];
			
			int currentElement = 0;
			
			string resultString = "";
			
			foreach(string s in elements)
			{
				for(int i = 0; i < s.Length; i++)
				{
					if(s[i] == '1')
					{
						numArray[currentElement] += 1;
					}
				}
				
				resultString += numArray[currentElement];
				
				currentElement++;
			}
			
			return int.Parse(resultString);
		}
		
		/// <summary>
		/// Parses a JavaScript expression that is NOT made of Parenthesis
		/// </summary>
		/// <param name="expression"> A JavaScript expression </param>
		/// <returns>
		/// An integer that is representing the value of the Unique expression
		/// </returns>
		private int ParseUniqueExpression(string expression)
		{			
			int numValue = 0;
			
			for(int i = 0; i < expression.Length; i++)
			{
				if(expression[i] == '1')
				{
					numValue += 1;
				}
			}
			
			return numValue;
		}
		
		/// <summary>
		/// Performs the required calculation according to the Last Operation
		/// </summary>
		/// <param name="value"> The operand value </param>
		private void PerformOpeartion(int value)
		{
			if(lastOpeartion == "-=")
			{
				challengeValue -= value;
			}
			else if(lastOpeartion == "+=")
			{
				challengeValue += value;
			}
			else if(lastOpeartion == "*=")
			{
				challengeValue *= value;
			}
			else if(lastOpeartion == "/=")
			{
				challengeValue /= value;
			}
			else
			{
				isLastOperationUnknown = true;
				Console.WriteLine("[!] Unknown Last Operation");
			}
		}
		
		/// <summary>
		/// Send the request after the challenge solved
		/// </summary>
		public void SendChallengeRequest()
		{
			Uri u = new Uri(targetUrl);
			
			var hwr = (HttpWebRequest) WebRequest.Create(
				string.Format(
					"{0}/cdn-cgi/l/chk_jschl?jschl_vc={1}&pass={2}&jschl_answer={3}",
					u.GetLeftPart(System.UriPartial.Authority),
					jschl_vc, pass, challengeValue
				)
			);
			hwr.UserAgent       = userAgent;
			hwr.CookieContainer = cookieContainer;
			
			using(var hwResponse = hwr.GetResponse())
			{

			}
		}
		
		/// <summary>
		/// Create a HttpWebRequest that can be used after CloudFlare JavaScript challenge solved
		/// The request URL is the default Target URL
		/// </summary>
		/// <returns>
		/// A HttpWebRequest that can be used to access the target
		/// </returns>
		public HttpWebRequest CreateWebRequest()
		{
			var hwr = (HttpWebRequest) WebRequest.Create(targetUrl);
			hwr.UserAgent       = userAgent;
			hwr.CookieContainer = cookieContainer;
			return hwr;
		}
		
		/// <summary>
		/// Create a HttpWebRequest that can be used after CloudFlare JavaScript challenge solved
		/// </summary>
		/// <param name="url"> A URL that has the same Host as the Host where the challenge was solved  </param>
		/// <returns>
		/// A HttpWebRequest that can be used to access the target
		/// </returns>
		public HttpWebRequest CreateWebRequest(string url)
		{
			Uri u1 = new Uri(targetUrl);
			Uri u2 = new Uri(url);
			
			if(u1.Host != u2.Host)
			{
				throw new Exception("[!] Request Hosts mismatch.");
			}
			
			var hwr = (HttpWebRequest) WebRequest.Create(url);
			hwr.UserAgent       = userAgent;
			hwr.CookieContainer = cookieContainer;
			return hwr;
		}
	}
}