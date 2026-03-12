import { createContext, useContext, useState, useEffect } from "react";
import type { Dispatch, ReactNode, SetStateAction } from "react";
import type { SharedContextType } from "./SharedContextType";

type SharedContextWrapperType<T = SharedContextType> = {
  /* Set the active module in the center component, which will trigger auto-rendering */
  activeModule: T;
  setActiveModule: Dispatch<SetStateAction<T>>;
  navigateTo: (moduleName: T) => void;
};

const SharedContext = createContext<SharedContextWrapperType<any> | null>(null);

export const AppSharedStoreProvider = ({ children }: { children: ReactNode }) => {
  const [activeModule, setActiveModule] = useState<SharedContextType>(() => {
    try {
      // Initialize state from localStorage if available
      const pathModule = window.location.pathname.replace('/', '');
      if (pathModule) return { moduleName: pathModule };

      const saved = localStorage.getItem("activeModule");
      return saved ? JSON.parse(saved) : { moduleName: "chats" };
    } catch {
      return { moduleName: "chats" };
    }
  });

  // Persist to localStorage
  useEffect(() => {
    try {
      localStorage.setItem("activeModule", JSON.stringify(activeModule));
    } catch {
      console.error("Failed to save activeModule to localStorage.");
    }
  }, [activeModule]);

  // Listen for back/forward browser buttons
  useEffect(() => {
    const handlePopState = (event: PopStateEvent) => {
    const module = event.state ?? { moduleName: "chats" };
    setActiveModule(module);
  };

    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  // Use this instead of setActiveModule when navigating
  const navigateTo = (module: SharedContextType) => {
    window.history.pushState(module, "", `/${module.moduleName}`);
    setActiveModule(module as any);
  };

  return (
    <SharedContext.Provider value={{ activeModule, setActiveModule, navigateTo }}>
      {children}
    </SharedContext.Provider>
  );
};

export function sharedContext<T = SharedContextType>() {
  const context = useContext(SharedContext);
  if (!context) {
    console.error("Store shared context was null within AppSharedStoreProvider.");
    throw new Error("sharedContext must be used within AppSharedStoreProvider");
  }
  return context as SharedContextWrapperType<T>;
}
