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


export {
    FormatUtcDate
};