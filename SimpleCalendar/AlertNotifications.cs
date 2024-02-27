using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCalendar {
    internal class AlertNotifications {
        public bool notificationsEnabled = true;
        public int notificationtimeSpan = 3;        //notifications will be send for all alerts in the next x days

        public DateTime currentDate;
        public Alerts alerts;

        public AlertNotifications(DateTime date, Alerts alerts) {
            this.currentDate = date;
            this.alerts = alerts;

            SendNotificationAsync(alerts.RetrieveAlerts(currentDate));
        }

        /// <summary>
        /// Sends a reminder notification on host os with the given alert data.
        /// </summary>
        /// <param name="alert"></param>
        public async Task SendNotificationAsync(List<Alert> alertList) {
            if(alertList == null) {
                return;
            }

            await Task.Delay(5000);


            foreach (Alert alert in alertList) {
                //Requires Microsoft.Toolkit.Uwp.Notifications NuGet package version 7.0 or greater
                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddAttributionText("")
                    .AddText("Reminder!")
                    .SetToastScenario(ToastScenario.Reminder)
                    .AddText($"{alert.hour} : {alert.minute}    {alert.alertName}")
                    .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 6 (or later), then your TFM must be net6.0-windows10.0.17763.0 or greater
            }
        }

    }
}
