using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.Courses;

public sealed class CourseMember : Entity
{
    private CourseMember()
    {
    }

    private CourseMember(
        Guid id,
        Guid courseId,
        Guid studentId,
        EnrollmentSource enrollmentSource,
        Guid? importBatchId,
        DateTimeOffset enrolledAtUtc)
        : base(id)
    {
        CourseId = RequireId(courseId, nameof(courseId));
        StudentId = RequireId(studentId, nameof(studentId));
        EnrollmentSource = enrollmentSource;
        ImportBatchId = importBatchId;
        EnrolledAtUtc = enrolledAtUtc.ToUniversalTime();
    }

    public Guid CourseId { get; private set; }

    public Guid StudentId { get; private set; }

    public EnrollmentSource EnrollmentSource { get; private set; }

    public Guid? ImportBatchId { get; private set; }

    public DateTimeOffset EnrolledAtUtc { get; private set; }

    public static CourseMember Create(
        Guid id,
        Guid courseId,
        Guid studentId,
        EnrollmentSource enrollmentSource,
        Guid? importBatchId,
        DateTimeOffset enrolledAtUtc) =>
        new(id, courseId, studentId, enrollmentSource, importBatchId, enrolledAtUtc);

    private static Guid RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }

        return value;
    }
}
