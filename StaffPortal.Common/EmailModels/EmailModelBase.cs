using System;
using System.Collections.Generic;

namespace StaffPortal.Common.EmailModels
{
    public abstract class EmailModelBase : IDictionariable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Template_FileName { get; set; }

        public EmailModelBase()
        {

        }

        public EmailModelBase(string firstName, string lastName, string to, string subject)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.To = to;
            this.Subject = subject;
        }

        public EmailModelBase(string firstName, string lastName, string to)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.To = to;
        }

        public IDictionary<string, string> ToDictionary(string prefix, string suffix = "")
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var properties = this.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (prop.PropertyType == typeof(string) && prop.GetValue(this, null) != null)
                {
                    var val = prop.GetValue(this).ToString();
                    dictionary.Add(prefix + prop.Name + suffix, val.ToString());
                }
            }

            return dictionary;
        }     
    }
}