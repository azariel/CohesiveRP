import styles from "./CharacterSheetComponent.module.css";
import { useState, useEffect, useRef } from "react";
import { ImSpinner2 } from "react-icons/im";
import { getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterSheetResponseDto } from "../../../../../ResponsesDto/characters/characterSheets/CharacterSheetResponseDto";
import type { ServerApiResponseDto } from "../../../../../ResponsesDto/ServerApiResponseDto";
import type { CharacterSheetRequestDto } from "../../../../../RequestDto/characters/characterSheets/CharacterSheetRequestDto";

/* ─────────────────────────── Constants ─────────────────────── */

const GENDER_OPTIONS = ["", "Male", "Female"];
const AGE_GROUP_OPTIONS = ["", "Child", "Teenager", "YoungAdult", "Adult", "MiddleAged", "Elderly"];
const GENITALS_OPTIONS = ["", "Male", "Female", "Both", "None"];
const BREASTS_SIZE_OPTIONS = ["", "Flat", "Small", "Medium", "Large", "ExtraLarge"];
const SEXUALITY_OPTIONS = ["", "Straight", "Gay", "Bisexual", "Pansexual", "Asexual"];

const PATHFINDER_ATTR_KEYS = [
  "Fortitude", "Reflex", "Willpower", "Stamina",
  "MagicalStamina", "Intelligence", "Discernment", "Perception",
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

const toDateInputValue = (iso?: string | null) => {
  if (!iso) return "";
  return iso.substring(0, 10); // "YYYY-MM-DD"
};

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
  return (
    <Field label={`${label} (one per line)`}>
      <SheetTextarea
        value={arrToText(value)}
        onChange={(v) => onChange(textToArr(v))}
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

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  const [characterSheetId, setCharacterSheetId] = useState<string | null>("");

  /* ── identity ── */
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [birthdayDate, setBirthdayDate] = useState("");
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

  /* ─── fetch ─── */
  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;

    const fetchSheet = async () => {
      try {
        let response:CharacterSheetResponseDto | null;
        
        if(characterId)
        {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/${characterId}/characterSheet`);
        } else
        {
          response = await getFromServerApiAsync<CharacterSheetResponseDto>(`api/characters/personaCharacterSheet/${personaId}`);
        }

        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || ex?.message) {

          if(ex?.code !== 404) {
            console.error("Failed to load character sheet:", ex);
            setLoadError(true);
            return;
          }
        }

        const s = response as CharacterSheetResponseDto;

        setCharacterSheetId(s.characterSheetId ?? null);
        setFirstName(s.characterSheet?.firstName ?? "");
        setLastName(s.characterSheet?.lastName ?? "");
        setBirthdayDate(toDateInputValue(s.characterSheet?.birthdayDate));
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
          s.characterSheet?.pathfinderAttributes.forEach((a) => { map[a.attributeType] = a.value; });
          setAttrMap(map);
        }
        if (s.characterSheet?.pathfinderSkills?.length) {
          const map = { ...defaultSkillMap() };
          s.characterSheet?.pathfinderSkills.forEach((a) => { map[a.skillType] = a.value; });
          setSkillMap(map);
        }
      } catch (err) {
        console.error("Fetch character sheet error:", err);
        setLoadError(true);
      } finally {
        setIsLoading(false);
      }
    };

    fetchSheet();
  }, [characterId]);

  /* ─── save ─── */
  const handleSave = async () => {
    if (isSaving) return;
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
          birthdayDate: birthdayDate ? new Date(birthdayDate).toISOString() : null,
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

      let response:ServerApiResponseDto | null;
      if(characterSheetId){
        response = await putToServerApiAsync(`api/characters/${characterId}/characterSheet/${characterSheetId}`, payload);
      } else {
        response = await postToServerApiAsync(`api/characters/${characterId}/characterSheet`, payload);
      }
      

      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error("Save character sheet failed:", ex);
        setSaveError(true);
      } else {
        const typedResponse = response as CharacterSheetResponseDto;
        setCharacterSheetId(typedResponse.characterSheetId ?? null);
        setSaveSuccess(true);
        setTimeout(() => setSaveSuccess(false), 5000);
      }
    } catch (err) {
      console.error("Save character sheet error:", err);
      setSaveError(true);
    } finally {
      setIsSaving(false);
    }
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
            <input
              type="date"
              className={styles.sheetInput}
              value={birthdayDate}
              onChange={(e) => setBirthdayDate(e.target.value)}
            />
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
            <SheetSelect value={sexuality} onChange={setSexuality} options={SEXUALITY_OPTIONS} />
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
        <div className={styles.twoCol}>
          <Field label="Height">
            <SheetInput value={height} onChange={setHeight} placeholder="5'4 (162 cm)" />
          </Field>
          <Field label="Body Type">
            <SheetInput value={bodyType} onChange={setBodyType} placeholder="Lean and slender" />
          </Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Hair Color">
            <SheetInput value={hairColor} onChange={setHairColor} placeholder="Platinum blonde" />
          </Field>
          <Field label="Hair Style">
            <SheetInput value={hairStyle} onChange={setHairStyle} placeholder="Long, straight, slicked back" />
          </Field>
          <Field label="Eye Color">
            <SheetInput value={eyeColor} onChange={setEyeColor} placeholder="Emerald green" />
          </Field>
        </div>
        <div className={styles.threeCol}>
          <Field label="Skin Color">
            <SheetInput value={skinColor} onChange={setSkinColor} placeholder="Very pale" />
          </Field>
          <Field label="Ear Shape">
            <SheetInput value={earShape} onChange={setEarShape} placeholder="Normal" />
          </Field>
          <Field label="Attractiveness">
            <SheetInput value={attractiveness} onChange={setAttractiveness} placeholder="Very High" />
          </Field>
        </div>
        <div className={styles.twoCol}>
          <Field label="Genitals">
            <SheetSelect value={genitals} onChange={setGenitals} options={GENITALS_OPTIONS} />
          </Field>
          <Field label="Breasts Size">
            <SheetSelect value={breastsSize} onChange={setBreastsSize} options={BREASTS_SIZE_OPTIONS} />
          </Field>
        </div>
        <Field label="Clothes Preference">
          <SheetTextarea value={clothesPreference} onChange={setClothesPreference} minHeight="5em" />
        </Field>
      </Section>

      {/* ── Voice & Manner ── */}
      <Section title="Voice & Manner">
        <Field label="Speech Pattern">
          <SheetTextarea value={speechPattern} onChange={setSpeechPattern} minHeight="5em" />
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
          <SheetTextarea value={behavior} onChange={setBehavior} minHeight="7em" />
        </Field>
        <ArrayField label="Personality Traits" value={personalityTraits} onChange={setPersonalityTraits} />
        <ArrayField label="Likes" value={likes} onChange={setLikes} />
        <ArrayField label="Dislikes" value={dislikes} onChange={setDislikes} />
        <ArrayField label="Fears" value={fears} onChange={setFears} />
        <ArrayField label="Secrets" value={secrets} onChange={setSecrets} minHeight="5em" />
      </Section>

      {/* ── Background ── */}
      <Section title="Background & Social">
        <Field label="Reputation">
          <SheetTextarea value={reputation} onChange={setReputation} minHeight="5em" />
        </Field>
        <ArrayField label="Relationships" value={relationships} onChange={setRelationships} minHeight="5em" />
      </Section>

      {/* ── Combat ── */}
      <Section title="Combat">
        <Field label="Preferred Combat Style">
          <SheetTextarea value={preferredCombatStyle} onChange={setPreferredCombatStyle} minHeight="5em" />
        </Field>
        <Field label="Weapons Proficiency">
          <SheetInput value={weaponsProficiency} onChange={setWeaponsProficiency} placeholder="Wand magic" />
        </Field>
        <Field label="Combat Affinity — Attack">
          <SheetTextarea value={combatAffinityAttack} onChange={setCombatAffinityAttack} minHeight="4em" />
        </Field>
        <Field label="Combat Affinity — Defense">
          <SheetTextarea value={combatAffinityDefense} onChange={setCombatAffinityDefense} minHeight="4em" />
        </Field>
        <ArrayField label="Skills" value={skills} onChange={setSkills} />
        <ArrayField label="Weaknesses" value={weaknesses} onChange={setWeaknesses} />
      </Section>

      {/* ── Goals ── */}
      <Section title="Goals">
        <ArrayField label="Goals for Next Year" value={goalsForNextYear} onChange={setGoalsForNextYear} minHeight="5em" />
        <ArrayField label="Long-Term Goals" value={longTermGoals} onChange={setLongTermGoals} minHeight="5em" />
      </Section>

      {/* ── Adult / Private ── */}
      <Section title="Private">
        <ArrayField label="Kinks" value={kinks} onChange={setKinks} minHeight="6em" />
        <ArrayField label="Secret Kinks" value={secretKinks} onChange={setSecretKinks} minHeight="6em" />
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
