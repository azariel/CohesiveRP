import React from 'react';
import styles from "./HighlightText.module.css";

interface HighlightedTextProps {
  text: string;
}

// Order matters: specific/longer patterns first
const MASTER_REGEX = /(\*\*[\s\S]*?\*\*|\*[\s\S]*?\*|<bold>[\s\S]*?<\/bold>|"[\s\S]*?")/g;

export const HighlightedText: React.FC<HighlightedTextProps> = ({ text }) => {
  if (!text) return <span>[empty]</span>;

  const parts = text.split(MASTER_REGEX);

  return (
    <>
      {parts.map((part, index) => {
        if (!part) return null;

        // Double Asterisks **text** -> Bold + Remove Tags
        if (part.startsWith('**') && part.endsWith('**')) {
          return <strong key={index} className={styles.boldHighlight}>{part.slice(2, -2)}</strong>;
        }

        // Single Asterisk *text* -> Italic + custom color, but brighter + Remove Tags
        if (part.startsWith('*') && part.endsWith('*')) {
          return <strong key={index} className={styles.thoughtsHighlight}>{part.slice(1, -1)}</strong>;
        }

        // Quotes "text" -> custom color + Keep Tags
        if (part.startsWith('"') && part.endsWith('"')) {
          return <span key={index} className={styles.quoteHighlight}>{part}</span>;
        }

        // Plain Text
        return part;
      })}
    </>
  );
};