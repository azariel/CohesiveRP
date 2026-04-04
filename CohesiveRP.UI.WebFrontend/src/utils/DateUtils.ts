const FormatUtcDate = (iso: string): string => {
    try {
        const date = new Date(iso);

        const datePart = new Intl.DateTimeFormat('en-US', {
            month: 'long',
            day: 'numeric',
            year: 'numeric',
            timeZone: 'UTC',
        }).format(date);

        const [h, m, s] = date.toISOString().slice(11, 19).split(':');

        return `${datePart} ${h}h${m}m${s}s`;
  }catch(err){
    return "unknown";
  }
};

const FormatDateTimeToMinutes = (date: string | Date | null | undefined): string => {
  if (!date) return "";
  const d = new Date(date);
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  const hh = String(d.getHours()).padStart(2, "0");
  const min = String(d.getMinutes()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd} ${hh}h${min}`;
};

export function ParseFocusedGenerationDate(raw: string | null | undefined): number | null {
  if (!raw)
    return null;

  let iso = raw.trim().replace(" ", "T");

  if (iso.startsWith("0001-01-01"))
    return null;

  // Force UTC if no timezone info is present
  if (!iso.endsWith("Z") && !iso.includes("+"))
    iso += "Z";

  const ms = Date.parse(iso);
  return isNaN(ms) ? null : ms;
}

const FormatDateTimeDurationMinutesAndSeconds = (ms: number | null | undefined): string => {
  if (ms === null || ms === undefined || isNaN(ms))
    return "-";
  const totalSeconds = Math.floor(ms / 1000);
  const min = Math.floor(totalSeconds / 60);
  const sec = totalSeconds % 60;
  const secStr = String(sec).padStart(2, "0");
  if (min > 0)
    return `${String(min).padStart(2, "0")}m${secStr}s`;
  return `${secStr}s`;
};


export {
    FormatUtcDate,
    FormatDateTimeToMinutes,
    FormatDateTimeDurationMinutesAndSeconds
};