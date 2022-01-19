using System.Collections.Generic;

namespace FrameworkAspNetExtended.Entities
{
    public class UserAutenticatedInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public IList<KeyValuePair<string, object>> ExtraInfo { get; set; }
        public IList<string> Profiles { get; set; }
        public IList<string> Operations { get; set; }

        public UserAutenticatedInfo(string username, string password)
        {
            Username = username;
            Password = password;
            Init();
        }

        public UserAutenticatedInfo()
        {
            Init();
        }

        private void Init()
        {
            ExtraInfo = new List<KeyValuePair<string, object>>();
            Profiles = new List<string>();
            Operations = new List<string>();
        }
    }
}
