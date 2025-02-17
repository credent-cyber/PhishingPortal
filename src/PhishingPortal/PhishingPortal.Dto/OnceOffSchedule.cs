﻿using Newtonsoft.Json;
using System;
using System.Globalization;

namespace PhishingPortal.Dto
{
    public class OnceOffSchedule : BaseSchedule
    {
        public OnceOffSchedule() { }

        public OnceOffSchedule(string value)
        {
            try
            {
                DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result);
                this.Date = result.Date;


                this.Time = result;
            }
            catch (Exception)
            {
            }
        }

        public DateTime Date { get; set; }
        public DateTime Time { get; set; }

        public override bool Eval()
        {
            if ((DateTime.Now - Time).TotalMinutes < 0)
                return false;

            return DateTime.Now.Date == Date && (DateTime.Now - Time).TotalMinutes < 10;
        }

        public override double GetElapsedTimeInMinutes()
        {
            var dtString = ToString();
            
            DateTime.TryParse(dtString, out var dt);

            return (DateTime.Now - dt).TotalMinutes;
        }

        public override string ToString()
        {
            return $"{this.Date.ToString("dd/MM/yyyy")} {this.Time.ToString("HH:mm:ss")}";
        }
    }
}
