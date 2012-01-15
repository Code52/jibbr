namespace Jabbot
{
    public static class BotExtensions
    {
        public static void SayToAllRooms(this IBot bot, string what)
        {
            foreach (var room in bot.Rooms)
            {
                bot.Say(what, room);
            }
        }
    }
}
