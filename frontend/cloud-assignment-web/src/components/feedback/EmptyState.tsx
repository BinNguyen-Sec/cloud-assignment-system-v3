interface EmptyStateProps {
  title: string;
  description: string;
}

export function EmptyState({ title, description }: EmptyStateProps) {
  return (
    <section className="feedback-state" aria-live="polite">
      <span className="feedback-state__symbol" aria-hidden="true">
        ✧
      </span>
      <h2>{title}</h2>
      <p>{description}</p>
    </section>
  );
}
