using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using TimetableMakerUIT.Models;

namespace TimetableMakerUIT.Controllers
{
    [Route("api/[controller]")]
    public class MainController : Controller
    {
        //Đọc file excel danh sách lớp để tạo thành object
        public Root ReadExcelFile()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open("/test.xlsx", FileMode.Open, FileAccess.Read))
            {

                //Object chứa tất cả dữ liệu
                Root root = new Root();
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            var subjectCode = reader.GetValue(1);
                            if (subjectCode != null)
                            {
                                // Dữ liệu lớp
                                MClass classData = new MClass();
                                classData.classCode = reader.GetValue(2).ToString();
                                classData.classTeacher = reader.GetValue(5) != null ? reader.GetValue(5).ToString() : "*";
                                classData.classWeekDay = reader.GetValue(10) != null ? reader.GetValue(10).ToString() : "*";
                                classData.classTime = reader.GetValue(11) != null ? reader.GetValue(11).ToString() : "*";
                                classData.className = reader.GetValue(3) != null ? reader.GetValue(3).ToString() : "*";

                                if (classData.classCode.Substring(classData.classCode.Length - 2) == ".1" || classData.classCode.Substring(classData.classCode.Length - 2) == ".2")
                                    classData.type = "TH";
                                else
                                    classData.type = "LT";

                                if (classData.type == "LT")
                                {
                                    if (classData.classCode.Substring(classData.classCode.Length - 2) == "TT")
                                        classData.genre = "TT";
                                    else if ((classData.classCode.Substring(classData.classCode.Length - 3) == "CLC") || (
                                        classData.classCode.Substring(classData.classCode.Length - 2) == "CL"))
                                        classData.genre = "CLC";
                                    else if (classData.classCode.Substring(classData.classCode.Length - 2) == "TN")
                                        classData.genre = "TN";
                                    else
                                        classData.genre = "CQ";
                                }

                                // Thêm dữ liệu lớp học
                                Subject subjectData = null;
                                if (root.subjects == null)
                                {
                                    subjectData = new Subject();

                                    List<MClass> classes = new List<MClass>();
                                    classes.Add(classData);
                                    subjectData.classes = classes;

                                    List<Subject> subjects = new List<Subject>();
                                    subjects.Add(subjectData);
                                    root.subjects = subjects;
                                }

                                //Tìm xem mã môn học đã tồn tại chưa, nếu có thì thêm lớp vào môn học có sẵn
                                for (int i = 0; i < root.subjects.Count; i++)
                                {
                                    if (root.subjects[i].subjectCode == subjectCode.ToString())
                                    {
                                        subjectData = root.subjects[i];
                                        subjectData.classes.Add(classData);
                                        root.subjects[i] = subjectData;
                                        break;
                                    }
                                }

                                //Nếu mã môn học chưa tồn tại, tạo môn học mới
                                if (subjectData == null)
                                {
                                    subjectData = new Subject();
                                    subjectData.subjectCode = subjectCode.ToString();
                                    List<MClass> classes = new List<MClass>();
                                    classes.Add(classData);
                                    subjectData.classes = classes;

                                    var subjects = root.subjects;
                                    subjects.Add(subjectData);
                                }

                            }
                        }
                    } while (reader.NextResult());
                }
                return root;
            }
        }

        //Visualize lịch tuần lên trên console
        public void Visualization(List<List<MClass>> currentWeek)
        {
            foreach(List<MClass> day in currentWeek)
            {
                if (day == null)
                    for (int i = 0; i < 10; i++)
                        System.Console.Write("null ");
                else
                    foreach (MClass mclass in day)
                    {
                        if (mclass == null) System.Console.Write("null ");
                        else System.Console.Write(mclass.classCode + " ");
                    }
                System.Console.WriteLine("");
            }
        }

        public List<MClass> CopyDay(List<MClass> day)
        {
            List<MClass> dayCopy = new List<MClass>();
            if(day != null)
                for(int i = 0; i < day.Count; i++)
                {
                    var mclass = day[i];
                    if (mclass != null)
                        dayCopy.Add(mclass.GetDeepCopy());
                    else
                        dayCopy.Add(null);
                }
            else
                dayCopy = new List<MClass>() { null, null, null, null, null, null, null, null, null, null };

            return dayCopy;
        }

        public List<List<MClass>> CopyWeek(List<List<MClass>> week)
        {
            List<List<MClass>> weekCopy = new List<List<MClass>>();
            if(week != null)
                for (int i = 0; i < week.Count; i++)
                {
                    var day = week[i];
                    weekCopy.Add(CopyDay(day));
                }
            else
                for (int i = 0; i < 9; i++)
                {
                    List<MClass> day = new List<MClass>() { null, null, null, null, null, null, null, null, null, null };
                    weekCopy.Add(day);
                }

            return weekCopy;
        }

        //Sắp xếp tất cả các lịch có thể
        public void GetAllPossibleArrangement(List<Subject> subjects, ref List<List<List<MClass>>> results, List<List<MClass>> currentWeek,  int index, string genre)
        {
            var checkedEnglishClasses = new List<MClass>();
            for (int mclassCounter = 0; mclassCounter < subjects[index].classes.Count; mclassCounter++)
            {
                var mclass = subjects[index].classes[mclassCounter];
                if (mclass.genre != genre || checkedEnglishClasses.Contains(mclass)) continue;
                List<List<MClass>> cloneCurrentWeek = null;
                //Nếu dãy hiện tại là null, khởi tạo 1 dãy mới
                if (currentWeek == null)
                {
                    cloneCurrentWeek = new List<List<MClass>>();

                    for (int i = 0; i < 9; i++)
                    {
                        List<MClass> day = new List<MClass>() { null, null, null, null, null, null, null, null, null, null, null };
                        cloneCurrentWeek.Add(day);
                    }
                }
                else
                    cloneCurrentWeek = CopyWeek(currentWeek);

                //Bỏ qua các lớp thực hành
                if (mclass.type == "TH") continue;

                else
                {
                    Console.WriteLine(mclass.classCode);
                    //Nếu thời gian là *, thêm lớp vào dãy hiện tại tại vị trí số 8 (không thuộc ngày nào cả)
                    if (mclass.classTime == "*" || mclass.classWeekDay == "*")
                    {
                        var currentWeekDay = cloneCurrentWeek[8];
                        var mclassTime = mclass.classTime.ToCharArray().Select(c => c.Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();

                        for (int time = 0; time < mclassTime.Count; time++)
                            currentWeekDay.Add(mclass);
                    }

                    else
                    {
                        //Ngày học của lớp đang xét
                        var mclassWeekDay = Int32.Parse(mclass.classWeekDay);
                        //Thời gian học của lớp đang xét
                        var mclassTime = mclass.classTime.ToCharArray().Select(c => c.Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();
                        //Lấy ngày học đó trong chuỗi
                        var currentWeekDay = cloneCurrentWeek[mclassWeekDay];

                        //Tìm xem có lớp thực hành nào tương ứng với lớp đang xét không
                        var THmclasses = subjects[index].classes.Where(item => item.classCode.Substring(0, item.classCode.Length - 2) == mclass.classCode);
                        Console.WriteLine("Class time: " + mclass.classTime);
                        Console.WriteLine("Class day: " + mclass.classWeekDay);
                        //Nếu đây là vòng lặp đầu tiên, thêm lớp vào dãy mà không cần xét điều kiện
                        if (index == subjects.Count - 1)
                        {
                            //Xét lớp Anh văn, có thể có nhiều tiết 1 tuần
                            MClass mclass2 = null;
                            if (mclass.classCode.Substring(0, 3).Equals("ENG"))
                            {
                                Console.WriteLine("This is English");
                                for (int mclass2Counter = 0; mclass2Counter < subjects[index].classes.Count; mclass2Counter++)
                                {
                                    if ((subjects[index].classes[mclass2Counter].classCode == mclass.classCode) && 
                                        (subjects[index].classes[mclass2Counter] != mclass))
                                    {
                                        mclass2 = subjects[index].classes[mclass2Counter];
                                        break;
                                    }
                                }   
                            }
                            //Gán giá trị các tiết học của lớp đang xét vào ngày học
                            for (int time = 0; time <= mclassTime.Count - 1; time++)
                            {
                                var classTime = mclassTime[time];
                                currentWeekDay[classTime] = mclass;
                            }
                            //Xét tiết học thứ 2 của lớp Anh văn nếu tồn tại
                            if (mclass2 != null)
                            {
                                Console.WriteLine("Class 2 available");
                                checkedEnglishClasses.Add(mclass2);
                                //Ngày học của lớp đang xét
                                var mclass2WeekDay = Int32.Parse(mclass2.classWeekDay);
                                //Thời gian học của lớp đang xét
                                var mclass2Time = mclass2.classTime.ToCharArray().Select(c => c.Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();
                                //Lấy ngày học đó trong chuỗi
                                var current2WeekDay = cloneCurrentWeek[mclass2WeekDay];

                                //Gán giá trị các tiết học của lớp đang xét vào ngày học
                                for (int time = 0; time <= mclass2Time.Count - 1; time++)
                                {
                                    var class2Time = mclass2Time[time];
                                    current2WeekDay[class2Time] = mclass2;
                                }
                            }
                            Visualization(cloneCurrentWeek);

                            if (THmclasses.Count() > 0)
                            {
                                foreach (MClass THmclass in THmclasses)
                                {
                                    var THCurrentWeek = CopyWeek(cloneCurrentWeek);
                                    if (THmclass.classTime == "*")
                                        THCurrentWeek[8].Add(THmclass);
                                    else
                                    {
                                        //Thêm ngày TH vào dãy
                                        var THmclassWeekDay = Int32.Parse(THmclass.classWeekDay);
                                        var THmclassTime = THmclass.classTime.ToCharArray().Select(c => c .Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();
                                        var THcurrentWeekDay = THCurrentWeek[THmclassWeekDay];

                                        for (int time = 0; time <= THmclassTime.Count - 1; time++)
                                        {
                                            var classTime = THmclassTime[time];
                                            THcurrentWeekDay[classTime] = THmclass;
                                        }
                                    }
                                    GetAllPossibleArrangement(subjects, ref results, THCurrentWeek, index - 1, genre);
                                }
                            }
                            else
                                GetAllPossibleArrangement(subjects, ref results, cloneCurrentWeek, index - 1, genre);
                        }

                        //Nếu đang xét các vòng lặp tiếp theo, cần xét điều kiện để thêm lớp vào dãy
                        else
                        {
                            var mclassTimeIsAvailable = true;
                            var THmclassTimeIsAvailable = true;

                            //Kiểm tra xem có thể xếp lớp vào chuỗi đc không
                            for (int time = 0; time <= mclassTime.Count - 1; time++)
                            {
                                var classTime = mclassTime[time];
                                if (currentWeekDay[classTime] != null)
                                {
                                    mclassTimeIsAvailable = false;
                                    break;
                                }
                            }
                            if (!mclassTimeIsAvailable)
                                continue;
                            else
                            {
                                Console.WriteLine("This class is available");
                                //Gán giá trị các tiết học của lớp đang xét vào ngày học
                                for (int time = 0; time <= mclassTime.Count - 1; time++)
                                {
                                    var classTime = mclassTime[time];
                                    currentWeekDay[classTime] = mclass;
                                }
                                //Xét lớp Anh văn, có thể có nhiều tiết 1 tuần
                                MClass mclass2 = null;
                                if (mclass.classCode.Substring(0, 3).Equals("ENG"))
                                {
                                    Console.WriteLine("This is English");
                                    for (int mclass2Counter = 0; mclass2Counter < subjects[index].classes.Count; mclass2Counter++)
                                    {
                                        if ((subjects[index].classes[mclass2Counter].classCode == mclass.classCode) &&
                                            (subjects[index].classes[mclass2Counter] != mclass))
                                        {
                                            mclass2 = subjects[index].classes[mclass2Counter];
                                            break;
                                        }
                                    }
                                }
                                
                                //Nếu ngày học Anh văn thứ 2 tồn tại
                                if (mclass2 != null)
                                {
                                    var mclass2TimeIsAvailable = true;
                                    Console.WriteLine("ENG Class 2 exists");
                                    checkedEnglishClasses.Add(mclass2);
                                    //Ngày học của lớp đang xét
                                    var mclass2WeekDay = Int32.Parse(mclass2.classWeekDay);
                                    //Thời gian học của lớp đang xét
                                    var mclass2Time = mclass2.classTime.ToCharArray().Select(c => c.Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();
                                    //Lấy ngày học đó trong chuỗi
                                    var current2WeekDay = cloneCurrentWeek[mclass2WeekDay];

                                    //Kiểm tra xem có thể xếp lớp vào chuỗi đc không
                                    for (int time = 0; time <= mclass2Time.Count - 1; time++)
                                    {
                                        var class2Time = mclass2Time[time];
                                        if (current2WeekDay[class2Time] != null)
                                        {
                                            mclass2TimeIsAvailable = false;
                                            break;
                                        }
                                    }
                                    if (!mclass2TimeIsAvailable)
                                        continue;

                                    Console.WriteLine("ENG Class 2 is available");
                                    //Gán giá trị các tiết học của lớp đang xét vào ngày học
                                    for (int time = 0; time <= mclass2Time.Count - 1; time++)
                                    {
                                        var class2Time = mclass2Time[time];
                                        current2WeekDay[class2Time] = mclass2;
                                    }
                                }


                                Visualization(cloneCurrentWeek);
                                //Xét lớp thực hành
                                if (THmclasses.Count() > 0)
                                {
                                    foreach (MClass THmclass in THmclasses)
                                    {
                                        var THCurrentWeek = CopyWeek(cloneCurrentWeek);
                                        Console.WriteLine("TH class: " + THmclass.classCode);
                                        if (THmclass.classTime == "*")
                                        {
                                            THCurrentWeek[8].Add(THmclass);
                                            if (index != 0)
                                                GetAllPossibleArrangement(subjects, ref results, THCurrentWeek, index - 1, genre);
                                            else
                                            {
                                                Console.WriteLine("add to results");
                                                results.Add(THCurrentWeek);
                                            }
                                        }

                                        //Kiểm tra xem có thể xếp lớp vào chuỗi đc không

                                        else
                                        {
                                            var THmclassWeekDay = Int32.Parse(THmclass.classWeekDay);
                                            var THmclassTime = THmclass.classTime.ToCharArray().Select(c => c.Equals('0') ? 10 : Convert.ToInt32(c.ToString())).ToList();
                                            var THcurrentWeekDay = THCurrentWeek[THmclassWeekDay];

                                            Console.WriteLine("TH Class time: " + THmclass.classTime);
                                            Console.WriteLine("TH Class day: " + THmclass.classWeekDay);

                                            for (int time = 0; time <= THmclassTime.Count - 1; time++)
                                            {
                                                int classTime = THmclassTime[time];
                                                if (THcurrentWeekDay[classTime] != null)
                                                {
                                                    THmclassTimeIsAvailable = false;
                                                    break;
                                                }
                                            }

                                            if (THmclassTimeIsAvailable)
                                            {
                                                Console.WriteLine("This TH class is available");

                                                for (int time = 0; time <= THmclassTime.Count - 1; time++)
                                                    THcurrentWeekDay[THmclassTime[time]] = THmclass;

                                                Visualization(THCurrentWeek);

                                                if (index != 0)
                                                    GetAllPossibleArrangement(subjects, ref results, THCurrentWeek, index - 1, genre);
                                                else
                                                {
                                                    Console.WriteLine("add to results");
                                                    results.Add(THCurrentWeek);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (index != 0)
                                        GetAllPossibleArrangement(subjects, ref results, cloneCurrentWeek, index - 1, genre);
                                    else
                                    {
                                        Console.WriteLine("add to results");
                                        results.Add(cloneCurrentWeek);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } 

        [HttpPost("[action]")]
        public List<List<List<MClass>>> CreateTimetable([FromBody]RequestData requestData)
        {
            //Lấy ra các môn học từ request
            List<string> subjectCodes = requestData.subjectCodes;
            List<Subject> allSubjects = ReadExcelFile().subjects;
            List<Subject> requestSubjects = new List<Subject>();
            for (int i = 0; i < subjectCodes.Count(); i++)
            {
                var subject = allSubjects.Find(item => item.subjectCode == subjectCodes[i]);
                if(subject != null)
                    requestSubjects.Add(subject);
            }

            List<List<List<MClass>>> results = new List<List<List<MClass>>>() { };
            GetAllPossibleArrangement(requestSubjects, ref results, null, requestSubjects.Count() - 1, requestData.classType);

            return results;
        }

        //[HttpGet("[action]")]
        //public Dictionary<string, Dictionary<string, Dictionary<string, string>>> ReadExcelFile()
        //{
        //    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        //    using (var stream = System.IO.File.Open("/test.xlsx", FileMode.Open, FileAccess.Read))
        //    {
        //        Dictionary<string, Dictionary<string, Dictionary<string, string>>> dataTable = 
        //            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        //        using (var reader = ExcelReaderFactory.CreateReader(stream))
        //        {
        //            do
        //            {
        //                while (reader.Read())
        //                {
        //                    var subjectCode = reader.GetValue(1);
        //                    if (subjectCode != null)
        //                    {
        //                        // Dữ liệu lớp
        //                        var classCode = reader.GetValue(2).ToString();
        //                        var classTeacher = reader.GetValue(5) != null ? reader.GetValue(5).ToString() : "*";
        //                        var classWeekDay = reader.GetValue(10) != null ? reader.GetValue(10).ToString() : "*";
        //                        var classTime = reader.GetValue(11) != null ? reader.GetValue(11).ToString() : "*";

        //                        var classData = new Dictionary<string, string>();
        //                        classData.Add("classTeacher", classTeacher);
        //                        classData.Add("classWeekDay", classWeekDay);
        //                        classData.Add("classTime", classTime);
        //                        if (classCode.Substring(classCode.Length - 2) == ".1" || classCode.Substring(classCode.Length - 2) == ".2")
        //                            classData.Add("type", "TH");
        //                        else
        //                            classData.Add("type", "LT");

        //                        // Đọc mã môn học
        //                        if (!dataTable.ContainsKey(subjectCode.ToString()))
        //                        {
        //                            //Nếu chưa có môn học này
        //                            var subjectData = new Dictionary<string, Dictionary<string, string>>();

        //                            //Thêm dữ liệu lớp vào dữ liệu môn học
        //                            subjectData.Add(classCode, classData);
        //                            dataTable.Add(subjectCode.ToString(), subjectData);
        //                        }
        //                        else
        //                        {
        //                            //Nếu đã có môn học này
        //                            var subjectData = dataTable[subjectCode.ToString()];

        //                            //Kiểm tra xem mã lớp đã tồn tại chưa (dành cho các lớp Anh văn)
        //                            if (subjectData.ContainsKey(classCode))
        //                            {
        //                                var newClassData = subjectData[classCode];
        //                                newClassData["classWeekDay"] += "+" + classWeekDay;
        //                                newClassData["classTime"] += "+" + classTime;
        //                                subjectData[classCode] = newClassData;
        //                            }
        //                            else
        //                                subjectData.Add(classCode, classData);
        //                            dataTable[subjectCode.ToString()] = subjectData;
        //                        }
        //                    }
        //                }
        //            } while (reader.NextResult());
        //        }
        //        return dataTable;
        //    }

        //}

    }
}
