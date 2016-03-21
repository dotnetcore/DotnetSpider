// Copyright ?2004,2010, Oracle and/or its affiliates.  All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace MySql.Data.MySqlClient
{
  internal class MySqlTokenizer
  {
    private string sql;

    private int startIndex;
    private int stopIndex;

    private bool ansiQuotes;
    private bool backslashEscapes;
    private bool returnComments;
    private bool multiLine;
    private bool sqlServerMode;

    private bool quoted;
    private bool isComment;

    private int pos;

    public MySqlTokenizer()
    {
      backslashEscapes = true;
      multiLine = true;
      pos = 0;
    }

    public MySqlTokenizer(string input)
      : this()
    {
      sql = input;
    }

    #region Properties

    public string Text
    {
      get { return sql; }
      set { sql = value; pos = 0; }
    }

    public bool AnsiQuotes
    {
      get { return ansiQuotes; }
      set { ansiQuotes = value; }
    }

    public bool BackslashEscapes
    {
      get { return backslashEscapes; }
      set { backslashEscapes = value; }
    }

    public bool MultiLine
    {
      get { return multiLine; }
      set { multiLine = value; }
    }

    public bool SqlServerMode
    {
      get { return sqlServerMode; }
      set { sqlServerMode = value; }
    }

    public bool Quoted
    {
      get { return quoted; }
      private set { quoted = value; }
    }

    public bool IsComment
    {
      get { return isComment; }
    }

    public int StartIndex
    {
      get { return startIndex; }
      set { startIndex = value; }
    }

    public int StopIndex
    {
      get { return stopIndex; }
      set { stopIndex = value; }
    }

    public int Position
    {
      get { return pos; }
      set { pos = value; }
    }

    public bool ReturnComments
    {
      get { return returnComments; }
      set { returnComments = value; }
    }

    #endregion

    public List<string> GetAllTokens()
    {
      List<string> tokens = new List<string>();
      string token = NextToken();
      while (token != null)
      {
        tokens.Add(token);
        token = NextToken();
      }
      return tokens;
    }

    public string NextToken()
    {
      while (FindToken())
      {
        string token = sql.Substring(startIndex, stopIndex - startIndex);
        return token;
      }
      return null;
    }

    public static bool IsParameter(string s)
    {
      if (String.IsNullOrEmpty(s)) return false;
      if (s[0] == '?') return true;
      if (s.Length > 1 && s[0] == '@' && s[1] != '@') return true;
      return false;
    }

    public string NextParameter()
    {
      while (FindToken())
      {
        if ((stopIndex - startIndex) < 2) continue;
        char c1 = sql[startIndex];
        char c2 = sql[startIndex + 1];
        if (c1 == '?' ||
            (c1 == '@' && c2 != '@'))
          return sql.Substring(startIndex, stopIndex - startIndex);
      }
      return null;
    }

    public bool FindToken()
    {
      isComment = quoted = false;  // reset our flags
      startIndex = stopIndex = -1;

      while (pos < sql.Length)
      {
        char c = sql[pos++];
        if (Char.IsWhiteSpace(c)) continue;

        if (c == '`' || c == '\'' || c == '"' || (c == '[' && SqlServerMode))
          ReadQuotedToken(c);
        else if (c == '#' || c == '-' || c == '/')
        {
          if (!ReadComment(c))
            ReadSpecialToken();
        }
        else
          ReadUnquotedToken();
        if (startIndex != -1) return true;
      }
      return false;
    }

    public string ReadParenthesis()
    {
      StringBuilder sb = new StringBuilder("(");
      int start = StartIndex;
      string token = NextToken();
      while (true)
      {
        if (token == null)
          throw new InvalidOperationException("Unable to parse SQL");
        sb.Append(token);
        if (token == ")" && !Quoted) break;
        token = NextToken();
      }
      return sb.ToString();
    }

    private bool ReadComment(char c)
    {
      // make sure the comment starts correctly
      if (c == '/' && (pos >= sql.Length || sql[pos] != '*')) return false;
      if (c == '-' && ((pos + 1) >= sql.Length || sql[pos] != '-' || sql[pos + 1] != ' ')) return false;

      string endingPattern = "\n";
      if (sql[pos] == '*')
        endingPattern = "*/";

      int startingIndex = pos - 1;

      int index = sql.IndexOf(endingPattern, pos);
      if (endingPattern == "\n")
        index = sql.IndexOf('\n', pos);
      if (index == -1)
        index = sql.Length - 1;
      else
        index += endingPattern.Length;

      pos = index;
      if (ReturnComments)
      {
        startIndex = startingIndex;
        stopIndex = index;
        isComment = true;
      }
      return true;
    }

    private void CalculatePosition(int start, int stop)
    {
      startIndex = start;
      stopIndex = stop;
      if (!MultiLine) return;
    }

    private void ReadUnquotedToken()
    {
      startIndex = pos - 1;

      if (!IsSpecialCharacter(sql[startIndex]))
      {
        while (pos < sql.Length)
        {
          char c = sql[pos];
          if (Char.IsWhiteSpace(c)) break;
          if (IsSpecialCharacter(c)) break;
          pos++;
        }
      }

      Quoted = false;
      stopIndex = pos;
    }

    private void ReadSpecialToken()
    {
      startIndex = pos - 1;

      Debug.Assert(IsSpecialCharacter(sql[startIndex]));

      stopIndex = pos;
      Quoted = false;
    }

    /// <summary>
    ///  Read a single quoted identifier from the stream
    /// </summary>
    /// <param name="quoteChar"></param>
    /// <returns></returns>
    private void ReadQuotedToken(char quoteChar)
    {
      if (quoteChar == '[')
        quoteChar = ']';
      startIndex = pos - 1;
      bool escaped = false;

      bool found = false;
      while (pos < sql.Length)
      {
        char c = sql[pos];

        if (c == quoteChar && !escaped)
        {
          found = true;
          break;
        }

        if (escaped)
          escaped = false;
        else if (c == '\\' && BackslashEscapes)
          escaped = true;
        pos++;
      }
      if (found) pos++;
      Quoted = found;
      stopIndex = pos;
    }

    private bool IsQuoteChar(char c)
    {
      return c == '`' || c == '\'' || c == '\"';
    }

    private bool IsParameterMarker(char c)
    {
      return c == '@' || c == '?';
    }

    private bool IsSpecialCharacter(char c)
    {
      if (Char.IsLetterOrDigit(c) ||
          c == '$' || c == '_' || c == '.') return false;
      if (IsParameterMarker(c)) return false;
      return true;
    }
  }
}
