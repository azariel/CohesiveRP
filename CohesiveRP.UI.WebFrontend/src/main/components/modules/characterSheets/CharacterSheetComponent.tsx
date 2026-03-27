import styles from "./CharacterSheetComponent.module.css";
import { useState, useEffect, useRef } from "react";
import { ImSpinner2 } from "react-icons/im";
import { getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterSheetResponseDto } from "../../../../ResponsesDto/characters/characterSheets/CharacterSheetResponseDto";
import type { ServerApiResponseDto } from "../../../../ResponsesDto/ServerApiResponseDto";
import type { CharacterSheetRequestDto } from "../../../../RequestDto/characters/characterSheets/CharacterSheetRequestDto";

/* ─────────────────────────── Constants ─────────────────────── */

const GENDER_OPTIONS = ["", "Male", "Female"];
const AGE_GROUP_OPTIONS = ["", "Child", "Teenager", "YoungAdult", "Adult", "MiddleAged", "Elderly"];
const GENITALS_OPTIONS = ["", "Male", "Female", "Both", "None"];
const BREASTS_SIZE_OPTIONS = ["", "Flat", "Small", "Average", "Large", "ExtraLarge"];

const PATHFINDER_ATTR_KEYS = [
  "Fortitude", "Reflex", "Willpower", "Stamina",
  "MagicalStamina", "MagicalPower", "Intelligence", "Discernment", "Perception",
];

const PATHFINDER_SKILL_KEYS = [
  "Sex", "Acrobatics", "Athletics", "Deception",
  "Charisma", "Intimidation", "Medicine", "Performance",
  "Society", "Aristocracy", "Stealth", "Thievery",
];

const defaultAttrMap = (): Record<string, number> =>
  Object.fromEntries(PATHFINDER_ATTR_KEYS.map((k) => [k, 10]));

const defaultSkillMap = (): Record<string, number> =>
  Object.fromEntries(PATHFINDER_SKILL_KEYS.map((k) => [k, 10]));

/* ─────────────────────────── Helpers ───────────────────────── */

const arrToText = (arr?: string[]) => (arr ?? []).join("\n");
const textToArr = (text: string) =>
  text.split("\n").map((s) => s.trim()).filter(Boolean);

/* ─────────────────────────── Sub-components ─────────────────── */

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className={styles.section}>
      <h3 className={styles.sectionTitle}>{title}</h3>
      <div className={styles.sectionBody}>{children}</div>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className={styles.field}>
      <label className={styles.fieldLabel}>{label}</label>
      {children}
    </div>
  );
}

function SheetInput({
  value,
  onChange,
  placeholder,
}: {
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
}) {
  return (
    <input
      className={styles.sheetInput}
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
    />
  );
}

function SheetTextarea({
  value,
  onChange,
  minHeight = "5em",
}: {
  value: string;
  onChange: (v: string) => void;
  minHeight?: string;
}) {
  return (
    <textarea
      className={styles.sheetTextarea}
      style={{ minHeight }}
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}

function SheetSelect({
  value,
  onChange,
  options,
}: {
  value: string;
  onChange: (v: string) => void;
  options: string[];
}) {
  return (
    <select
      className={styles.sheetSelect}
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
    >
      {options.map((opt) => (
        <option key={opt} value={opt}>
          {opt || "— none —"}
        </option>
      ))}
    </select>
  );
}

function ArrayField({
  label,
  value,
  onChange,
  minHeight = "6em",
}: {
  label: string;
  value: string[];
  onChange: (v: string[]) => void;
  minHeight?: string;
}) {
  const [localText, setLocalText] = useState(() => arrToText(value));

  useEffect(() => {
    setLocalText((prevText) => {
      const parsedLocal = textToArr(prevText);
      // Compare arrays to see if the parent was updated by the server/import
      if (JSON.stringify(parsedLocal) !== JSON.stringify(value)) {
        return arrToText(value);
      }
      return prevText;
    });
  }, [value]);

  const handleTextChange = (v: string) => {
    setLocalText(v);
    onChange(textToArr(v));
  };

  return (
    <Field label={`${label} (one per line)`}>
      <SheetTextarea
        value={localText}
        onChange={handleTextChange}
        minHeight={minHeight}
      />
    </Field>
  );
}

/* ─────────────────────────── Main component ─────────────────── */

interface Props {
  personaId: string | null;
  characterId: string | null;
}

export default function CharacterSheetComponent({ characterId, personaId }: Props) {
  const didMount = useRef(false);
  const importFileRef = useRef<HTMLInputElement>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isRegenerating, setIsRegenerating] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [regenerateError, setRegenerateError] = useState(false);

  const [characterSheetId, setCharacterSheetId] = useState<string | null>("");

  /* ── identity ── */
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [birthday, setBirthday] = useState("");
  const [gender, setGender] = useState("");
  const [ageGroup, setAgeGroup] = useState("");

  /* ── appearance ── */
  const [race, setRace] = useState("");
  const [height, setHeight] = useState("");
  const [bodyType, setBodyType] = useState("");
  const [hairColor, setHairColor] = useState("");
  const [hairStyle, setHairStyle] = useState("");
  const [eyeColor, setEyeColor] = useState("");
  const [earShape, setEarShape] = useState("");
  const [skinColor, setSkinColor] = useState("");
  const [genitals, setGenitals] = useState("");
  const [breastsSize, setBreastsSize] = useState("");
  const [penisSize, setPenisSize] = useState("");
  const [sexuality, setSexuality] = useState("");
  const [attractiveness, setAttractiveness] = useState("");

  /* ── voice & manner ── */
  const [speechPattern, setSpeechPattern] = useState("");
  const [speechImpairment, setSpeechImpairment] = useState("");
  const [mannerisms, setMannerisms] = useState("");
  const [socialAnxiety, setSocialAnxiety] = useState("");
  const [clothesPreference, setClothesPreference] = useState("");

  /* ── background ── */
  const [profession, setProfession] = useState("");
  const [reputation, setReputation] = useState("");
  const [relationships, setRelationships] = useState<string[]>([]);

  /* ── personality ── */
  const [behavior, setBehavior] = useState("");
  const [personalityTraits, setPersonalityTraits] = useState<string[]>([]);
  const [likes, setLikes] = useState<string[]>([]);
  const [dislikes, setDislikes] = useState<string[]>([]);
  const [fears, setFears] = useState<string[]>([]);
  const [secrets, setSecrets] = useState<string[]>([]);

  /* ── combat ── */
  const [preferredCombatStyle, setPreferredCombatStyle] = useState("");
  const [weaponsProficiency, setWeaponsProficiency] = useState("");
  const [combatAffinityAttack, setCombatAffinityAttack] = useState("");
  const [combatAffinityDefense, setCombatAffinityDefense] = useState("");
  const [skills, setSkills] = useState<string[]>([]);
  const [weaknesses, setWeaknesses] = useState<string[]>([]);

  /* ── goals ── */
  const [goalsForNextYear, setGoalsForNextYear] = useState<string[]>([]);
  const [longTermGoals, setLongTermGoals] = useState<string[]>([]);

  /* ── adult ── */
  const [kinks, setKinks] = useState<string[]>([]);
  const [secretKinks, setSecretKinks] = useState<string[]>([]);

  /* ── pathfinder ── */
  const [attrMap, setAttrMap] = useState<Record<string, number>>(defaultAttrMap());
  const [skillMap, setSkillMap] = useState<Record<string, number>>(defaultSkillMap());

  /* ─── shared sheet hydration ─── */
  const hydrateSheet = (s: CharacterSheetResponseDto) => {
    console.log(`A: setting to ${s.characterSheetId ?? null}`);
    setCharacterSheetId(s.characterSheetId ?? null);
    setFirstName(s.characterSheet?.firstName ?? "");
    setLastName(s.characterSheet?.lastName ?? "");
    setBirthday(s.characterSheet?.birthday ?? "");
    setGender(s.characterSheet?.gender ?? "");
    setAgeGroup(s.characterSheet?.ageGroup ?? "");
    setRace(s.characterSheet?.race ?? "");
    setHeight(s.characterSheet?.height ?? "");
    setBodyType(s.characterSheet?.bodyType ?? "");
    setHairColor(s.characterSheet?.hairColor ?? "");
    setHairStyle(s.characterSheet?.hairStyle ?? "");
    setEyeColor(s.characterSheet?.eyeColor ?? "");
    setEarShape(s.characterSheet?.earShape ?? "");
    setSkinColor(s.characterSheet?.skinColor ?? "");
    setGenitals(s.characterSheet?.genitals ?? "");
    setBreastsSize(s.characterSheet?.breastsSize ?? "");
    setPenisSize(s.characterSheet?.penisSize ?? "");
    setSexuality(s.characterSheet?.sexuality ?? "");
    setAttractiveness(s.characterSheet?.attractiveness ?? "");
    setSpeechPattern(s.characterSheet?.speechPattern ?? "");
    setSpeechImpairment(s.characterSheet?.speechImpairment ?? "");
    setMannerisms(s.characterSheet?.mannerisms ?? "");
    setSocialAnxiety(s.characterSheet?.socialAnxiety ?? "");
    setClothesPreference(s.characterSheet?.clothesPreference ?? "");
    setProfession(s.characterSheet?.profession ?? "");
    setReputation(s.characterSheet?.reputation ?? "");
    setRelationships(s.characterSheet?.relationships ?? []);
    setBehavior(s.characterSheet?.behavior ?? "");
    setPersonalityTraits(s.characterSheet?.personalityTraits ?? []);
    setLikes(s.characterSheet?.likes ?? []);
    setDislikes(s.characterSheet?.dislikes ?? []);
    setFears(s.characterSheet?.fears ?? []);
    setSecrets(s.characterSheet?.secrets ?? []);
    setPreferredCombatStyle(s.characterSheet?.preferredCombatStyle ?? "");
    setWeaponsProficiency(s.characterSheet?.weaponsProficiency ?? "");
    setCombatAffinityAttack(s.characterSheet?.combatAffinityAttack ?? "");
    setCombatAffinityDefense(s.characterSheet?.combatAffinityDefense ?? "");
    setSkills(s.characterSheet?.skills ?? []);
    setWeaknesses(s.characterSheet?.weaknesses ?? []);
    setGoalsForNextYear(s.characterSheet?.goalsForNextYear ?? []);
    setLongTermGoals(s.characterSheet?.longTermGoals ?? []);
    setKinks(s.characterSheet?.kinks ?? []);
    setSecretKinks(s.characterSheet?.secretKinks ?? []);

    if (s.characterSheet?.pathfinderAttributes?.length) {
      const map = { ...defaultAttrMap() };
      s.characterSheet.pathfinderAttributes.forEach((a) => { map[a.attributeType] = a.value; });
      setAttrMap(map);
    }
    if (s.characterSheet?.pathfinderSkills?.length) {
      const map = { ...defaultSkillMap() };
      s.characterSheet.pathfinderSkills.forEach((a) => { map[a.skillType] = a.value; });
      setSkillMap(map);
    }
  };

  /* ─── fetch ─── */
  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;

    const fetchSheet = async () => {
      try {
        let response: CharacterSheetResponseDto | null;

        console.log(`characterId:${characterId} personaId:${personaId}`);
        if (characterId) {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/${characterId}/characterSheet`);
        } else {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/personaCharacterSheet/${personaId}`);
        }

        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || ex?.message) {
          if (ex?.code !== 404) {
            console.error("Failed to load character sheet:", ex);
            setLoadError(true);
            return;
          }
        }

        hydrateSheet(response as CharacterSheetResponseDto);
      } catch (err) {
        console.error("Fetch character sheet error:", err);
        setLoadError(true);
      } finally {
        setIsLoading(false);
      }
    };

    fetchSheet();
  }, [characterId]);

  /* ─── regenerate ─── */
  const handleRegenerate = async () => {
    if (isRegenerating || (!characterId && !personaId))
      return;

    setIsRegenerating(true);
    setRegenerateError(false);

    try {

      let characterSheetIdToUse = characterSheetId;
      if(!characterSheetIdToUse) {
        // Save the characterSheet as-is and THEN regenerate it
        characterSheetIdToUse = await handleSave();
      }

      if(!characterSheetIdToUse) {
        console.error(`Couldn't regenerate the CharacterSheet, the operation failed as the CharacterSheetId is null.`);
        return;
      }

      const payload = {
        characterId,
        personaId,
        characterSheetIdToUse,
      };

      console.log(`Regenerating CharacterSheet.`);

      let response = null;
      if (characterId) {
        response = await postToServerApiAsync<CharacterSheetResponseDto>(`api/characters/${characterId}/characterSheet/${characterSheetIdToUse}/regenerate`, payload);
      } else {
        response = await postToServerApiAsync<CharacterSheetResponseDto>(`api/characters/personaCharacterSheet/${personaId}/regenerate`, payload);
      }

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Regenerate character sheet failed:", ex);
        setRegenerateError(true);
        return;
      }

      hydrateSheet(response as CharacterSheetResponseDto);
    } catch (err) {
      console.error("Regenerate character sheet error:", err);
      setRegenerateError(true);
    } finally {
      setIsRegenerating(false);
    }
  };

  /* ─── save ─── */
  const handleSave = async (): Promise<string | null> => {
    if (isSaving)
      return null;

    setIsSaving(true);
    setSaveError(false);
    setSaveSuccess(false);

    try {
      const payload: CharacterSheetRequestDto = {
        characterId: characterId,
        personaId: personaId,
        characterSheetId: characterSheetId,
        characterSheet: {
          firstName,
          lastName,
          birthday: birthday || null,
          gender: gender || null,
          ageGroup: ageGroup || null,
          race,
          height,
          bodyType,
          hairColor,
          hairStyle,
          eyeColor,
          earShape,
          skinColor,
          genitals: genitals || null,
          breastsSize: breastsSize || null,
          penisSize: penisSize || null,
          sexuality: sexuality || null,
          attractiveness,
          speechPattern,
          speechImpairment,
          mannerisms,
          socialAnxiety,
          clothesPreference,
          profession,
          reputation,
          relationships,
          behavior,
          personalityTraits,
          likes,
          dislikes,
          fears,
          secrets,
          preferredCombatStyle,
          weaponsProficiency,
          combatAffinityAttack,
          combatAffinityDefense,
          skills,
          weaknesses,
          goalsForNextYear,
          longTermGoals,
          kinks,
          secretKinks,
          pathfinderAttributes: PATHFINDER_ATTR_KEYS.map((k) => ({
            attributeType: k,
            value: attrMap[k] ?? 10,
          })),
          pathfinderSkills: PATHFINDER_SKILL_KEYS.map((k) => ({
            skillType: k,
            value: skillMap[k] ?? 10,
          })),
        }
      };

      let response: ServerApiResponseDto | null;
      if (characterSheetId && characterId) {
        response = await putToServerApiAsync(`api/characters/${characterId}/characterSheet/${characterSheetId}`, payload);
      } else if(characterId) {
        response = await postToServerApiAsync(`api/characters/${characterId}/characterSheet`, payload);
      } else if(characterSheetId && personaId) {
        response = await putToServerApiAsync(`api/characters/${personaId}/characterSheet/${characterSheetId}`, payload);
      } else {
        response = await postToServerApiAsync(`api/characters/${personaId}/characterSheet`, payload);
      }

      const ex = response as ServerApiExceptionResponseDto | null;
      const typedResponse = response as CharacterSheetResponseDto;
      if (!response || ex?.message || !typedResponse || !typedResponse.characterSheetId) {
        console.error("Save character sheet failed:", ex);
        console.error(`current CharacterSheetId:${characterSheetId}`);
        setSaveError(true);
      } else {
        console.log(`B: setting to ${typedResponse.characterSheetId ?? null}`);
        setCharacterSheetId(typedResponse.characterSheetId ?? null);
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 5000);
        return typedResponse.characterSheetId ?? null;
      }
    } catch (err) {
      console.error("Save character sheet error:", err);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }

    return null;
  };

  /* ─── export ─── */
  const handleExportJson = () => {
    const exportPayload = {
        firstName,
        lastName,
        birthday: birthday || null,
        gender: gender || null,
        ageGroup: ageGroup || null,
        race,
        height,
        speechPattern,
        speechImpairment,
        bodyType,
        hairColor,
        hairStyle,
        eyeColor,
        earShape,
        skinColor,
        genitals: genitals || null,
        breastsSize: breastsSize || null,
        sexuality: sexuality || null,
        relationships,
        profession,
        reputation,
        preferredCombatStyle,
        weaponsProficiency,
        combatAffinityAttack,
        combatAffinityDefense,
        socialAnxiety,
        clothesPreference,
        mannerisms,
        behavior,
        attractiveness,
        kinks,
        secretKinks,
        skills,
        weaknesses,
        fears,
        likes,
        dislikes,
        secrets,
        personalityTraits,
        goalsForNextYear,
        longTermGoals,
        pathfinderAttributes: PATHFINDER_ATTR_KEYS.map((k) => ({ attributeType: k, value: attrMap[k] ?? 10 })),
        pathfinderSkills: PATHFINDER_SKILL_KEYS.map((k) => ({ skillType: k, value: skillMap[k] ?? 10 })),
    };

    const blob = new Blob([JSON.stringify(exportPayload, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    let name = `${firstName || "character"}`;
    if(lastName && lastName != ""){
      name += `_${lastName}`;
    }

    anchor.download = `${name}_CharacterSheet.json`;
    anchor.click();
    URL.revokeObjectURL(url);
  };

  /* ─── import ─── */
  const handleImportJson = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = async (event) => {
      try {
        const parsed = JSON.parse(event.target?.result as string);
        const s = parsed.characterSheet ?? parsed;

        setFirstName(s.firstName ?? "");
        setLastName(s.lastName ?? "");
        setBirthday(s.birthday ?? "");
        setGender(s.gender ?? "");
        setAgeGroup(s.ageGroup ?? "");
        setRace(s.race ?? "");
        setHeight(s.height ?? "");
        setBodyType(s.bodyType ?? "");
        setHairColor(s.hairColor ?? "");
        setHairStyle(s.hairStyle ?? "");
        setEyeColor(s.eyeColor ?? "");
        setEarShape(s.earShape ?? "");
        setSkinColor(s.skinColor ?? "");
        setGenitals(s.genitals ?? "");
        setBreastsSize(s.breastsSize ?? "");
        setSexuality(s.sexuality ?? "");
        setAttractiveness(s.attractiveness ?? "");
        setSpeechPattern(s.speechPattern ?? "");
        setSpeechImpairment(s.speechImpairment ?? "");
        setMannerisms(s.mannerisms ?? "");
        setSocialAnxiety(s.socialAnxiety ?? "");
        setClothesPreference(s.clothesPreference ?? "");
        setProfession(s.profession ?? "");
        setReputation(s.reputation ?? "");
        setRelationships(s.relationships ?? []);
        setBehavior(s.behavior ?? "");
        setPersonalityTraits(s.personalityTraits ?? []);
        setLikes(s.likes ?? []);
        setDislikes(s.dislikes ?? []);
        setFears(s.fears ?? []);
        setSecrets(s.secrets ?? []);
        setPreferredCombatStyle(s.preferredCombatStyle ?? "");
        setWeaponsProficiency(s.weaponsProficiency ?? "");
        setCombatAffinityAttack(s.combatAffinityAttack ?? "");
        setCombatAffinityDefense(s.combatAffinityDefense ?? "");
        setSkills(s.skills ?? []);
        setWeaknesses(s.weaknesses ?? []);
        setGoalsForNextYear(s.goalsForNextYear ?? []);
        setLongTermGoals(s.longTermGoals ?? []);
        setKinks(s.kinks ?? []);
        setSecretKinks(s.secretKinks ?? []);

        if (s.pathfinderAttributes?.length) {
          const map = { ...defaultAttrMap() };
          s.pathfinderAttributes.forEach((a: { attributeType: string; value: number }) => { map[a.attributeType] = a.value; });
          setAttrMap(map);
        }
        if (s.pathfinderSkills?.length) {
          const map = { ...defaultSkillMap() };
          s.pathfinderSkills.forEach((a: { skillType: string; value: number }) => { map[a.skillType] = a.value; });
          setSkillMap(map);
        }

        e.target.value = "";
      } catch (err) {
        console.error("Failed to parse imported sheet JSON:", err);
        setSaveError(true);
      }
    };
    reader.readAsText(file);
  };

  /* ─── render ─── */
  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <ImSpinner2 className={styles.spinner} />
      </div>
    );
  }

  if (loadError) {
    return (
      <div className={styles.errorContainer}>
        <label>Failed to load character sheet.</label>
      </div>
    );
  }

  return (
    <div className={styles.sheetWrapper}>

      {/* ── JSON export / import + Regenerate bar ── */}
      <div className={styles.topActionsBar}>
        <div className={styles.jsonActionsContainer}>
          <button
            className={styles.jsonActionButton}
            onClick={handleExportJson}
            disabled={isSaving || isRegenerating}
            title="Export character sheet as JSON"
          >
            Export JSON
          </button>
          <button
            className={styles.jsonActionButton}
            onClick={() => importFileRef.current?.click()}
            disabled={isSaving || isRegenerating}
            title="Import character sheet from JSON and save"
          >
            Import JSON
          </button>
          <input
            ref={importFileRef}
            type="file"
            accept="application/json,.json"
            className={styles.hiddenFileInput}
            onChange={handleImportJson}
          />
        </div>

        <div className={styles.regenerateBar}>
          {regenerateError && (
            <label className={styles.saveErrorLabel}>Regeneration failed. Please try again.</label>
          )}
          <button
            className={styles.regenerateButton}
            onClick={handleRegenerate}
            disabled={isRegenerating}
            title="Override the whole CharacterSheet with values from querying the LLM"
          >
            {isRegenerating ? <ImSpinner2 className={styles.saveSpinner} /> : "Regenerate Sheet"}
          </button>
        </div>
      </div>

      {/* ── Identity ── */}
      <Section title="Identity">
        <div className={styles.twoCol}>
          <Field label="First Name">
            <SheetInput value={firstName} onChange={setFirstName} placeholder="Daphne" />
          </Field>
          <Field label="Last Name">
            <SheetInput value={lastName} onChange={setLastName} placeholder="Greengrass" />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Birthday">
            <SheetInput value={birthday} onChange={setBirthday} placeholder="01 March 1990" />
          </Field>
          <Field label="Age Group">
            <SheetSelect value={ageGroup} onChange={setAgeGroup} options={AGE_GROUP_OPTIONS} />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Gender">
            <SheetSelect value={gender} onChange={setGender} options={GENDER_OPTIONS} />
          </Field>
          <Field label="Sexuality">
            <SheetInput value={sexuality} onChange={setSexuality} placeholder="Heterosexual" />
          </Field>
        </div>
        <Field label="Race / Species">
          <SheetInput value={race} onChange={setRace} placeholder="Human (Pure-blood witch)" />
        </Field>
        <Field label="Profession">
          <SheetInput value={profession} onChange={setProfession} placeholder="Hogwarts student (Slytherin)" />
        </Field>
      </Section>

      {/* ── Appearance ── */}
      <Section title="Appearance">
        <div className={styles.oneCol}>
          <Field label="Body Type">
            <SheetInput value={bodyType} onChange={setBodyType} placeholder="Lean and slender" />
          </Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Height">
            <SheetInput value={height} onChange={setHeight} placeholder="5'4 (162 cm)" />
          </Field>
          <Field label="Eye Color">
            <SheetInput value={eyeColor} onChange={setEyeColor} placeholder="Emerald green" />
          </Field>
          <Field label="Skin Color">
            <SheetInput value={skinColor} onChange={setSkinColor} placeholder="Very pale" />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Hair Color">
            <SheetInput value={hairColor} onChange={setHairColor} placeholder="Platinum blonde" />
          </Field>
          <Field label="Hair Style">
            <SheetInput value={hairStyle} onChange={setHairStyle} placeholder="Long, straight, slicked back" />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Ear Shape">
            <SheetInput value={earShape} onChange={setEarShape} placeholder="Normal" />
          </Field>
          <Field label="Genitals">
            <SheetSelect value={genitals} onChange={setGenitals} options={GENITALS_OPTIONS} />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Breasts Size">
            <SheetSelect value={breastsSize} onChange={setBreastsSize} options={BREASTS_SIZE_OPTIONS} />
          </Field>
          <Field label="Penis Size">
            <SheetInput value={penisSize} onChange={setPenisSize} placeholder="Average (5 inches)" />
          </Field>
        </div>
        <div className={styles.oneCol}>
          <Field label="Attractiveness">
            <SheetInput value={attractiveness} onChange={setAttractiveness} placeholder="Very High" />
          </Field>
        </div>
        <Field label="Clothes Preference">
          <SheetTextarea value={clothesPreference} onChange={setClothesPreference} minHeight="6em" />
        </Field>
      </Section>

      {/* ── Voice & Manner ── */}
      <Section title="Voice & Manner">
        <Field label="Speech Pattern">
          <SheetTextarea value={speechPattern} onChange={setSpeechPattern} minHeight="6em" />
        </Field>
        <Field label="Speech Impairment">
          <SheetInput value={speechImpairment} onChange={setSpeechImpairment} placeholder="None" />
        </Field>
        <Field label="Mannerisms">
          <SheetTextarea value={mannerisms} onChange={setMannerisms} minHeight="5em" />
        </Field>
        <Field label="Social Anxiety">
          <SheetInput value={socialAnxiety} onChange={setSocialAnxiety} placeholder="None" />
        </Field>
      </Section>

      {/* ── Personality ── */}
      <Section title="Personality">
        <Field label="Behavior">
          <SheetTextarea value={behavior} onChange={setBehavior} minHeight="10em" />
        </Field>
        <ArrayField label="Personality Traits" value={personalityTraits} onChange={setPersonalityTraits} minHeight="10em" />
        <ArrayField label="Likes" value={likes} onChange={setLikes} minHeight="10em" />
        <ArrayField label="Dislikes" value={dislikes} onChange={setDislikes} minHeight="10em" />
        <ArrayField label="Fears" value={fears} onChange={setFears} minHeight="10em" />
        <ArrayField label="Secrets" value={secrets} onChange={setSecrets} minHeight="10em" />
        <ArrayField label="Skills" value={skills} onChange={setSkills} minHeight="10em" />
        <ArrayField label="Weaknesses" value={weaknesses} onChange={setWeaknesses} minHeight="10em" />
      </Section>

      {/* ── Background ── */}
      <Section title="Background & Social">
        <Field label="Reputation">
          <SheetTextarea value={reputation} onChange={setReputation} minHeight="6em" />
        </Field>
        <ArrayField label="Relationships" value={relationships} onChange={setRelationships} minHeight="10em" />
      </Section>

      {/* ── Combat ── */}
      <Section title="Combat">
        <Field label="Preferred Combat Style">
          <SheetTextarea value={preferredCombatStyle} onChange={setPreferredCombatStyle} minHeight="10em" />
        </Field>
        <Field label="Weapons Proficiency">
          <SheetInput value={weaponsProficiency} onChange={setWeaponsProficiency} placeholder="Wand magic" />
        </Field>
        <Field label="Combat Affinity — Attack">
          <SheetTextarea value={combatAffinityAttack} onChange={setCombatAffinityAttack} minHeight="8em" />
        </Field>
        <Field label="Combat Affinity — Defense">
          <SheetTextarea value={combatAffinityDefense} onChange={setCombatAffinityDefense} minHeight="8em" />
        </Field>
      </Section>

      {/* ── Goals ── */}
      <Section title="Goals">
        <ArrayField label="Goals for Next Year" value={goalsForNextYear} onChange={setGoalsForNextYear} minHeight="12em" />
        <ArrayField label="Long-Term Goals" value={longTermGoals} onChange={setLongTermGoals} minHeight="10em" />
      </Section>

      {/* ── Adult / Private ── */}
      <Section title="Private">
        <ArrayField label="Kinks" value={kinks} onChange={setKinks} minHeight="20em" />
        <ArrayField label="Secret Kinks" value={secretKinks} onChange={setSecretKinks} minHeight="20em" />
      </Section>

      {/* ── Pathfinder Attributes ── */}
      <Section title="Pathfinder Attributes">
        <div className={styles.statGrid}>
          {PATHFINDER_ATTR_KEYS.map((key) => (
            <div key={key} className={styles.statCell}>
              <label className={styles.statLabel}>{key}</label>
              <input
                type="number"
                className={styles.statInput}
                value={attrMap[key] ?? 10}
                min={0}
                max={99}
                onChange={(e) =>
                  setAttrMap((prev) => ({ ...prev, [key]: Number(e.target.value) }))
                }
              />
            </div>
          ))}
        </div>
      </Section>

      {/* ── Pathfinder Skills ── */}
      <Section title="Pathfinder Skills">
        <div className={styles.statGrid}>
          {PATHFINDER_SKILL_KEYS.map((key) => (
            <div key={key} className={styles.statCell}>
              <label className={styles.statLabel}>{key}</label>
              <input
                type="number"
                className={styles.statInput}
                value={skillMap[key] ?? 10}
                min={0}
                max={99}
                onChange={(e) =>
                  setSkillMap((prev) => ({ ...prev, [key]: Number(e.target.value) }))
                }
              />
            </div>
          ))}
        </div>
      </Section>

      {/* ── Save bar ── */}
      <div className={styles.saveBar}>
        {saveError && (
          <label className={styles.saveErrorLabel}>Failed to save. Please try again.</label>
        )}
        {saveSuccess && (
          <label className={styles.saveSuccessLabel}>Character sheet saved.</label>
        )}
        <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
          {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save Sheet"}
        </button>
      </div>
    </div>
  );
}
