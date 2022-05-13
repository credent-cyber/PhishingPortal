using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class OnceOffSchedule : BaseSchedule
    {
        public OnceOffSchedule() { }

        public OnceOffSchedule(string value)
        {
            try
            {
                this.Date = DateTime.Parse(value);
                this.Time = DateTime.Parse(value);
            }
            catch (Exception)
            {
            }
        }

        public DateTime Date { get; set; }
        public DateTime Time { get; set; }

        public override bool Eval()
        {
            return DateTime.Now >= Date && (DateTime.Now - Date).TotalMinutes < 10;
        }

        public override string ToString()
        {
            return $"{this.Date.ToString("dd/MM/yyyy")} {this.Time.ToString("HH:mm:ss")}";
        }
    }
}
