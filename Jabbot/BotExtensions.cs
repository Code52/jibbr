namespace Jabbot
{
    public static class BotExtensions
    {
        public static void SayToAllRooms(this Bot bot, string what)
        {
            foreach (var room in bot.Rooms)
            {
                bot.Say(what, room);
            }
        }
    }
}
