interface ErrorStateProps {
  title?: string;
  description: string;
}

export function ErrorState({
  title = 'Không thể hoàn thành yêu cầu',
  description,
}: ErrorStateProps) {
  return (
    <section className="feedback-state feedback-state--error" role="alert">
      <span className="feedback-state__symbol" aria-hidden="true">
        !
      </span>
      <h2>{title}</h2>
      <p>{description}</p>
    </section>
  );
}
