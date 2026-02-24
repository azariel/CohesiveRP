import styles from "./UserInputComponent.module.css";
import { useRef, useState, useEffect  } from "react";
import { HiChip } from "react-icons/hi";
import { BiSolidPaperPlane, BiPaperPlane  } from "react-icons/bi";

interface Props {
  messagesRef?: React.RefObject<HTMLDivElement | null>;
}

export default function UserInputComponent({ messagesRef }: Props) {
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [hoveringSendBtn, setHoveringSendBtn] = useState(false);

  useEffect(() => {
    const textarea = textareaRef.current;
    const messagesContainer = messagesRef?.current;

    if (!textarea || !messagesContainer)
      return;

    const handleFocus = () => {
      setTimeout(() => {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
      }, 300); // 300ms is usually enough for keyboard animation
    };

    textarea.addEventListener("focus", handleFocus);
    return () => {
      textarea.removeEventListener("focus", handleFocus);
    };
  }, [messagesRef]);

  const handleInput = () => {
    const el = textareaRef.current;
    if (!el) {
      return;
    }

    el.style.marginBottom = "-0.3em";// hack...sigh
    el.style.height = "auto";
    el.style.height = Math.min(el.scrollHeight, 140) + "px";
    el.style.overflowY = el.scrollHeight > 140 ? "auto" : "hidden";

    // Scroll parent messages container to bottom
    if (messagesRef?.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  };

  return (
    <main className={styles.userInputComponent}>
      <div className={styles.inputContainer}>
        <HiChip className={styles.autoCorrectIcon} />
        <div className={styles.inputAutoCorrectSeparator} />
        <div className={styles.inputControlContainer}>
          <textarea className={styles.inputControl} rows={1} ref={textareaRef} onInput={handleInput} placeholder="Type a message..."/>
        </div>
        <div className={styles.inputSendSeparator} />
        <div onMouseEnter={() => setHoveringSendBtn(true)} onMouseLeave={() => setHoveringSendBtn(false)}>
          {hoveringSendBtn ? (
            <BiPaperPlane className={styles.sendInputIcon} />
          ) : (
            <BiSolidPaperPlane className={styles.sendInputIcon} />
          )}
        </div>
      </div>
    </main>
  );
}