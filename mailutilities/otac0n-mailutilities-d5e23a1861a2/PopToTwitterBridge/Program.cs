namespace TwitterPop
{
    using System.Threading;
    using JohnGietzen.MailUtilities.Pop.Server;

    class Program
    {
        static void Main(string[] args)
        {
            PopServer ps = new PopServer(new TwitterMailboxProvider());
            ps.Port = 1110;
            ps.Start();
            var st = ps.Status;
            do
            {
                Thread.Sleep(1000);
                st = ps.Status;
            }
            while (st == ThreadState.Background || st == ThreadState.WaitSleepJoin || st == (ThreadState.Background | ThreadState.WaitSleepJoin));
        }
    }
}
