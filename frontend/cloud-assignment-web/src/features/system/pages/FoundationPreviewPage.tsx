import { foundationNavigation } from '../../../app/navigation';
import { AcademyShell } from '../../../layouts/AcademyShell';

const foundations = [
  {
    symbol: '◈',
    title: 'Modular Monolith',
    detail: 'Domain, Application, Infrastructure và API có ranh giới rõ ràng.',
  },
  {
    symbol: '⌁',
    title: 'Cloud-agnostic',
    detail: 'Storage và nền tảng cloud được đặt sau interface của Application.',
  },
  {
    symbol: '✧',
    title: 'Feature-first UI',
    detail: 'Frontend được tổ chức theo module, không tạo dashboard all-in-one.',
  },
  {
    symbol: '◇',
    title: 'Quality Gate',
    detail: 'Build, type-check và test là điều kiện bắt buộc trước khi merge.',
  },
];

export function FoundationPreviewPage() {
  return (
    <AcademyShell navigation={foundationNavigation}>
      <div className="academy-page">
        <header className="academy-hero">
          <div>
            <p className="academy-kicker">Phase 1 · Foundation</p>
            <h2>Nền móng của học viện đã được dựng sạch.</h2>
            <p>
              Đây là màn hình kiểm chứng design system và cấu trúc frontend. Chưa có
              nút nghiệp vụ giả; Authentication sẽ được triển khai trọn bộ ở Phase 2.
            </p>
          </div>
          <div className="magic-seal" aria-label="Foundation ready">
            <span>V3</span>
            <small>READY</small>
          </div>
        </header>

        <section className="foundation-grid" aria-label="Các nguyên tắc nền tảng">
          {foundations.map((item) => (
            <article className="rune-card" key={item.title}>
              <span className="rune-card__symbol" aria-hidden="true">
                {item.symbol}
              </span>
              <h3>{item.title}</h3>
              <p>{item.detail}</p>
            </article>
          ))}
        </section>

        <section className="architecture-panel">
          <div>
            <p className="academy-kicker">Development topology</p>
            <h3>React → ASP.NET Core → PostgreSQL</h3>
            <p>
              Frontend gọi API qua proxy <code>/api</code>. PostgreSQL chạy trong Docker
              và API cung cấp live/readiness health checks riêng biệt.
            </p>
          </div>
          <dl className="architecture-status">
            <div>
              <dt>Frontend</dt>
              <dd>localhost:5173</dd>
            </div>
            <div>
              <dt>API</dt>
              <dd>localhost:8080</dd>
            </div>
            <div>
              <dt>Database</dt>
              <dd>localhost:5432</dd>
            </div>
          </dl>
        </section>
      </div>
    </AcademyShell>
  );
}
