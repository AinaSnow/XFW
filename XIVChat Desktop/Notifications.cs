using System.Runtime.InteropServices;
using Microsoft.Toolkit.Uwp.Notifications;

namespace XIVChat_Desktop {
    public static class Notifications {
        [System.Obsolete]
        public static void Initialise() {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<XivChatNotificationActivator>("XIVChat.XIVChat_Desktop");
            DesktopNotificationManagerCompat.RegisterActivator<XivChatNotificationActivator>();
        }
    }


    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("F12BCC85-6FEE-4A9A-BBB8-08DFAA7BE1A8"), ComVisible(true)]
    [System.Obsolete]
    public class XivChatNotificationActivator : NotificationActivator {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId) {
            // TODO: Handle activation
        }
    }
}
