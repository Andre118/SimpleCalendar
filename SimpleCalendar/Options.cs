using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCalendar {
    internal class Options {
        private bool alertsActivated = true;
        public bool AlertsActivated {
            get { return alertsActivated; }
            set { 
                alertsActivated = value;
                SafeOptions();
            }
        }


        private string TranslateOptionsToText() {
            string options = "";
            options += $"alertsActivated:{alertsActivated}";

            return options;
        }

        private void SetOptionsFromText(string options) {
            StringReader str = new StringReader(options);
            string? optionLine = str.ReadLine();

            if (optionLine != null) {
                string option = optionLine;
                string[] optionComponents = optionLine.Split(":");


                switch (optionComponents[0]) {
                    case "alertsActivated":
                        alertsActivated = bool.Parse(optionComponents[1]);
                        break;
                    default:
                        break;
                }
            }
        }

        public void SafeOptions() {
            string options = TranslateOptionsToText();
            string txtPath = Path.Combine(Environment.CurrentDirectory, "options.txt");
            File.WriteAllText(txtPath, options);
        }

        public void LoadOptions() {
            string txtPath = Path.Combine(Environment.CurrentDirectory, "options.txt");


            if (File.Exists(txtPath)) {
                string options = File.ReadAllText(txtPath);
                SetOptionsFromText(options);
            }
        }










        private static Options? Instance;
        public static Options GetInstance() {
            if(Instance == null) {
                Instance = new Options();
            }

            return Instance;
        }

        private Options() {
            LoadOptions();
        }
    }
}
