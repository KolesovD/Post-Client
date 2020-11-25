namespace TwitterPop
{
    public class TwitterMessage
    {
        public string in_reply_to_screen_name
        {
            get;
            set;
        }

        public string created_at
        {
            get;
            set;
        }

        public string text
        {
            get;
            set;
        }

        public string source
        {
            get;
            set;
        }

        public long id
        {
            get;
            set;
        }

        public TwitterUser user
        {
            get;
            set;
        }
    }
}
