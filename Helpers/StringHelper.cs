using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CrucibleBlog.Helpers
{
	public static class StringHelper //using static to avoid coupling too tightly
	{
		public static string BlogPostSlug(string? title)
		{
			// Remove all accents and make the string lower case
			string? output = RemoveAccents(title).ToLower(); //"use this RemoveAccents to return a string and make it all lowercase"

			//Remove special characters with Regex
			output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");

			//Remove all additional spaces in favor of just 1
			output = Regex.Replace(output, @"\s+", " ");

			//Replace all spaces with hyphen
			output = Regex.Replace(output, @"\s", "-");

			return output;
		}

		private static string RemoveAccents(string? title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				return title!;
			}

			//Convert for Unicode
			title = title.Normalize(NormalizationForm.FormD);

			//Formate unicode/ascii
			char[] chars = title.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();

			//Convert and return to new title
			return new string(chars).Normalize(NormalizationForm.FormC); //turns back into C#
		}
	}
}
