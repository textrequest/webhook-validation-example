using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebhookValidationExample
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 4)
			{
				Console.Error.WriteLine("Missing one or more arguments.");
				Console.WriteLine("Syntax: WebhookValidationExample.exe <inputFilePath> <key> <comparisonSignature> <comparisonTimestamp>");
				return;
			}

			var inputFilePath = args[0];
			var key = args[1];
			var comparisonSignature = args[2];
			var comparisonTimestampText = args[3];

			if (!long.TryParse(comparisonTimestampText, out long comparisonTimestamp))
			{
				Console.Error.WriteLine("<comparisonTimestamp> must be a Unix time expressed as the number of seconds that have elapsed since 1970-01-01T00:00:00Z. Example: Mon Feb 01 2021 13:30:00 GMT+0000 is 1612186200.");
				Console.WriteLine("Syntax: WebhookValidationExample.exe <inputFilePath> <key> <comparisonSignature> <comparisonTimestamp>");
				return;
			}
			var comparisonDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(comparisonTimestamp);

			var inputText = File.ReadAllText(path: inputFilePath);

			Console.WriteLine($"Input Text:");
			Console.WriteLine($"{inputText}");
			Console.WriteLine($"---");
			Console.WriteLine($"Key: {key}");
			Console.WriteLine($"---");

			var signature = EncodeHmacSha256Signature(input: inputText, key: key, timestamp: comparisonTimestamp);

			Console.WriteLine($"Resulting Signature (using comparison timestamp): {signature}");

			Console.WriteLine($"---");
			Console.WriteLine($"Comparison Signature: {comparisonSignature}");
			Console.WriteLine($"Comparison Timestamp: {comparisonTimestampText}");
			Console.WriteLine($"---");

			var signaturesMatch = signature == comparisonSignature;
			Console.WriteLine($"Resulting signature {(signaturesMatch ? "matches" : "does not match")} comparison signature.");

			var currentDateTimeoffset = DateTimeOffset.Now;
			var differenceBetweenComparisonTimeAndCurrentTime = comparisonDateTimeOffset.Subtract(currentDateTimeoffset).Duration().TotalMinutes;
			var comparisonTimeMatchesCurrentTime = differenceBetweenComparisonTimeAndCurrentTime <= 5;

			Console.WriteLine($"Comparison timestamp {(comparisonTimeMatchesCurrentTime ? "is" : "is not")} within 5 minutes of current time. ({differenceBetweenComparisonTimeAndCurrentTime:0.0} minutes)");
		}

		/// <summary>
		/// Returns Base-64 version of HMAC SHA-256 hash of the specified input and timestamp with the specified key.
		/// Encodes the payload as {timestamp}.{input}
		/// </summary>
		/// <param name="input"></param>
		/// <param name="key"></param>
		/// <param name="timestamp">Unix Timestamp in seconds.</param>
		/// <returns></returns>
		public static string EncodeHmacSha256Signature(string input, string key, long timestamp)
		{
			input = input ?? throw new ArgumentNullException(paramName: nameof(input));
			key = key ?? throw new ArgumentNullException(paramName: nameof(key));

			var payload = $"{timestamp}.{input}";

			var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
			var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
			return Convert.ToBase64String(computedHashBytes);
		}
	}
}
