
namespace ReadXml
{
    internal static class Parser
    {
        public static string ParseViewType(long flags)
        {
            if ((flags & 2048) == 2048)
                return "Grid";
            else if ((flags & 524288) == 524288)
                return "Calendar";
            else if ((flags & 67108864) == 67108864)
                return "Gantt";
            else
                return "Html";
        }
        public static string ParseScope(long flags)
        {
            if ((flags & 4096) != 0 && (flags & 2097152) != 0)
                return "Recursive";
            else if ((flags & 4096) != 0 && (flags & 2097152) == 0)
                return "RecursiveAll";
            else if ((flags & 4096) == 0 && (flags & 2097152) != 0)
                return "FilesOnly";
            else
                return "";
        }
    }
}
