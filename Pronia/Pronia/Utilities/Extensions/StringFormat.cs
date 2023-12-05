namespace Pronia.Utilities.Extensions
{
	public class StringFormat
	{
		private static string Capitalize(string word)
		{
			if (string.IsNullOrEmpty(word))
			{
				return word;
			}

			return char.ToUpper(word[0]) + word.Substring(1).ToLower().Trim();
		}
	}
}
