using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCalendar {
    //this class manages the initilization and saving/loading of background data
    internal class Datamanager {
        //datastructure to keep the users daily notes, grouped by year, month, day
        private Dictionary<int, Dictionary<int, Dictionary<int, string>>> dailyNotes = new Dictionary<int, Dictionary<int, Dictionary<int, string>>>();
        public event Action OnNoteChange;

        //add an entry to a day
        public void WriteDailyNote(DateTime date, string note) {
            if (!dailyNotes.ContainsKey(date.Year)) {
                dailyNotes.Add(date.Year, new Dictionary<int, Dictionary<int, string>>());
            }

            if (!dailyNotes[date.Year].ContainsKey(date.Month)) {
                dailyNotes[date.Year].Add(date.Month, new Dictionary<int, string>());
            }

            dailyNotes[date.Year][date.Month][date.Day] = note;
            SaveDailyNotes();
            OnNoteChange?.Invoke();
        }

        public string GetDailyNote(DateTime date) {

            if (!dailyNotes.ContainsKey(date.Year)) {
                return "";
            }

            if (!dailyNotes[date.Year].ContainsKey(date.Month)) {
                return "";
            }

            if (!dailyNotes[date.Year][date.Month].ContainsKey(date.Day)) {
                return "";
            }

            return dailyNotes[date.Year][date.Month][date.Day];
        }

        public void SaveDailyNotes() {
            string alertJson = JsonConvert.SerializeObject(dailyNotes, Formatting.Indented);
            string txtPath = Path.Combine(Environment.CurrentDirectory, "dailyNotes.json");
            File.WriteAllText(txtPath, alertJson);
        }

        public void LoadDailyNotes() {
            string txtPath = Path.Combine(Environment.CurrentDirectory, "dailyNotes.json");

            if (File.Exists(txtPath)) {
                dailyNotes = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, Dictionary<int, string>>>>(File.ReadAllText(txtPath));
                OnNoteChange?.Invoke();
            }
        }
    }
}
