namespace BLApi;

/// <summary>
/// Defines the operations available for managing courier entities within the system.
/// </summary>
/// <remarks>This interface provides methods for logging in, reading, updating, deleting, and creating courier
/// records. Implementations should ensure that appropriate authorization checks are performed based on the requesting
/// user's role.</remarks>
public interface ICourier
{
    BO.UserRole Login(int userId, string password);
    IEnumerable<BO.CourierInList> ReadAll(int requestingUserId, bool? isActive = null, BO.CourierFieldSort? sortBy = null);
    BO.Courier? Read(int requestingUserId);
    void Update(BO.Courier boCourier, int couruerId);
    void Delete(int requestingUserId, int courierId);
    void Create(int requestingUserId, BO.Courier boCourier);
}
/*
	void RegisterStudentToCourse(int studentId, int courseId);
	void UnRegisterStudentFromCourse(int studentId, int courseId);
 
	IEnumerable<BO.CourseInList> GetRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);
	IEnumerable<BO.CourseInList> GetUnRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);
 
	BO.StudentGradeSheet GetGradeSheetPerStudent(int studentId, BO.Year year = BO.Year.None);
	void UpdateGrade(int studentId, int courseId, double grade);

 */