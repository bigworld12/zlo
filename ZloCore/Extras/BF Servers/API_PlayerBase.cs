namespace Zlo.Extras
{
    public class API_PlayerBase
    {
        /// <summary>
        /// the in-game slot (don't know how to use yet)
        /// </summary>
        public byte Slot
        {
            get;
            internal set;
        }

        /// <summary>
        /// Player id on zloemu
        /// </summary>
        public uint ID
        {
            get;
            internal set;
        }

        /// <summary>
        /// Player name
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// returns the player name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
