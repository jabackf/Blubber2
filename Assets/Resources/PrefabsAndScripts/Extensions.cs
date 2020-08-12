using UnityEngine;
using System;
using System.Reflection;

namespace Extensions
{
    public static class MonoBevahiourExtentions
    {
        //Thanks to BenProductions1 for this extension https://answers.unity.com/questions/285988/sendmessage-and-method-overload-dont-get-well-toge.html
        //These extensions can send multiple parameters and can even return a value
        public static object SendMessageEx(this MonoBehaviour obj, string name, params object[] parameters)
        {
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].GetType();
            }

            MethodInfo mInfo = obj.GetType().GetMethod(name, types);

            if (mInfo != null)
            {
                return mInfo.Invoke(obj, parameters);
            }
            return null;
        }
    }

    public static class StringExtensions
    {
        public static bool CaseInsensitiveContains(this string text, string value,StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}