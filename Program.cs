using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DNS.Server;
using DNS.Protocol.ResourceRecords;
using System.IO;

namespace DNS_ADS
{
    internal class Program
    {
        const string BLACK_LIST = "blocklist.txt";

        const string DNS_GOOGLE = "8.8.8.8";
        const string DEAD_END = "0.0.0.0";

        static ConsoleColor previousColor;

        static DnsServer dnsServer;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                Console.ForegroundColor = previousColor;
                Environment.Exit(0);
            };

            previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;

            loadBanner();

            Console.WriteLine(" #: Start DNS Server");
            Console.WriteLine(" #: using {0}", DNS_GOOGLE);

            MasterFile file = loadBlackList();
            await loadServerAsync(file);
        }

        static async Task loadServerAsync(MasterFile masterFile)
        {

            try
            {
                dnsServer = new DnsServer(DNS_GOOGLE);

                dnsServer.Requested += DnsServer_Requested;
                dnsServer.Responded += DnsServer_Responded;

                Console.WriteLine(" #: Listen...");
                await dnsServer.Listen();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await loadServerAsync(masterFile);

        }

        private static void DnsServer_Responded(object sender, DnsServer.RespondedEventArgs e)
        {

            string AAAA = string.Empty;
            string CNAME = string.Empty;

            if (e.Response.AnswerRecords.Count != 0)
            {
                for (int i = 0; i < e.Response.AnswerRecords.Count; i++)
                {
                    if (e.Response.AnswerRecords[i].Type == DNS.Protocol.RecordType.CNAME)
                    {
                        CNAME = e.Response.AnswerRecords[i].Name.ToString();
                    }

                    else if (e.Response.AnswerRecords[i].Type == DNS.Protocol.RecordType.AAAA)
                    {

                        IPAddressResourceRecord records = (IPAddressResourceRecord)e.Response.AnswerRecords[i];
                        AAAA = records.IPAddress.ToString();
                    }

                    else if (e.Response.AnswerRecords[i].Type == DNS.Protocol.RecordType.A)
                    {

                        IPAddressResourceRecord records = (IPAddressResourceRecord)e.Response.AnswerRecords[i];
                        AAAA = records.IPAddress.ToString();
                    }
                }
            }

            if (AAAA != string.Empty && CNAME != string.Empty)
            {
                Console.WriteLine(" #: {0} -> {1}", AAAA, CNAME);
            }

        }

        private static void DnsServer_Requested(object sender, DnsServer.RequestedEventArgs e)
        {
            if (e.Request.Questions.Count != 0)
            {
                for (int i = 0; i < e.Request.Questions.Count; i++)
                {
                    Console.WriteLine(" #: Request: " + e.Request.Questions[i].Name);
                }
            }
        }

        static MasterFile loadBlackList()
        {
            MasterFile masterFile = new MasterFile();

            string[] rows = File.ReadAllText(BLACK_LIST).Split('\n');
            if (rows.Length > 0)
            {
                Console.WriteLine(" #: Blacklist addresses:");

                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i].StartsWith("#"))
                    {
                        continue;
                    }

                    string address = rows[i].Trim();

                    masterFile.AddIPAddressResourceRecord(address, DEAD_END);

                    Console.Write(" #: [{0}] {1}", i, address.PadRight(Console.BufferWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                Console.WriteLine(" #: Load complete");
            }

            return masterFile;

        }

       

        static void loadBanner()
        {
            Console.WriteLine("  _____  _   _  _____                     _____   _____ ");
            Console.WriteLine(" |  __ \\| \\ | |/ ____|              /\\   |  __ \\ / ____|");
            Console.WriteLine(" | |  | |  \\| | (___    ______     /  \\  | |  | | (___  ");
            Console.WriteLine(" | |  | | . ` |\\___ \\  |______|   / /\\ \\ | |  | |\\___ \\ ");
            Console.WriteLine(" | |__| | |\\  |____) |           / ____ \\| |__| |____) |");
            Console.WriteLine(" |_____/|_| \\_|_____/           /_/    \\_\\_____/|_____/ ");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine(" DNS ads blocker, beta 0.31");
            Console.WriteLine(" www.rudenetworks.com");
            Console.WriteLine();
        }
    }
}
