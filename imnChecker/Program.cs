using System;
using System.Threading.Tasks;

namespace imnChecker
{

    class Program
    {
        private static int _delay = 1000;
        private static string _indexNumber;
        private static string _pass;
        private static string _item;
        private static string _type;
        private static System _system;

        private static string _apiKey = "c5bb04c3d4f23a0fa976c53a37a08832694e5306edf0e0ff";

        private static INotifier _notifier;

        private static void Main(string[] args)
        {
            var delay = args.GetSafe(4);
            _indexNumber = args.GetSafe(0);
            _pass = args.GetSafe(1);
            _item = args.GetSafe(2);
            _type = args.GetSafe(3);
            _system = (System)int.Parse(args.GetSafe(5) ?? "0");

            switch (_system)
            {
                case System.Android:
                    _notifier = new AndroidNotifier();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (delay != null)
            {
                _delay = int.Parse(delay);
            }

            try
            {
                AsyncMain().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }            
        }

        private static async Task AsyncMain()
        {           
            var connectionProvider = new ConnetionProvider()
            {
                IndexNumber = _indexNumber,
                Pass = _pass,
                Item = _item?.Replace("_", " ") ?? "Programowanie obiektowe II",
                Type = _type?.Replace("_", " ") ?? "Egzamin"
            };

            if (_item != null)
            {
                connectionProvider.Item = _item;
            }

            if (_type != null)
            {
                connectionProvider.Type = _type;
            }

            while (!await connectionProvider.CheckGrades())
            {
                Console.WriteLine($"Last checked: {DateTime.Now} - no {_item ?? "POII"} from {_type ?? "Egzamin"} grade");
                await Task.Delay(_delay);
            }

            _notifier.ApiKey = _apiKey;
            var notified = "";

            while (true)
            {
                if (notified != "200")
                {
                    notified = await _notifier.SendNotification($"{DateTime.Now} - {_item ?? "POII"} from {_type ?? "Egzamin"} GRADE");
                }
                Console.WriteLine($"Last checked: {DateTime.Now} - {_item ?? "POII"} from {_type ?? "Egzamin"} GRADE");
                Console.Beep();
            }
        }
    }
}
