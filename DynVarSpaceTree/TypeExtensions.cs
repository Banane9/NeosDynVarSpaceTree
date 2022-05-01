using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynVarSpaceTree
{
    internal static class TypeExtensions
    {
        public static void AppendTypeName(this StringBuilder builder, Type type)
        {
            if (!type.IsGenericType)
            {
                builder.Append(type.Name);
                return;
            }

            builder.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
            builder.Append('<');

            var appendComma = false;
            foreach (var arg in type.GetGenericArguments())
            {
                if (appendComma)
                    builder.Append(", ");

                builder.AppendTypeName(arg);
                appendComma = true;
            }

            builder.Append('>');
        }
    }
}