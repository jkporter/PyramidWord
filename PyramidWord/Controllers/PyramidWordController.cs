#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace PyramidWord.Controllers
{
    [ApiController]
    [Route("pyramid-word")]
    // URL Path: /pyramid-word?<pyramid-word>
    // The URL (template) identifies a pyramid word. If the text in the query doesn't match
    // a pyramid word then the resource doesn't exist.
    public class PyramidWordController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            if (!Request.QueryString.HasValue)
                return NotFound("🤷");

            var query = Request.QueryString.Value.Remove(0, 1);

            string s;
            try
            {
                s = Decode(query);
            }
            catch
            {
                return NotFound($"😕 ‘{query}’ is NOT a pyramid word.");
            }

            if (s == null || !IsPyramidWord(s))
                return NotFound($"☹️ ‘{s}’ is NOT a pyramid word.");

            return Ok($"😄 ‘{s}’ is a pyramid word.");
        }

        private static bool IsPyramidWord(string str)
        {
            // Account for Unicode
            str = str.Normalize();
            IEnumerable<string> Characters()
            {
                var textElementEnumerator = StringInfo.GetTextElementEnumerator(str);
                while (textElementEnumerator.MoveNext())
                    yield return (string)textElementEnumerator.Current;
            }

            // Actual pyramid word evaluator
            return IsWord(str) && Characters()
                       .GroupBy(character => character, new CharacterComparer())
                       .Select(groupedCharacters => groupedCharacters.Count())
                       .OrderBy(count => count)
                       // Using own extension that provides and index, instead of a closure
                       .All((count, index) => count == index + 1);
        }

        private class CharacterComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private static readonly Regex IsWordRegex = new Regex(@"\w+");
        private static bool IsWord(string s)
        {
            // Simple test for a word
            return IsWordRegex.IsMatch(s);
        }

        // Since we are not relying on application/x-www-form-urlencoded we have to decode
        // our self. Assumes UTF-8.
        private static string Decode(string s)
        {
            IEnumerable<byte> GetBytes()
            {
                var bytes = Encoding.ASCII.GetBytes(s);
                var buffer = new char[2];
                for (var index = 0; index < bytes.Length; index++)
                {
                    if (bytes[index] == 37)
                    {
                        buffer[0] = (char)bytes[++index];
                        buffer[1] = (char)bytes[++index];
                        yield return (byte)Convert.ToInt32(new string(buffer), 16);
                    }
                    else
                    {
                        yield return bytes[index];
                    }
                }
            }

            return Encoding.UTF8.GetString(GetBytes().ToArray());
        }
    }
}
