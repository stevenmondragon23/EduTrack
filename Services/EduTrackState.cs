namespace EduTrack.Services;

public class EduTrackState
{
    public record TeacherItem(int Id, string Name, string Email, string Specialty, bool Active);
    public record SubjectItem(int Id, string Code, string Name, int? TeacherId, string TeacherName);
    public record StudentItem(int Id, string Name, string Email, string Section, bool Active);
    public record EnrollmentItem(int Id, int StudentId, string StudentName, int SubjectId, string SubjectName);
    public record ActivityItem(int Id, int SubjectId, string Title, string Area, decimal MaxScore);
    public record GradeItem(int ActivityId, int StudentId, decimal Score, bool IsSubmitted);

    private int _nextTeacherId = 1;
    private int _nextSubjectId = 1;
    private int _nextStudentId = 1;
    private int _nextEnrollmentId = 1;
    private int _nextActivityId = 1;

   
    public string ActiveRole { get; set; } = "Student";
    public int ActiveStudentId { get; set; } = 1;

    public List<TeacherItem> Teachers { get; } = new();
    public List<SubjectItem> Subjects { get; } = new();
    public List<StudentItem> Students { get; } = new();
    public List<EnrollmentItem> Enrollments { get; } = new();
    public List<ActivityItem> Activities { get; } = new();
    public List<GradeItem> Grades { get; } = new();

    public EduTrackState()
    {
        SeedDemoData();
    }

    private void SeedDemoData()
    {
        AddTeacher("Jane Doe", "jane@edutrack.edu", "Mathematics");
        AddSubject("MATH101", "Mathematics", 1);

        AddStudent("Alex Rivera", "alex@edutrack.edu", "Section A");
        AddStudent("Sam Taylor", "sam@edutrack.edu", "Section A");

        EnrollStudent(1, 1);
        EnrollStudent(2, 1);
    }

    // --- Teachers ---
    public void AddTeacher(string name, string email, string specialty)
    {
        Teachers.Add(new TeacherItem(_nextTeacherId++, name, email, specialty, true));
    }

    public void UpdateTeacher(int id, string name, string email, string specialty)
    {
        var idx = Teachers.FindIndex(t => t.Id == id);
        if (idx != -1)
        {
            Teachers[idx] = Teachers[idx] with { Name = name, Email = email, Specialty = specialty };
        }
    }

    public void DeleteTeacher(int id)
    {
        Teachers.RemoveAll(t => t.Id == id);
    }

    // --- Subjects ---
    public void AddSubject(string code, string name, int teacherId)
    {
        var teacher = Teachers.FirstOrDefault(t => t.Id == teacherId);
        var teacherLabel = teacher?.Name ?? "Unassigned";
        Subjects.Add(new SubjectItem(_nextSubjectId++, code, name, teacherId, teacherLabel));
    }

    public void UpdateSubject(int id, string code, string name, int teacherId)
    {
        var idx = Subjects.FindIndex(s => s.Id == id);
        if (idx != -1)
        {
            var teacher = Teachers.FirstOrDefault(t => t.Id == teacherId);
            var teacherLabel = teacher?.Name ?? "Unassigned";
            Subjects[idx] = Subjects[idx] with
            {
                Code = code,
                Name = name,
                TeacherId = teacherId,
                TeacherName = teacherLabel
            };
        }
    }

    public void DeleteSubject(int id)
    {
        Subjects.RemoveAll(s => s.Id == id);
    }

    // --- Students ---
    public void AddStudent(string name, string email, string section)
    {
        Students.Add(new StudentItem(_nextStudentId++, name, email, section, true));
    }

    public void UpdateStudent(int id, string name, string email, string section)
    {
        var idx = Students.FindIndex(s => s.Id == id);
        if (idx != -1)
        {
            Students[idx] = Students[idx] with { Name = name, Email = email, Section = section };
        }
    }

    public void DeleteStudent(int id)
    {
        Students.RemoveAll(s => s.Id == id);
    }

    // --- Enrollments ---
    public void EnrollStudent(int studentId, int subjectId)
    {
        if (Enrollments.Any(e => e.StudentId == studentId && e.SubjectId == subjectId))
            return;

        var student = Students.FirstOrDefault(s => s.Id == studentId);
        var subject = Subjects.FirstOrDefault(s => s.Id == subjectId);
        if (student is null || subject is null) return;

        Enrollments.Add(new EnrollmentItem(_nextEnrollmentId++, studentId, student.Name, subjectId, subject.Name));
    }

    public void RemoveEnrollment(int id)
    {
        Enrollments.RemoveAll(e => e.Id == id);
    }

    // --- Activities & Grades ---
    public ActivityItem AddActivity(int subjectId, string title, string area, decimal maxScore)
    {
        var activity = new ActivityItem(_nextActivityId++, subjectId, title, area, maxScore);
        Activities.Add(activity);
        return activity;
    }

    public void SubmitGrade(int activityId, int studentId, decimal score)
    {
        
        var existing = Grades.FirstOrDefault(g => g.ActivityId == activityId && g.StudentId == studentId);
        if (existing is not null && existing.IsSubmitted)
            return;

        Grades.Add(new GradeItem(activityId, studentId, score, true));
    }

    // --- Calculations ---
    public decimal GetStudentSubjectAverage(int studentId, int subjectId)
    {
        var subjectActivityIds = Activities
            .Where(a => a.SubjectId == subjectId)
            .Select(a => a.Id)
            .ToHashSet();

        var studentGrades = Grades
            .Where(g => g.StudentId == studentId && g.IsSubmitted && subjectActivityIds.Contains(g.ActivityId))
            .ToList();

        if (studentGrades.Count == 0)
            return 0;

        var percentages = new List<decimal>();
        foreach (var grade in studentGrades)
        {
            var activity = Activities.FirstOrDefault(a => a.Id == grade.ActivityId);
            if (activity is not null && activity.MaxScore > 0)
            {
                percentages.Add((grade.Score / activity.MaxScore) * 100m);
            }
        }

        return percentages.Count == 0 ? 0 : Math.Round(percentages.Average(), 1);
    }
}