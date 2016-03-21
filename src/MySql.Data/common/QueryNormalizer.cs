// Copyright (c) 2009-2010 Sun Microsystems, Inc.
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;


namespace MySql.Data.Common
{
  internal class QueryNormalizer
  {
    private static List<string> keywords = new List<string>();
    private List<Token> tokens = new List<Token>();
    private int pos;
    private string fullSql;
    private string queryType;

    static QueryNormalizer()
    {
      StringReader sr = new StringReader("keywords.txt;System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089;Windows-1252");
      string keyword = sr.ReadLine();
      while (keyword != null)
      {
        keywords.Add(keyword);
        keyword = sr.ReadLine();
      }
    }

    public string QueryType
    {
      get { return queryType; }
    }

    public string Normalize(string sql)
    {
      tokens.Clear();
      StringBuilder newSql = new StringBuilder();
      fullSql = sql;

      TokenizeSql(sql);
      DetermineStatementType(tokens);
      ProcessMathSymbols(tokens);
      CollapseValueLists(tokens);
      CollapseInLists(tokens);
      CollapseWhitespace(tokens);

      foreach (Token t in tokens)
        if (t.Output)
          newSql.Append(t.Text);

      return newSql.ToString();
    }

    private void DetermineStatementType(List<Token> tok)
    {
      foreach (Token t in tok)
      {
        if (t.Type == TokenType.Keyword)
        {
          queryType = t.Text.ToUpperInvariant();
          //string s = t.Text.ToLowerInvariant();
          //if (s == "select")
          //    queryType = "SELECT";
          //else if (s == "update" || s == "insert")
          //    queryType = "UPSERT";
          //else
          //    queryType = "OTHER";
          break;
        }
      }
    }

    /// <summary>
    /// Mark - or + signs that are unary ops as no output
    /// </summary>
    /// <param name="tok"></param>
    private void ProcessMathSymbols(List<Token> tok)
    {
      Token lastToken = null;

      foreach (Token t in tok)
      {
        if (t.Type == TokenType.Symbol &&
            (t.Text == "-" || t.Text == "+"))
        {
          if (lastToken != null &&
              lastToken.Type != TokenType.Number &&
              lastToken.Type != TokenType.Identifier &&
              (lastToken.Type != TokenType.Symbol || lastToken.Text != ")"))
            t.Output = false;
        }
        if (t.IsRealToken)
          lastToken = t;
      }
    }

    private void CollapseWhitespace(List<Token> tok)
    {
      Token lastToken = null;

      foreach (Token t in tok)
      {
        if (t.Output &&
            t.Type == TokenType.Whitespace &&
            lastToken != null &&
            lastToken.Type == TokenType.Whitespace)
        {
          t.Output = false;
        }
        if (t.Output)
          lastToken = t;
      }
    }

    private void CollapseValueLists(List<Token> tok)
    {
      int pos = -1;
      while (++pos < tok.Count)
      {
        Token t = tok[pos];
        if (t.Type != TokenType.Keyword) continue;
        if (!t.Text.StartsWith("VALUE", StringComparison.OrdinalIgnoreCase)) continue;
        CollapseValueList(tok, ref pos);
      }
    }

    private void CollapseValueList(List<Token> tok, ref int pos)
    {
      List<int> parenIndices = new List<int>();

      // this while loop will find all closing parens in this value list
      while (true)
      {
        // find the close ')'
        while (++pos < tok.Count)
        {
          if (tok[pos].Type == TokenType.Symbol && tok[pos].Text == ")")
            break;
          if (pos == tok.Count - 1)
            break;
        }
        parenIndices.Add(pos);

        // now find the next "real" token
        while (++pos < tok.Count)
          if (tok[pos].IsRealToken) break;
        if (pos == tok.Count) break;

        if (tok[pos].Text != ",")
        {
          pos--;
          break;
        }
      }

      // if we only have 1 value then we don't collapse
      if (parenIndices.Count < 2) return;
      int index = parenIndices[0];
      tok[++index] = new Token(TokenType.Whitespace, " ");
      tok[++index] = new Token(TokenType.Comment, "/* , ... */");
      index++;

      // now mark all the other tokens as no output
      while (index <= parenIndices[parenIndices.Count - 1])
        tok[index++].Output = false;
    }

    private void CollapseInLists(List<Token> tok)
    {
      int pos = -1;
      while (++pos < tok.Count)
      {
        Token t = tok[pos];
        if (t.Type != TokenType.Keyword) continue;
        if (!(t.Text == "IN")) continue;
        CollapseInList(tok, ref pos);
      }
    }

    private Token GetNextRealToken(List<Token> tok, ref int pos)
    {
      while (++pos < tok.Count)
      {
        if (tok[pos].IsRealToken) return tok[pos];
      }
      return null;
    }

    private void CollapseInList(List<Token> tok, ref int pos)
    {
      Token t = GetNextRealToken(tok, ref pos);
      // Debug.Assert(t.Text == "(");
      if (t == null)
        return;

      // if the first token is a keyword then we likely have a 
      // SELECT .. IN (SELECT ...)
      t = GetNextRealToken(tok, ref pos);
      if (t == null || t.Type == TokenType.Keyword) return;

      int start = pos;
      // first find all the tokens that make up the in list
      while (++pos < tok.Count)
      {
        t = tok[pos];
        if (t.Type == TokenType.CommandComment) return;
        if (!t.IsRealToken) continue;
        if (t.Text == "(") return;
        if (t.Text == ")") break;
      }
      int stop = pos;

      for (int i = stop; i > start; i--)
        tok.RemoveAt(i);
      tok.Insert(++start, new Token(TokenType.Whitespace, " "));
      tok.Insert(++start, new Token(TokenType.Comment, "/* , ... */"));
      tok.Insert(++start, new Token(TokenType.Whitespace, " "));
      tok.Insert(++start, new Token(TokenType.Symbol, ")"));
    }

    private void TokenizeSql(string sql)
    {
      pos = 0;

      while (pos < sql.Length)
      {
        char c = sql[pos];
        if (LetterStartsComment(c) && ConsumeComment())
          continue;
        if (Char.IsWhiteSpace(c))
          ConsumeWhitespace();
        else if (c == '\'' || c == '\"' || c == '`')
          ConsumeQuotedToken(c);
        else if (!IsSpecialCharacter(c))
          ConsumeUnquotedToken();
        else
          ConsumeSymbol();
      }
    }

    private bool LetterStartsComment(char c)
    {
      return c == '#' || c == '/' || c == '-';
    }

    private bool IsSpecialCharacter(char c)
    {
      if (Char.IsLetterOrDigit(c) ||
          c == '$' || c == '_' || c == '.') return false;
      return true;
    }

    private bool ConsumeComment()
    {
      char c = fullSql[pos];
      // make sure the comment starts correctly
      if (c == '/' && ((pos + 1) >= fullSql.Length || fullSql[pos + 1] != '*')) return false;
      if (c == '-' && ((pos + 2) >= fullSql.Length || fullSql[pos + 1] != '-' || fullSql[pos + 2] != ' ')) return false;

      string endingPattern = "\n";
      if (c == '/')
        endingPattern = "*/";

      int startingIndex = pos;

      int index = fullSql.IndexOf(endingPattern, pos);
      if (index == -1)
        index = fullSql.Length - 1;
      else
        index += endingPattern.Length;
      string comment = fullSql.Substring(pos, index - pos);
      if (comment.StartsWith("/*!", StringComparison.Ordinal))
        tokens.Add(new Token(TokenType.CommandComment, comment));
      pos = index;
      return true;
    }

    private void ConsumeSymbol()
    {
      char c = fullSql[pos++];
      tokens.Add(new Token(TokenType.Symbol, c.ToString()));
    }

    private void ConsumeQuotedToken(char c)
    {
      bool escaped = false;
      int start = pos;
      pos++;
      while (pos < fullSql.Length)
      {
        char x = fullSql[pos];

        if (x == c && !escaped) break;

        if (escaped)
          escaped = false;
        else if (x == '\\')
          escaped = true;
        pos++;
      }
      pos++;
      if (c == '\'')
        tokens.Add(new Token(TokenType.String, "?"));
      else
        tokens.Add(new Token(TokenType.Identifier, fullSql.Substring(start, pos - start)));
    }

    private void ConsumeUnquotedToken()
    {
      int startPos = pos;
      while (pos < fullSql.Length && !IsSpecialCharacter(fullSql[pos]))
        pos++;
      string word = fullSql.Substring(startPos, pos - startPos);
      double v;
      if (Double.TryParse(word, out v))
        tokens.Add(new Token(TokenType.Number, "?"));
      else
      {
        Token t = new Token(TokenType.Identifier, word);
        if (IsKeyword(word))
        {
          t.Type = TokenType.Keyword;
          t.Text = t.Text.ToUpperInvariant();
        }
        tokens.Add(t);
      }
    }

    private void ConsumeWhitespace()
    {
      tokens.Add(new Token(TokenType.Whitespace, " "));
      while (pos < fullSql.Length && Char.IsWhiteSpace(fullSql[pos]))
        pos++;
    }

    private bool IsKeyword(string word)
    {
      return keywords.Contains(word.ToUpperInvariant());
    }
  }

  internal class Token
  {
    public TokenType Type;
    public string Text;
    public bool Output;

    public Token(TokenType type, string text)
    {
      Type = type;
      Text = text;
      Output = true;
    }

    public bool IsRealToken
    {
      get
      {
        return Type != TokenType.Comment &&
               Type != TokenType.CommandComment &&
               Type != TokenType.Whitespace &&
               Output;
      }
    }
  }

  internal enum TokenType
  {
    Keyword,
    String,
    Number,
    Symbol,
    Identifier,
    Comment,
    CommandComment,
    Whitespace
  }
}