using Windows.Storage;

namespace Kifu.Utils
{
    static class Settings
    {
        public static string Black
        {
            get { return GetValue("black") ?? "Human"; }
            set { SetValue("black", value); }
        }

        public static string White
        {
            get { return GetValue("white") ?? "Human"; }
            set { SetValue("white", value); }
        }

        public static string Size
        {
            get { return GetValue("size") ?? "19x19"; }
            set { SetValue("size", value); }
        }

        public static string Rules
        {
            get { return GetValue("rules") ?? "Chinese"; }
            set { SetValue("rules", value); }
        }

        public static int Handicap
        {
            get
            {
                string h = GetValue("handicap");
                return h == null ? 0 : int.Parse(h);
            }
            set { SetValue("handicap", value); }
        }

        private static string GetValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return ApplicationData.Current.LocalSettings.Values[key].ToString();
            return null;
        }

        private static void SetValue(string key, object value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
