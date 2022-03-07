using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserController
{
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>() { };

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void RunCommand()
        {
            ConsoleOutput.Add(ConsoleInput);
            if (ConsoleOutput.Count > 1000)
            {
                while (ConsoleOutput.Count > 1000)
                {
                    ConsoleOutput.RemoveAt(0);
                }
            }
            // do your stuff here.
            ConsoleInput = String.Empty;
        }
        public void RunCommand(string input)
        {
            ConsoleOutput.Add(input);
            if (ConsoleOutput.Count > 1000)
            {
                while (ConsoleOutput.Count > 1000)
                {
                    ConsoleOutput.RemoveAt(0);
                }
            }

            //ConsoleInput = String.Empty;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
