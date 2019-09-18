namespace TimetableMakerUIT.Models
{
    public class MClass
    {
        public string classCode { get; set; }
        public string className { get; set; }
        public string classTeacher { get; set; }
        public string classWeekDay { get; set; }
        public string classTime { get; set; }
        public string type { get; set; }
        public string genre{ get; set; }

        public MClass GetDeepCopy()
        {
            MClass deepCopy = new MClass();
            deepCopy.classCode = this.classCode;
            deepCopy.classTeacher = this.classTeacher;
            deepCopy.classWeekDay = this.classWeekDay;
            deepCopy.classTime = this.classTime;
            deepCopy.type = this.type;
            deepCopy.genre = this.genre;
            deepCopy.className = this.className;

            return deepCopy;
        }
    }
}