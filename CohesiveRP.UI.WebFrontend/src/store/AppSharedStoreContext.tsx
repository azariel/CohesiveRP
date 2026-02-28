import { createContext, useContext, useState, useEffect  } from "react";
import type { ReactNode } from "react";
import type { SharedContextType } from "./SharedContextType";

type SharedContextWrapperType<T = SharedContextType> = {
  /* Set the active module in the center component, which will trigger auto-rendering */
  activeModule: T;
  setActiveModule: (module: T) => void;
};

const SharedContext = createContext<SharedContextWrapperType<any> | null>(null);

export const AppSharedStoreProvider = ({ children }: { children: ReactNode }) => {
    // Initialize state from localStorage if available
    const [activeModule, setActiveModule] = useState<SharedContextType>(() => {
      try {
        const saved = localStorage.getItem("activeModule");
        return saved ? JSON.parse(saved) : { moduleName: "chats" };
      } catch {
        return { moduleName: "chats" };
    }
  });

  // Persist state to localStorage whenever it changes
  useEffect(() => {
    try {
      localStorage.setItem("activeModule", JSON.stringify(activeModule));
    } catch {
      console.error("Failed to save activeModule to localStorage.");
    }
  }, [activeModule]);

  return (
    <SharedContext.Provider value={{ activeModule, setActiveModule }}>
      {children}
    </SharedContext.Provider>
  );
};

export function sharedContext<T = SharedContextType>() {
  const context = useContext(SharedContext);
  if (!context) {
    console.error('Store shared context was null within AppSharedStoreProvider.');
    throw new Error("sharedContext must be used within AppSharedStoreProvider");
  }
  return context as SharedContextWrapperType<T>;
}
