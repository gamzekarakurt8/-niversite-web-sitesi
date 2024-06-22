using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;


namespace mezunprojesi.Controllers
{
    public class HomeController : Controller
    {

        public const string SessionUserIDKey = "userid";
        public const string SessionUserNameKey = "userName";
        public const string SessionDeptIDKey = "deptid";
        string connectionString = "Server=localhost;Database=university;Uid=root;Pwd=1234;";
        private MySqlConnection GetConnection()
        {
            // Veritabanı bağlantı dizesini Web.config dosyasından al
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new MySqlConnection(connectionString);
        }

        //App_Start dosyasında bulunan RouteConfigde homeyi açılacak html sayfası ile aynı yapman gerekiyo eğer 404 alırsan aklında  bulunsun mal:)

        public ActionResult Giris()
        {
            return View();
        }

        public ActionResult Admin()
        {
            // Admin sayfasının içeriği
            return View();

        }

        public ActionResult Teacher()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];
            return View();
        }


        [HttpPost]
        public ActionResult Giris(string userName, string password)
        {

            // klasik sql bağlantısını tanımlıyoruz burda kişilerin giriş bilgisine erişmek için.
            string connectionString = "Server=localhost;Database=university;Uid=root;Pwd=1234;";

            MySqlConnection conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM usertable WHERE userName = @UserName AND userPassword = @Password"; // şifre ve kulanıcı adını aldığımız tablo adını belirtiyoruz
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Password", password);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string accessLevel = reader["accessLevel"].ToString(); // kullanıcıların erişim seviyesini kontrol ediyoruz
                        int userID = Convert.ToInt32(reader["userid"]);
                        string retrievedUserName = reader["userName"].ToString();
                        int retrievedDeptID = Convert.ToInt32(reader["deptid"]);
                        


                        System.Web.Security.FormsAuthentication.SetAuthCookie(retrievedUserName, false);

                        // oturum oluştur
                        Session[SessionUserIDKey] = userID;
                        Session[SessionUserNameKey] = retrievedUserName;
                        Session[SessionDeptIDKey] = retrievedDeptID;

                        
                        switch (accessLevel)
                        {
                            case "admin":
                                return RedirectToAction("Admin");
                            case "teacher":
                                return RedirectToAction("Teacher");
                            case "student":
                                return RedirectToAction("Student");
                            default:
                                TempData["ErrorMessage"] = "Bilinmeyen erişim seviyesi!";
                                return RedirectToAction("Giris");
                        }
                        

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kullanıcı adı veya şifre yanlış!";
                        return RedirectToAction("Giris"); // yanlış giriş yaptığında giriş formuna yönlendiriyoruz sayfa yenileniyor
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bağlantı sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("Giris");
            }
            finally
            {
                conn.Close();
            }


            return View();
        }

        public ActionResult StudentHomework()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptID = Convert.ToInt32(Session[SessionDeptIDKey]);
            List<Assignment> assignment = GetAssignments(deptID);
            return View(assignment);
        }

        public List<Assignment> GetAssignments(int deptID)
        {
            List<Assignment> assignmentList = new List<Assignment>();

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            //MySqlConnection connection = new MySqlConnection(connectionString);
            {
                try
                {
                    connection.Open();
                    string query = "Select * from studenthomework where deptid = @deptid";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@deptid", deptID);

                    using(MySqlDataReader reader = command.ExecuteReader())
                    //MySqlDataReader reader = command.ExecuteReader();
                    {
                        while(reader.Read())
                        {
                            Assignment asg = new Assignment
                            {
                                dersid = Convert.ToInt32(reader["dersid"]),
                                lessonname = reader["lessonname"].ToString(),
                                explanation = reader["explanation"].ToString(),
                                startanddate = (DateTime) reader["startanddate"]
                            };

                            assignmentList.Add(asg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                    TempData["ErrorMessage"] = "Ders programı alınırken bir hata oluştu: " + ex.Message;
                }

                return assignmentList;
            }
        }

        public ActionResult StudentSyllabus()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptID = Convert.ToInt32(Session[SessionDeptIDKey]);
            List<Syllabus> syllabus = GetSyllabus(deptID);
            return View(syllabus);

        }

        public List<Syllabus> GetSyllabus(int deptID)
        {
            List<Syllabus> syllabusList = new List<Syllabus>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT s.lessonid, l.deptid, s.day, s.time, l.lessonname FROM syllabus s JOIN lesson l ON s.lessonid = l.lessonid WHERE s.deptid = @deptid ORDER BY s.day, s.time";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@deptid", deptID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Syllabus s = new Syllabus
                            {
                                coursename = reader["lessonname"].ToString(),
                                day = reader["day"].ToString(),
                                time = reader["time"].ToString(),
                            };
                            syllabusList.Add(s);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                    TempData["ErrorMessage"] = "Ders programı alınırken bir hata oluştu: " + ex.Message;
                }
            }

            return syllabusList;
        }


        public ActionResult StudentAbsence()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int userID = (int)Session[SessionUserIDKey];
            List<StudentAbsenceView> absence = GetAbsence(userID);
            return View(absence);

        }



        private List<StudentAbsenceView> GetAbsence(int userid)
        {
            List<StudentAbsenceView> absences = new List<StudentAbsenceView>();


            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT sa.*, l.lessonname FROM studentabsence sa JOIN lesson l ON sa.dersid = l.lessonid WHERE sa.userid = @userid";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userid", userid);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StudentAbsenceView absence = new StudentAbsenceView
                            {
                                dersadi = reader["lessonname"].ToString(),
                                dersid = Convert.ToInt32(reader["dersid"]),


                                week1 = Convert.ToInt32(reader["week1"] == DBNull.Value ? 0 : reader["week1"]),
                                week2 = Convert.ToInt32(reader["week2"] == DBNull.Value ? 0 : reader["week2"]),
                                week3 = Convert.ToInt32(reader["week3"] == DBNull.Value ? 0 : reader["week3"]),
                                week4 = Convert.ToInt32(reader["week4"] == DBNull.Value ? 0 : reader["week4"]),
                                week5 = Convert.ToInt32(reader["week5"] == DBNull.Value ? 0 : reader["week5"]),
                                week6 = Convert.ToInt32(reader["week6"] == DBNull.Value ? 0 : reader["week6"]),
                                week7 = Convert.ToInt32(reader["week7"] == DBNull.Value ? 0 : reader["week7"]),
                                week8 = Convert.ToInt32(reader["week8"] == DBNull.Value ? 0 : reader["week8"]),
                                week9 = Convert.ToInt32(reader["week9"] == DBNull.Value ? 0 : reader["week9"]),
                                week10 = Convert.ToInt32(reader["week10"] == DBNull.Value ? 0 : reader["week10"]),
                                week11 = Convert.ToInt32(reader["week11"] == DBNull.Value ? 0 : reader["week11"]),
                                week12 = Convert.ToInt32(reader["week12"] == DBNull.Value ? 0 : reader["week12"]),
                                week13 = Convert.ToInt32(reader["week13"] == DBNull.Value ? 0 : reader["week13"]),
                                week14 = Convert.ToInt32(reader["week14"] == DBNull.Value ? 0 : reader["week14"]),
                            };
                            absences.Add(absence);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    TempData["ErrorMessage"] = "Devams zl k sonu lar n  g sterirken bir hata meydana geldi.....: " + ex.Message;
                }
            }

            return absences;

        }




        public void SaveAbsence(StudentAbsence absence, int selectedWeek)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO StudentAbsence (userid, dersid,  week" + selectedWeek + ") VALUES (@userid, @dersid, @devamsizlik)";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@userid", absence.userid);
                    command.Parameters.AddWithValue("@dersid", absence.dersid);
                    command.Parameters.AddWithValue("@devamsizlik", absence.devamsizlik);


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "İşlem başarıyla kaydedildi.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: Kayıt eklenmedi.";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }
        }

        public void SaveStudent(Studentclass stu)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();  //bağlantıyı aç

                    string query = "Insert into usertable (userid, userName, userPassword, accessLevel, deptid, name_surname) Values (@StudentNumber, @StudentUsername, @StudentPassword, @AccessLevel, @Department, @StudentName_StudentSurname)";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@StudentNumber", stu.StudentNumber);
                    command.Parameters.AddWithValue("@StudentUsername", stu.StudentName);
                    command.Parameters.AddWithValue("@StudentPassword", stu.Password);
                    command.Parameters.AddWithValue("@AccessLevel", stu.AccessLevel);
                    command.Parameters.AddWithValue("@Department", stu.Department);
                    command.Parameters.AddWithValue("@StudentName_StudentSurname", stu.StudentSurname); // isim + soyisim yazacak buraya


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "İşlem başarıyla kaydedildi.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: Kayıt eklenmedi.";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }
        }

        private List<AnnouncmentView> GetAnnouncements(int deptid)
        {
            List<AnnouncmentView> announcements = new List<AnnouncmentView>();

            // Connect to the database and retrieve announcements here
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM announcements WHERE deptid = @deptid";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@deptid", deptid);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AnnouncmentView announcement = new AnnouncmentView
                            {
                                // Assuming your Announcement class has properties like lessonname, announce, and tarih
                                lessonname = reader["lessonname"].ToString(),
                                announce = reader["announce"].ToString(),
                                teachername = reader["teachername"].ToString()
                            };
                            announcements.Add(announcement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    TempData["ErrorMessage"] = "An error occurred while retrieving announcements: " + ex.Message;
                }
            }

            return announcements;
        }

        public void CreateAssignment(Assignment assignment)
        {
            int deptid = (int)Session[SessionDeptIDKey];

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "Insert into studenthomework (dersid, lessonname, explanation, startanddate, deptid) Values (@dersid, @lessonname, @explanation, @startanddate, @deptid)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@dersid", assignment.dersid);
                    command.Parameters.AddWithValue("@lessonname", assignment.lessonname);
                    command.Parameters.AddWithValue("@explanation", assignment.explanation);
                    command.Parameters.AddWithValue("@startanddate", assignment.startanddate);
                    command.Parameters.AddWithValue("@deptid", deptid);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Ödev girildi!";

                    }
                    else
                    {
                        TempData["SuccessMessage"] = "İşlem sırasında bir hata oluştu";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }


        }

        public ActionResult TeacherAssignment()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            return View();
        }

        [HttpPost]
        public ActionResult TeacherAssignment(Assignment assignment)
        {
            CreateAssignment(assignment);
            return View(assignment);
        }


        public void CreateExamResult(TeacherExam teacher)
        {
            if (Session[SessionUserIDKey] == null || Session[SessionUserNameKey] == null)
            {
                TempData["ErrorMessage"] = "Oturum bilgileri eksik. Lütfen tekrar giriş yapın.";
                return;
            }

            int teacherid = (int) Session[SessionUserIDKey];

            using(MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "Insert into examresult (dersid, userid, midtermresult, finalresult, teacherid) Values (@lessonid, @userid, @midtermresult, @finalresult, @teacherid)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@lessonid", teacher.lessonid);
                    command.Parameters.AddWithValue("@userid", teacher.userid);
                    command.Parameters.AddWithValue("@midtermresult", teacher.midtermresult);
                    command.Parameters.AddWithValue("@finalresult", teacher.finalresult);
                    command.Parameters.AddWithValue("@teacherid", teacherid);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Not Girildi!";

                    }
                    else
                    {
                        TempData["SuccessMessage"] = "İşlem sırasında bir hata oluştu";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }
            
        }

        public ActionResult TeacherExamGrade()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            return View();
        }

        [HttpPost]
        public ActionResult TeacherExamGrade(TeacherExam teacher)
        {
            CreateExamResult(teacher);
            return View(teacher);
        }

        public void CreateNewAnnouncement(Announcement announce)
        {
            // Ensure the session variables are set before accessing them
            if (Session[SessionUserIDKey] == null || Session[SessionUserNameKey] == null)
            {
                TempData["ErrorMessage"] = "Oturum bilgileri eksik. Lütfen tekrar giriş yapın.";
                return;
            }

            int userID = (int)Session[SessionUserIDKey];
            int deptID = (int)Session[SessionDeptIDKey];
            string sistemeGiren = (string)Session[SessionUserNameKey];

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "Insert into announcements (lessonname, teachername, announce, userid, deptid) Values (@lessonname, @teachername, @announce, @userid, @deptID)";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@lessonname", announce.lessonname);
                    command.Parameters.AddWithValue("@announce", announce.announce);
                    command.Parameters.AddWithValue("@teachername", sistemeGiren);
                    command.Parameters.AddWithValue("@userid", userID);
                    command.Parameters.AddWithValue("@deptid", deptID);


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Duyuru gönderildi!";

                    }
                    else
                    {
                        TempData["SuccessMessage"] = "İşlem sırasında bir hata oluştu";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }
        }

        public ActionResult CreateAnnouncement()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            return View();
        }


        [HttpPost]
        public ActionResult CreateAnnouncement(Announcement ann)
        {
            

            CreateNewAnnouncement(ann);
            return View(ann);
        }

        public void SaveTeacher(Teacherclass teacher)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();  //bağlantıyı aç

                    string query = "Insert into usertable (userid, userName, userPassword, accessLevel, deptid, name_surname) Values (@TeacherNumber, @TeacherUsername, @TeacherPassword, @AccessLevel, @Department, @TeacherName_TeacherSurname)";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@TeacherNumber", teacher.TeacherNumber);
                    command.Parameters.AddWithValue("@TeacherUsername", teacher.TeacherName);
                    command.Parameters.AddWithValue("@TeacherPassword", teacher.Password);
                    command.Parameters.AddWithValue("@AccessLevel", teacher.AccessLevel);  
                    command.Parameters.AddWithValue("@Department", teacher.Department);
                    command.Parameters.AddWithValue("@TeacherName_TeacherSurname", teacher.TeacherSurname); // isim + soyisim yazacak buraya


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "İşlem başarıyla kaydedildi.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: Kayıt eklenmedi.";
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda işlemler
                    TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu: " + ex.Message;
                    TempData["ErrorMessageDetail"] = ex.ToString(); // Daha detaylı hata bilgisi için
                }
            }
        }

        public ActionResult Student()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptid = (int)Session[SessionDeptIDKey]; // bunu ekledik
            List<AnnouncmentView> announcements = GetAnnouncements(deptid); // buradan fonksiyona verdik
            return View(announcements);

        }

        public ActionResult StudentAverage()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];


            int deptid = (int)Session[SessionDeptIDKey];
            int userid = (Session[SessionUserIDKey] != null) ? (int)Session[SessionUserIDKey] : 0;
            List<TeacherExam> examResults = GetStudentAverages(userid, deptid);

            double semesterAverage = CalculateSemesterAverage(examResults);

            ViewBag.SemesterAverage = semesterAverage;
            return View(examResults);
        }

        public List<TeacherExam> GetStudentAverages(int userid, int deptid)
        {
            List<TeacherExam> examResults = new List<TeacherExam>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT e.dersid, e.midtermresult, e.finalresult, l.lessonname 
                    FROM examresult e
                    INNER JOIN lesson l ON e.dersid = l.lessonid
                    WHERE e.userid = @userid";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userid", userid);
                    command.Parameters.AddWithValue("@deptid", deptid);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int midtermResult = Convert.ToInt32(reader["midtermresult"]);
                            int finalResult = Convert.ToInt32(reader["finalresult"]);

                            // Loglama (debug amaçlı)
                            System.Diagnostics.Debug.WriteLine($"Midterm: {midtermResult}, Final: {finalResult}");

                            TeacherExam result = new TeacherExam
                            {
                                lessonid = Convert.ToInt32(reader["dersid"]),
                                lessonname = reader["lessonname"].ToString(),
                                midtermresult = midtermResult,
                                finalresult = finalResult,
                                average = CalculateAverage(midtermResult, finalResult)
                            };
                            examResults.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sınav sonuçları alınırken bir hata oluştu: " + ex.Message;
                }
            }

            return examResults;
        }


        public ActionResult StudentExam()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptid = (int)Session[SessionDeptIDKey];
            int userid = (Session[SessionUserIDKey] != null) ? (int)Session[SessionUserIDKey] : 0;
            List<TeacherExam> examResults = GetStudentExams(userid, deptid);
            return View(examResults);
        }

        public List<TeacherExam> GetStudentExams(int userid, int deptid)
        {
            List<TeacherExam> examResults = new List<TeacherExam>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT e.dersid, e.midtermresult, e.finalresult, l.lessonname 
                    FROM examresult e
                    INNER JOIN lesson l ON e.dersid = l.lessonid
                    WHERE e.userid = @userid";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userid", userid);
                    command.Parameters.AddWithValue("@deptid", deptid);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TeacherExam result = new TeacherExam
                            {
                                lessonid = Convert.ToInt32(reader["dersid"]),
                                lessonname = reader["lessonname"].ToString(),
                                midtermresult = Convert.ToInt32(reader["midtermresult"]),
                                finalresult = Convert.ToInt32(reader["finalresult"]),
                                average = CalculateAverage(Convert.ToInt32(reader["midtermresult"]), Convert.ToInt32(reader["finalresult"]))
                            };
                            examResults.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sınav sonuçları alınırken bir hata oluştu: " + ex.Message;
                }
            }

            return examResults;
        }

        public double CalculateAverage(int midterm, int final)
        {
            double average = (midterm * 0.4) + (final * 0.6);
            // Loglama (debug amaçlı)
            System.Diagnostics.Debug.WriteLine($"Calculated Average: {average}");
            return average;
        }

        private double CalculateSemesterAverage(List<TeacherExam> examResults)
        {
            if (examResults.Count == 0) return 0;

            double totalAverage = examResults.Sum(exam => exam.average);
            return totalAverage / examResults.Count;
        }

        public List<StudentExamSchedule> GetStudentExamSchedules(int deptid)
        {
            List<StudentExamSchedule> stexamsch = new List<StudentExamSchedule>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                                e.dersid,
                                e.date,
                                e.class,
                                e.deptid,
                                l.lessonname
                            FROM 
                                examschedule e
                            JOIN 
                                lesson l ON e.dersid = l.lessonid
                            WHERE 
                                e.deptid = @deptid";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@deptid", deptid);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StudentExamSchedule stschedule = new StudentExamSchedule
                            {
                                dersid = Convert.ToInt32(reader["dersid"]),
                                date = DateTime.Parse(reader["date"].ToString()),
                                classs = reader["class"].ToString(),
                                lessonname = reader["lessonname"].ToString(),


                            };
                            stexamsch.Add(stschedule);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sınav sonuçları alınırken bir hata oluştu: " + ex.Message;
                }

                return stexamsch;
            }
        }

        public ActionResult StudentExamSchedule()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptid = (int)Session[SessionDeptIDKey];
            List<StudentExamSchedule> stschedule = GetStudentExamSchedules(deptid);
            return View(stschedule);
        }

        public List<ExamSchedule> GetSchedule(int deptid)
        {
            List<ExamSchedule> examsch = new List<ExamSchedule>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                                e.dersid,
                                e.date,
                                e.class,
                                e.deptid,
                                l.lessonname
                            FROM 
                                examschedule e
                            JOIN 
                                lesson l ON e.dersid = l.lessonid
                            WHERE 
                                e.deptid = @deptid";


                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@deptid", deptid);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExamSchedule schedule = new ExamSchedule
                            {
                                dersid = Convert.ToInt32(reader["dersid"]),
                                date = DateTime.Parse(reader["date"].ToString()),
                                classs = reader["class"].ToString(),
                                lessonname = reader["lessonname"].ToString(),


                            };
                            examsch.Add(schedule);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sınav sonuçları alınırken bir hata oluştu: " + ex.Message;
                }
            }

            return examsch;
        }

        public ActionResult TeacherExamSchedule()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            //int deptid = (int)Session[SessionDeptIDKey];
            List<ExamSchedule> schedule = GetSchedule(2);
            return View(schedule);
        }


        public ActionResult StudentAdd()
        {
            return View();
        }

        public ActionResult TeacherAdd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StudentAdd(Studentclass stu)
        {
            SaveStudent(stu);
            return View(stu);
        }

        [HttpPost]
        public ActionResult TeacherAdd(Teacherclass teacher)
        {
            SaveTeacher(teacher);
            return View(teacher);
        }

       
        public ActionResult TeacherSyllabus()
        {
            ViewBag.UserName = Session[SessionUserNameKey];
            ViewBag.UserID = Session[SessionUserIDKey];

            int deptID = (int)Session[SessionDeptIDKey];
            int userID = (int)Session[SessionUserIDKey];
            List<TeacherSyllabus> syllabus = GetTeacherSyllabus(deptID, userID);
            return View(syllabus);
        }

        public List<TeacherSyllabus> GetTeacherSyllabus(int deptID, int userID)
        {
            string connectionString = "Server=localhost;Database=university;Uid=root;Pwd=1234;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            List<TeacherSyllabus> syllabusList = new List<TeacherSyllabus>();

                try
                {
                    conn.Open();
                    string query = @"SELECT s.lessonid, l.deptid, u.userid, s.day, s.time, l.lessonname 
                             FROM syllabus s 
                             JOIN lesson l ON s.lessonid = l.lessonid 
                             JOIN usertable u ON l.userid = u.userid 
                             WHERE s.deptid = @deptid AND u.userid = @userid 
                             ORDER BY s.day, s.time";
                    MySqlCommand command = new MySqlCommand(query, conn);
                    command.Parameters.AddWithValue("@deptid", deptID);
                    command.Parameters.AddWithValue("@userid", userID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TeacherSyllabus s = new TeacherSyllabus
                            {
                                coursename = reader["lessonname"].ToString(),
                                day = reader["day"].ToString(),
                                time = reader["time"].ToString(),
                            };
                            syllabusList.Add(s);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                    TempData["ErrorMessage"] = "Ders programı alınırken bir hata oluştu: " + ex.Message;
                }
            

            return syllabusList;
        }

    }
}