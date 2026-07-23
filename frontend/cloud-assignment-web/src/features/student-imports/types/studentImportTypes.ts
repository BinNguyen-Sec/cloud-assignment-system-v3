import type { PagedResponse } from '../../../types/api';

export type StudentImportRowStatus =
  | 'Valid'
  | 'Invalid'
  | 'DuplicateInFile'
  | 'AlreadyEnrolled'
  | 'UserNotFound'
  | 'InactiveUser'
  | 'WrongRole'
  | 'Imported';

export interface StudentImportRow {
  rowNumber: number;
  studentCode: string | null;
  fullName: string | null;
  email: string | null;
  status: StudentImportRowStatus;
  message: string | null;
}

export interface StudentImportPreview {
  batchId: string;
  fileName: string;
  totalRows: number;
  validRows: number;
  invalidRows: number;
  expiresAtUtc: string;
  rows: StudentImportRow[];
}

export interface StudentImportConfirm {
  batchId: string;
  status: string;
  importedRows: number;
  skippedRows: number;
  completedAtUtc: string | null;
  rows: StudentImportRow[];
}

export interface StudentImportHistoryItem {
  batchId: string;
  courseId: string;
  courseCode: string;
  courseName: string;
  originalFileName: string;
  status: string;
  totalRows: number;
  validRows: number;
  invalidRows: number;
  importedRows: number;
  skippedRows: number;
  createdAtUtc: string;
  completedAtUtc: string | null;
  expiresAtUtc: string;
}

export type StudentImportHistoryPage = PagedResponse<StudentImportHistoryItem>;
