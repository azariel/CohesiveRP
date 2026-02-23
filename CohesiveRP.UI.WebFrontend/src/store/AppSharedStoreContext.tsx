import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";

type SharedContextType = {
  /* Set the active module in the center component, which will trigger auto-rendering */
  activeModule: string;
  setActiveModule: (module: string) => void;
};

const SharedContext = createContext<SharedContextType | undefined>(undefined);

export const AppSharedStoreProvider = ({ children }: { children: ReactNode }) => {
  const [activeModule, setActiveModule] = useState<string>("chat");// default to 'chat' module

  return (
    <SharedContext.Provider value={{ activeModule, setActiveModule }}>
      {children}
    </SharedContext.Provider>
  );
};

export const sharedContext = () => {
  
  const context = useContext(SharedContext);
  if (!context) {
    throw new Error("sharedContext must be used within AppSharedStoreProvider");
  }

  return context;
};