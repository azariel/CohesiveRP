import React from 'react';
import styles from "./HighlightText.module.css";

interface HighlightedTextProps {
  text: string;
}

const MASTER_REGEX = /(\*\*[\s\S]*?\*\*|\*[\s\S]*?\*|"[\s\S]*?")/g;
const INNER_ASTERISK_REGEX = /(\*[\s\S]*?\*)/g;

function renderQuoteContent(part: string, outerIndex: number) {
  // part includes the surrounding quotes, e.g. "I hate you—*ah*—"
  const inner = part.slice(1, -1); // strip surrounding quotes
  const innerParts = inner.split(INNER_ASTERISK_REGEX);

  return (
    <span key={outerIndex} className={styles.quoteHighlight}>
      {'"'}
      {innerParts.map((p, i) => {
        if (p.startsWith('*') && p.endsWith('*') && p.length > 2) {
          return <strong key={i} className={styles.boldQuoteHighlight}>{p.slice(1, -1)}</strong>;
        }
        return p;
      })}
      {'"'}
    </span>
  );
}

export const HighlightedText: React.FC<HighlightedTextProps> = ({ text }) => {
  if (!text) return <span>[empty]</span>;

  const parts = text.split(MASTER_REGEX);

  return (
    <>
      {parts.map((part, index) => {
        if (!part) return null;

        // **text** -> Bold + italic + colored #83d2ff + remove tags
        if (part.startsWith('**') && part.endsWith('**')) {
          return <strong key={index} className={styles.boldHighlight}>{part.slice(2, -2)}</strong>;
        }

        // *text* -> Italic + colored #83d2ff + remove tags
        if (part.startsWith('*') && part.endsWith('*')) {
          return <em key={index} className={styles.thoughtsHighlight}>{part.slice(1, -1)}</em>;
        }

        // "text" or "text with *bold* inside" -> colored #2faaf1, inner *x* bolded
        if (part.startsWith('"') && part.endsWith('"')) {
          return renderQuoteContent(part, index);
        }

        return part;
      })}
    </>
  );
};