export interface Persona {
  personaId: string;
  name: string;
  createdAtUtc: string;
  description: string;
  isDefault: boolean;
  lastActivityAtUtc: Date;
}