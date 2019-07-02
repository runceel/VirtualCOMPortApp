using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace VirtualCOMPortApp
{
    public static class SerialPortService
    {
        private static AppServiceConnection _appServiceConnection;
        private static BackgroundTaskDeferral _backgroundTaskDeferral;
        private static readonly TaskCompletionSource<object> _initialized = new TaskCompletionSource<object>();

        private static Task WaitForInitializedAsync() => _initialized.Task;

        // launch the full trust process
        public static Task LaunchWin32AppAsync() => FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync().AsTask();
        // set up app service
        public static void AcceptAppServiceConnection(IBackgroundTaskInstance instance)
        {
            instance.Canceled += AppServiceCanceled;
            var appService = (AppServiceTriggerDetails)instance.TriggerDetails;
            _backgroundTaskDeferral = instance.GetDeferral();
            _appServiceConnection = appService.AppServiceConnection;
            _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
            _initialized.SetResult(null);
        }

        // get port names using AppServiceConnection
        public static async Task<string[]> GetPortNamesAsync()
        {
            await WaitForInitializedAsync();
            var response = await _appServiceConnection.SendMessageAsync(new ValueSet
            {
                ["command"] = "GetPortNames",
            });

            if (response.Status != AppServiceResponseStatus.Success)
            {
                throw new InvalidOperationException($"{response.Status}");
            }

            return JsonConvert.DeserializeObject<string[]>(response.Message["portNames"] as string ?? "[]");
        }

        // closing process
        private static void AppServiceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) =>
            _backgroundTaskDeferral.Complete();

        private static void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args) =>
            _backgroundTaskDeferral.Complete();

    }
}
