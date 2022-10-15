using System.Drawing;

namespace PhishingPortal.UI.Blazor.Helpers
{
    public class ColorHelper
    {
        public static string[] GetBackgroundColors(Color color, int arraySize)
        {
            var colors = new List<string>();
            for (var i = 0; i < arraySize; i++)
                colors.Add(color.Name.ToString().ToLower());
            return colors.ToArray();
        }
    }
}
