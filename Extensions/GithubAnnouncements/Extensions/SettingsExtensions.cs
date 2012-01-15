using System;
using Jabbot;

namespace GithubAnnouncements.Extensions
{
    public static class SettingsExtensions
    {
        public static T GetValue<T>(this ISettingsService settings, string key, Func<T> defaultValue)
        {
            return settings.ContainsKey(key) ? settings.Get<T>(key) : defaultValue();
        }
    }
}