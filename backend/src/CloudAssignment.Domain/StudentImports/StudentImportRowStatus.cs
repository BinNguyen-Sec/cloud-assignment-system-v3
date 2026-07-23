namespace CloudAssignment.Domain.StudentImports;

public enum StudentImportRowStatus
{
    Valid = 1,
    Invalid = 2,
    DuplicateInFile = 3,
    AlreadyEnrolled = 4,
    UserNotFound = 5,
    InactiveUser = 6,
    WrongRole = 7,
    Imported = 8
}
