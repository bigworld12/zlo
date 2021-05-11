using System.Collections.Generic;
namespace Zlo
{
    public class RunnableGameList : List<RunnableGame>
    {
        public bool IsOSx64 { get; internal set; } = true;
    }

}
