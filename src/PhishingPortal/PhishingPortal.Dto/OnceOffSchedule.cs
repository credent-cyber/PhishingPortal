using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class OnceOffSchedule : BaseSchedule
    {
        public OnceOffSchedule() { }

        public OnceOffSchedule(string value)
        {
            this.DateTime = DateTime.Parse(value);
        }

        public DateTime DateTime { get; set; }

        public override bool Eval()
        {
            return DateTime.Now >= DateTime && (DateTime.Now - DateTime).TotalMinutes < 10;
        }

        public override string ToString()
        {
            return this.DateTime.ToString();
        }
    }
}
