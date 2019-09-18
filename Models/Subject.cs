using System.Collections.Generic;

namespace TimetableMakerUIT.Models
{
    public class Subject
    {
        public string subjectCode { get; set; }
        public List<MClass> classes { get; set; }
    }
}