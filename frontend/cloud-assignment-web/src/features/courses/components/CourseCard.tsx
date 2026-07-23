import { Link } from 'react-router-dom';
import type { UserRole } from '../../auth/types/authTypes';
import type { CourseSummary } from '../types/courseTypes';

interface CourseCardProps {
  course: CourseSummary;
  role: UserRole;
}

export function CourseCard({ course, role }: CourseCardProps) {
  return (
    <article className={`course-grimoire course-theme--${course.themeKey}`}>
      <div className="course-grimoire__constellation" aria-hidden="true">✦ · ✧ · ◇</div>
      <div className="course-grimoire__header">
        <span className="course-code">{course.code}</span>
        <span className={`status-sigil ${course.isArchived ? 'status-sigil--muted' : 'status-sigil--active'}`}>
          {course.isArchived ? 'Đã lưu trữ' : 'Đang hoạt động'}
        </span>
      </div>
      <h3>{course.name}</h3>
      <p className="course-grimoire__description">
        {course.description || 'Môn học chưa có mô tả.'}
      </p>
      <dl className="course-grimoire__meta">
        <div><dt>Giảng viên</dt><dd>{course.teacherName}</dd></div>
        <div><dt>Học kỳ</dt><dd>{course.semester || 'Chưa đặt'}</dd></div>
        <div><dt>Sinh viên</dt><dd>{course.studentCount}</dd></div>
      </dl>
      <Link className="magic-button magic-button--secondary" to={`/${role.toLowerCase()}/courses/${course.id}`}>
        Mở học phần
      </Link>
    </article>
  );
}
