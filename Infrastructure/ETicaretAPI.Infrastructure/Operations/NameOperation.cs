﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Infrastructure.Operations
{
    public static class NameOperation
    {
        public static string CharacterRegulatory(string name)
            => name.Replace("é", "")
                .Replace("<", "")
                .Replace("!", "")
                .Replace(">", "")
                .Replace("'", "")
                .Replace("£", "")
                .Replace("^", "")
                .Replace("^^", "")
                .Replace("#", "")
                .Replace("+", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("½", "")
                .Replace("&", "")
                .Replace("/", "")
                .Replace("{", "")
                .Replace("(", "")
                .Replace("[", "")
                .Replace(")", "")
                .Replace("]", "")
                .Replace("=", "")
                .Replace("}", "")
                .Replace("?", "")
                .Replace("*", "")
                .Replace("\"", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace("|", "")
                .Replace("||", "")
                .Replace("@", "")
                .Replace("€", "")
                .Replace("¨", "")
                .Replace("¨¨", "")
                .Replace("~", "")
                .Replace("~~", "")
                .Replace(",", "")
                .Replace("`", "")
                .Replace("``", "")
                .Replace(";", "")
                .Replace("æ", "")
                .Replace("ß;", "")
                .Replace("´", "")
                .Replace("´´", "")
                .Replace(":", "")
                .Replace(".", "-")
                .Replace("Ğ", "g")
                .Replace("ğ", "g")
                .Replace("Ü", "u")
                .Replace("ü", "u")
                .Replace("Ş", "s")
                .Replace("ş", "s")
                .Replace("İ", "i")
                .Replace("ı", "i")
                .Replace("Ç", "c")
                .Replace("ç", "c")
                .Replace("Ö", "o")
                .Replace("ö", "o");
    }
}