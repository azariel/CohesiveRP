import type { SharedContextType } from "./SharedContextType";

interface SharedContextSettings extends SharedContextType {
  LLMProviders: string;//todo real model with collection of providers
};

export type {
  SharedContextSettings
};