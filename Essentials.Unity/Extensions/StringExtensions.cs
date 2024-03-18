using System;
using System.Collections.Generic;

namespace Database.Essentials.Unity.Extensions
{
    public static class StringExtensions
    {
        private const int EscapeCharacter = '\\';

        /// <summary>
        /// Returns the number of times the character cppears in the string.
        /// </summary>
        public static int CountCharacter(this ArraySegment<char> str, params char[] character)
        {
            int count = 0;
            int last = -1;
            for (int i = 0; i < str.Count; i++)
            {
                for (int j = 0; j < character.Length; j++)
                {
                    if (str[i] == character[j])
                    {
                        if (last != i - 1)
                            count++;
                        last = i;
                        break;
                    }
                }
            }
            return count + (last == str.Count - 1 ? 0 : 1);
        }

        /// <summary>
        /// Returns the number of times the character appears in the string, not counting escape characters
        /// </summary>
        public static int CountCharacterNonEscape(this ArraySegment<char> str, char character)
        {
            int count = 0;
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i] == EscapeCharacter)
                {
                    i++;
                    continue;
                }
                count += str[i] == character ? 1 : 0;
            }
            return count;
        }

        /// <summary>
        /// Splits the string by the given character
        /// </summary>
        public static ArraySegment<char>[] SplitToSegments(this ArraySegment<char> str, params char[] characters)
        {
            var rows = CountCharacter(str, characters);
            var result = new ArraySegment<char>[rows];
            int index = 0;

            for (int i = 0; i < str.Count; i++)
            {
                for (int j = 0; j < characters.Length; j++)
                {
                    if (str[i] == characters[j])
                    {
                        if (i != 0)
                        {
                            result[index] = str[..i];
                            index++;
                        }
                        str = str[(i + 1)..];
                        i = -1;
                        break;
                    }
                }
            }

            if (str.Count > 0)
            {
                result[index] = str;
            }

            return result;
        }

        /// <summary>
        /// Splits the string by the given character, not counting escape characters and occurrences in-between quotations (' and ")
        /// </summary>
        public static List<ArraySegment<char>> SplitToSegmentsNonEscapeNonQuote(this ArraySegment<char> str,
            int initialListCapacity = 32,
            params char[] characters)
        {
            var result = new List<ArraySegment<char>>(initialListCapacity);
            char lastQuoteChar = default;

            for (int i = 0; i < str.Count; i++)
            {
                if (str[i] == EscapeCharacter)
                {
                    i++;
                    continue;
                }
                if (lastQuoteChar != default)
                {
                    if (str[i] == lastQuoteChar)
                    {
                        lastQuoteChar = default;
                    }
                    continue;
                }
                switch (str[i])
                {
                    case '"':
                        lastQuoteChar = str[i];
                        continue;
                    case '\'':
                        lastQuoteChar = str[i];
                        continue;
                }
                for (int j = 0; j < characters.Length; j++)
                {
                    if (str[i] == characters[j])
                    {
                        if (i != 0)
                        {
                            result.Add(str[..i]);
                        }
                        str = str[(i + 1)..];
                        i = -1;
                        break;
                    }
                }
            }

            // leftover characters
            if (str.Count > 0)
            {
                result.Add(str);
            }
            return result;
        }

        public static object ToValue(this ArraySegment<char> str, Type type)
        {
            return Convert.ChangeType(str.ToString(), type);
        }
    }
}