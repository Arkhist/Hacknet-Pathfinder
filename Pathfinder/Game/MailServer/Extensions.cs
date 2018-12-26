using Hacknet;

namespace Pathfinder.Game.MailServer
{
    public static class Extensions
    {
        public static void AddEmailToServer(this Hacknet.MailServer server,
                                            string sender = null,
                                            string recip = null,
                                            string subject = null,
                                            string body = null,
                                            bool dontFilter = false)
        {
            sender = dontFilter ? sender ?? "UNKNOWN" : ComputerLoader.filter(sender ?? "UNKNOWN");
            recip = dontFilter || recip == null ? recip : ComputerLoader.filter(sender);
            subject = dontFilter ? subject ?? "UNKNOWN" : ComputerLoader.filter(subject ?? "UNKNOWN");
            body = dontFilter ? body ?? "UNKNOWN" : ComputerLoader.filter(body ?? "UNKNOWN");
            if (recip != null)
                server.AddMailToServer(recip, Hacknet.MailServer.generateEmail(subject, body, sender));
        }

        public static void AddMailToServer(this Hacknet.MailServer server, string recip, string mail)
            => server.setupComplete = () => server.addMail(mail, recip);
    }
}
