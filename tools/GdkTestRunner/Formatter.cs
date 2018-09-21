using System;

namespace GdkTestRunner
{
    public static class Formatter
    {
        public static string TitleCaseToKebabCase(string titleCase)
        {
            return titleCase.ToLower().Replace(" ", "-");
        }
    }
}
