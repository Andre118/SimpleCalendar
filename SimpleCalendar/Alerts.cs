using DynamicData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleCalendar {

    //this class will provide various definitions and functions to implement an appointment tracker and a corresponding alerts
    public class Alerts {

        private Dictionary<int, Dictionary<int, Dictionary<int, List<Alert>>>> alerts = new Dictionary<int, Dictionary<int, Dictionary<int, List<Alert>>>>();
        public event Action OnAlertChange;      //triggers whenever an alert is edited

        //list with items 0-23 to bind to a dropdown hour selection
        public ObservableCollection<int> hours = new ObservableCollection<int>();
        //list with items 0-59 to bind to a dropdown minute selection
        public ObservableCollection<int> minutes = new ObservableCollection<int>();


        public void RegisterAlert(DateTime date, Alert alert) {
            if (!alerts.ContainsKey(date.Year)) {
                alerts.Add(date.Year, new Dictionary<int, Dictionary<int, List<Alert>>>());
            }

            if (!alerts[date.Year].ContainsKey(date.Month)) {
                alerts[date.Year].Add(date.Month, new Dictionary<int, List<Alert>>());
            }

            if (!alerts[date.Year][date.Month].ContainsKey(date.Day)) {
                alerts[date.Year][date.Month].Add(date.Day, new List<Alert>());
            }

            InsertAlertToList(alerts[date.Year][date.Month][date.Day], alert);
            //alerts[date.Year][date.Month][date.Day].Add(alert);

            SaveAlertsToJson();
            OnAlertChange?.Invoke();


        }

        public List<Alert>? RetrieveAlerts(DateTime date) {
            if (!alerts.ContainsKey(date.Year)) {
                return null;
            }

            if (!alerts[date.Year].ContainsKey(date.Month)) {
                return null;
            }

            if (!alerts[date.Year][date.Month].ContainsKey(date.Day)) {
                return null;
            }

            return alerts[date.Year][date.Month][date.Day];
        }

        public void EditAlert(DateTime date, int alertIndex, int hour, int minute, string name) {
            if (!alerts.ContainsKey(date.Year)) {
                throw new Exception("EditAlert was called on a previously unregistered day, this should normally never be possible as EditAlert should only be able to be called after RegisterAlert.");
            }

            if (!alerts[date.Year].ContainsKey(date.Month)) {
                throw new Exception("EditAlert was called on a previously unregistered day, this should normally never be possible as EditAlert should only be able to be called after RegisterAlert.");
            }

            if (!alerts[date.Year][date.Month].ContainsKey(date.Day)) {
                throw new Exception("EditAlert was called on a previously unregistered day, this should normally never be possible as EditAlert should only be able to be called after RegisterAlert.");
            }

            Alert alertToEdit = alerts[date.Year][date.Month][date.Day][alertIndex];
            alertToEdit.hour = hour;
            alertToEdit.minute = minute;
            alertToEdit.alertName = name;


            SaveAlertsToJson();
            OnAlertChange?.Invoke();
        }

        public void DeleteAlert(DateTime date, int alertindex) {
            if (!alerts.ContainsKey(date.Year) || !alerts[date.Year].ContainsKey(date.Month) || !alerts[date.Year][date.Month].ContainsKey(date.Day)) {
                throw new Exception("DeleteAlert was called on a nonexistant alert.");
            }

            alerts[date.Year][date.Month][date.Day].RemoveAt(alertindex);

            SaveAlertsToJson();
            OnAlertChange?.Invoke();
        }

        //inserts an alert to an alert list, guarantees alerts are sorted in chronological order
        private void InsertAlertToList(List<Alert> alerts, Alert alert) {
            for (int i = 0; i < alerts.Count; i++) {
                if(alert.hour < alerts[i].hour || (alert.hour == alerts[i].hour && alert.minute < alerts[i].minute)) {
                    alerts.Insert(i, alert);
                    return;
                }
            }

            alerts.Add(alert);

            Debug.Assert(IsAlertListChronological(alerts), "Assertion broken: Alert list is not in chronological order!");

            return;
        }

        private bool IsAlertListChronological(List<Alert> alerts) {
            int hour = 0;
            int minute = 0;

            foreach(Alert alert in alerts) {
                if(hour > alert.hour || (hour == alert.hour && minute > alert.minute)) {
                    return false;
                }
                else {
                    hour = alert.hour; 
                    minute = alert.minute;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves alerts dictionary as json textfile.
        /// </summary>
        public void SaveAlertsToJson() {
            string alertJson = JsonConvert.SerializeObject(alerts, Formatting.Indented);
            string txtPath = Path.Combine(Environment.CurrentDirectory, "alertJson.json");
            File.WriteAllText(txtPath, alertJson);
        }

        public void LoadAlertsFromJson() {
            string txtPath = Path.Combine(Environment.CurrentDirectory, "alertJson.json");


            if (File.Exists(txtPath)) {
                alerts = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, Dictionary<int, List<Alert>>>>>(File.ReadAllText(txtPath));
                OnAlertChange?.Invoke();
            }
        }

        public Alerts() { 
            for(int i = 0; i < 24; i++) { 
                hours.Add(i);
            }

            for (int i = 0; i < 60; i++) {
                minutes.Add(i);
            }
        }
    }

    public class Alert {
        public int hour;
        public int minute;
        public string alertName;

        public Alert(int hour, int minute, string alertName) {
            this.hour = hour;
            this.minute = minute;
            this.alertName = alertName;
        }
    }
}
