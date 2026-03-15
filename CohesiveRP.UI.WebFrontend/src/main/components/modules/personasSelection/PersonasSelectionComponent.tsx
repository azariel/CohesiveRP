import styles from "./PersonasSelectionComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { MdAddBox } from "react-icons/md";
import { AiOutlineDisconnect } from "react-icons/ai";
import { HiUserCircle } from "react-icons/hi";

/* Store */
import { sharedContext } from "../../../../store/AppSharedStoreContext";
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { PersonaResponseDto } from "../../../../ResponsesDto/personas/PersonaResponseDto";
import type { PersonasResponseDto } from "../../../../ResponsesDto/personas/PersonasResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SharedContextPersonaType } from "../../../../store/SharedContextPersonaType";
import { GetAvatarPathFromCharacterId } from "../../../../utils/avatarUtils";

export default function PersonasSelectionComponent() {
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [personasResponse, setPersonasResponse] = useState<PersonasResponseDto | null>(null);

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        const response: PersonasResponseDto | null = await getFromServerApiAsync<PersonasResponseDto>("api/personas");

        const serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code !== 200 || serverApiException?.message) {
          console.error(`Call to fetch personas list failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setPersonasResponse({ code: -1, personas: [] });
          return;
        }

        console.log("Personas list fetched successfully.");
        setPersonasResponse(response);
      } catch (error) {
        console.error("Fetch personas list error:", error);
      }
    };

    fetchData();
  }, []);

  const handleSpecificPersonaClick = (personaId: string) => {
    const selectedPersona = {
      selectedPersonaId: personaId,
      moduleName: "personaDetails",
    } as SharedContextPersonaType;

    navigateTo(selectedPersona);
    console.log("PersonaDetails selected -> Module personaDetails selected.");
  };

  const handleAddPersonaClick = async () => {
    try {
      const response = await postToServerApiAsync<PersonaResponseDto>("api/personas", {});

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || serverApiException?.message) {
        console.error(`Create new persona failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
        return;
      }

      console.log("Persona created successfully.");

      const personaToAdd = response.persona;
      if (!personaToAdd)
        return;

      setPersonasResponse((prev) => {
        if (!prev)
          return { code: 200, personas: [personaToAdd] } as PersonasResponseDto;

        return { ...prev, personas: [personaToAdd, ...(prev.personas || [])] };
      });
    } catch (err) {
      console.error(err);
      // TODO: show err to user
    }
  };

  return (
    <main className={styles.personasComponent}>
      {isNetworkDown ? (
        <div className={styles.networkDownContainer}>
          <AiOutlineDisconnect className={styles.networkDownIcon} />
          <label>CohesiveRP backend is unreachable</label>
        </div>
      ) : (
        <div className={styles.personasMainContainer}>
          <div className={styles.personasHeader}>
            <div className={styles.personasToolsComponent}>
              <MdAddBox
                className={styles.addNewPersonaIcon}
                onClick={handleAddPersonaClick}
              />
            </div>
          </div>

          <div className={styles.personasGridContainer}>
            {personasResponse?.personas?.map((persona) => (
              <div
                key={persona.personaId}
                className={styles.personaContainer}
                onClick={() => handleSpecificPersonaClick(persona.personaId)}
              >
                <div className={styles.personaAvatarContainer}>
                  {persona.personaId ? (
                    <img
                      src={GetAvatarPathFromCharacterId(persona.personaId)}
                      alt={persona.name}
                    />
                  ) : (
                    <HiUserCircle className={styles.personaAvatarFallback} />
                  )}
                </div>

                <div className={styles.personaInfoPanel}>
                  <div className={styles.personaNameRow}>
                    <label className={styles.personaNameLabel}>
                      {persona.name}
                    </label>
                  </div>
                  <label className={styles.personaDescriptionLabel}>
                    {persona.description?.length ?? 0 > 512
                      ? `${persona.description?.substring(0, 512) ?? ""}...`
                      : persona.description}
                  </label>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </main>
  );
}
