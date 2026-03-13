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


export {
    FormatUtcDate,
    FormatDateTimeToMinutes
};