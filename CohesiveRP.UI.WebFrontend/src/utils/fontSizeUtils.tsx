const GetChatNameFontSize = (name: string): string => {
  const len = name.length;
  if (len <= 8) return "0.9em";
  if (len <= 10) return "0.8em";
  if (len <= 12) return "0.7em";
  if (len <= 14) return "0.6em";
  return "0.5em";
};

const GetCharacterDetailsNameFontSize = (name: string): string => {
  const len = name.length;
  if (len <= 8) return "1.2em";
  if (len <= 10) return "1.1em";
  if (len <= 12) return "1.0em";
  if (len <= 14) return "0.9em";
  if (len <= 16) return "0.8em";
  return "0.7em";
};

export {
    GetChatNameFontSize,
    GetCharacterDetailsNameFontSize
};