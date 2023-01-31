using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APPID
{
    internal class StringTools
    {
        public static string RemoveEverythingAfterFirstKeepString(string s, string removeMe)
        {
            int index = s.IndexOf(removeMe);
            if (index > 0)
                s = s.Substring(0, index);
            s = s + removeMe;
            return s;
        }
        public static string RemoveEverythingAfterFirstRemoveString(string s, string removeMe)
        {
            int index = s.IndexOf(removeMe);
            if (index > 0)
                s = s.Substring(0, index);
            return s;
        }

        public static string RemoveEverythingAfterLastRemoveString(string s, string removeMe)
        {
            int index = s.LastIndexOf(removeMe);
            if (index > 0)
                s = s.Substring(0, index);
            return s;
        }
        public static string RemoveEverythingAfterLastKeepString(string s, string removeMe)
        {
            int index = s.LastIndexOf(removeMe);
            if (index > 0)
                s = s.Substring(0, index);
            s = s + removeMe;
            return s;
        }

        public static string RemoveEverythingBeforeFirstKeepString(string s, string removeMe)
        {
            int index = s.IndexOf(removeMe);
            if (index > 0)
                s = s.Substring(index);
            return s;
        }

        public static string RemoveEverythingBeforeFirstRemoveString(string s, string removeMe)
        {
            int index = s.IndexOf(removeMe);
            int removelength = removeMe.Length;
            if (index > 0)
                s = s.Substring(index + removelength);
            return s;
        }

        public static string KeepOnlyNumbers(string s)
        {
            string numbers = "0123456789";
            string a = "";
            foreach (char ch in s)
            {
                if (numbers.Contains(ch))
                {
                    a += ch;
                }
            }
            return a;
        }

        public static string RemoveEverythingBeforeLastRemoveString(string s, string removeMe)
        {
            int index = s.LastIndexOf(removeMe);
            int removelength = removeMe.Length;
            if (index > 0)
                s = s.Substring(index + removelength);
            return s;
        }
        public static string RemoveEverythingBeforeLastKeepString(string s, string removeMe)
        {
            int index = s.LastIndexOf(removeMe);
            if (index > 0)
                s = s.Substring(index);
            return s;
        }
    }
    }
