import styles from "./BackgroundQueriesStatusComponent.module.css";
import type { BackgroundQuery, BackgroundQueryStatus } from "../../../../../ResponsesDto/chat/BusinessObjects/BackgroundQuery";

interface Props {
  queries: BackgroundQuery[];
}

function getStatusDotClass(status: BackgroundQueryStatus): string {
  switch (status) {
    case "Pending": return styles.statusPending;
    case "InProgress": return styles.statusInProgress;
    case "ProcessedWaitingForFinalInstruction":
    case "ProcessingFinalInstruction":
      return styles.statusCyan;
    case "Error": return styles.statusError;
    default: return styles.statusOther; // unknown/future statuses -> orange, per spec
  }
}

function getStatusLabel(status: BackgroundQueryStatus): string {
  switch (status) {
    case "Pending": return "Pending";
    case "InProgress": return "In progress";
    case "ProcessedWaitingForFinalInstruction": return "Awaiting final instruction";
    case "ProcessingFinalInstruction": return "Finalizing";
    case "Error": return "Error";
    default: return status;
  }
}

export default function BackgroundQueriesStatusComponent({ queries }: Props) {
  const visibleQueries = queries.filter((q) => q.status !== "Completed");

  if (visibleQueries.length <= 0)
    return null;

  return (
    <div className={styles.backgroundQueriesStatusComponent}>
      {visibleQueries.map((query) => (
        <div
          key={query.backgroundQueryId}
          className={styles.statusPill}
          title={query.tags?.length ? `${getStatusLabel(query.status)} — ${query.tags.join(", ")}` : getStatusLabel(query.status)}
        >
          <span className={`${styles.statusDot} ${getStatusDotClass(query.status)}`} />
          <span className={styles.statusLabel}>{getStatusLabel(query.status)}</span>
        </div>
      ))}
    </div>
  );
}