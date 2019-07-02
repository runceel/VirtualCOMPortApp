using Newtonsoft.Json;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace VirtualCOMPortApp.Win32
{
    class Program
    {
        private static TaskCompletionSource<object> Finished { get; } = new TaskCompletionSource<object>();
        static async Task Main(string[] args)
        {
            try
            {
                var conn = new AppServiceConnection();
                conn.PackageFamilyName = Package.Current.Id.FamilyName;
                conn.AppServiceName = "InProcessAppService";
                conn.RequestReceived += Conn_RequestReceived;
                conn.ServiceClosed += Conn_ServiceClosed;
                await conn.OpenAsync();
                await Finished.Task;
            }
            catch
            {
                // add error handling logic
            }
        }

        private static void Conn_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Finished.SetResult(null);
        }

        private static async void Conn_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var d = args.GetDeferral();
            var requestMessage = args.Request.Message;
            if (requestMessage.TryGetValue("command", out var commandName))
            {
                switch((string)commandName)
                {
                    case "GetPortNames":
                        {
                            await args.Request.SendResponseAsync(new ValueSet
                            {
                                ["portNames"] = JsonConvert.SerializeObject(SerialPort.GetPortNames()),
                            });
                            break;
                        }
                    default:
                        // noop
                        break;
                }
            }
            d.Complete();
        }
    }
}
