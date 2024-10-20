using RLE.Core.Extensions;

namespace RLE.Core.Helpers
{
    public static class EnumHelpers
    {
        public static List<string>? GetListOfDescription<T>() where T : struct
        {
            Type t = typeof(T);
            return !t.IsEnum ? null : Enum.GetValues(t).Cast<Enum>().Select(x => x.GetDescription()).ToList();
        }
    }
}
