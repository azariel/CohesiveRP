import styles from "./ChatDetailsComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";
import { ImSpinner2 } from "react-icons/im";
import { MdCheck, MdKeyboardArrowDown, MdKeyboardArrowUp } from "react-icons/md";

import { deleteFromServerApiAsync, getFromServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import type { ChatResponseDto } from "../../../../ResponsesDto/chat/ChatResponseDto";
import type { SharedContextType } from "../../../../store/SharedContextType";
import { GetAvatarPathFromChatIdAndAvatarId } from "../../../../utils/avatarUtils";
import type { Lorebook } from "../../../../ResponsesDto/lorebooks/BusinessObjects/Lorebook";
import type { LorebooksResponseDto } from "../../../../ResponsesDto/lorebooks/LorebooksResponseDto";
import type { Persona } from "../../../../ResponsesDto/personas/BusinessObjects/Persona";
import type { PersonasResponseDto } from "../../../../ResponsesDto/personas/PersonasResponseDto";
import type { CharactersResponseDto } from "../../../../ResponsesDto/characters/CharactersResponseDto";
import type { CharacterResponse } from "../../../../ResponsesDto/characters/CharacterResponse";

export default function ChatDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextChatType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingChatDetails, setIsLoadingChatDetails] = useState(true);
  const [chatResponse, setChatResponse] = useState<ChatResponseDto | null>(null);

  // saving state
  const [name, setName] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  // character state
  const [availableCharacters, setAvailableCharacters] = useState<CharacterResponse[]>([]);
  const [selectedCharacterIds, setSelectedCharacterIds] = useState<string[]>([]);
  const [isCharacterDropdownOpen, setIsCharacterDropdownOpen] = useState(false);
  const [isLoadingCharacters, setIsLoadingCharacters] = useState(false);
  const characterDropdownRef = useRef<HTMLDivElement>(null);

  // lorebook state
  const [availableLorebooks, setAvailableLorebooks] = useState<Lorebook[]>([]);
  const [selectedLorebookIds, setSelectedLorebookIds] = useState<string[]>([]);
  const [isLorebookDropdownOpen, setIsLorebookDropdownOpen] = useState(false);
  const [isLoadingLorebooks, setIsLoadingLorebooks] = useState(false);
  const lorebookDropdownRef = useRef<HTMLDivElement>(null);

  // persona state
  const [availablePersonas, setAvailablePersonas] = useState<Persona[]>([]);
  const [selectedPersonaId, setSelectedPersonaId] = useState<string>("");
  const [isPersonaDropdownOpen, setIsPersonaDropdownOpen] = useState(false);
  const [isLoadingPersonas, setIsLoadingPersonas] = useState(false);
  const personaDropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;
    didComponentMountAlready.current = true;

    fetchChatDetails();
    fetchCharacters();
    fetchLorebooks();
    fetchPersonas();
  }, []);

  // Close dropdowns when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (characterDropdownRef.current && !characterDropdownRef.current.contains(e.target as Node))
        setIsCharacterDropdownOpen(false);
      if (lorebookDropdownRef.current && !lorebookDropdownRef.current.contains(e.target as Node))
        setIsLorebookDropdownOpen(false);
      if (personaDropdownRef.current && !personaDropdownRef.current.contains(e.target as Node))
        setIsPersonaDropdownOpen(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const fetchChatDetails = async () => {
    if (!activeModule?.chatId) {
      console.error(`Selected chat id is not valid and thus has no details.`);
      return;
    }

    try {
      setIsLoadingChatDetails(true);
      const response: ChatResponseDto | null = await getFromServerApiAsync<ChatResponseDto>(`api/chats/${activeModule.chatId}`);

      let serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Call to fetch chat details failed. [${JSON.stringify(serverApiException)}]`);
        setIsNetworkDown(true);
        setChatResponse({
          code: -1,
          chatId: "",
          characterIds: [],
          lorebookIds: [],
          personaId: "",
          lastActivityDateTime: null,
          name: null,
        });
        return;
      }

      console.log(`Chat details fetched successfully.`);
      setChatResponse(response);
      setName(response?.name ?? "");
      setSelectedPersonaId(response?.personaId ?? "");
      setSelectedLorebookIds(response?.lorebookIds ?? []);
      setSelectedCharacterIds(response?.characterIds ?? []);
    } catch (error) {
      console.error("Fetch chat error:", error);
    } finally {
      setIsLoadingChatDetails(false);
    }
  };

  const fetchCharacters = async () => {
    try {
      setIsLoadingCharacters(true);
      const response: CharactersResponseDto | null = await getFromServerApiAsync<CharactersResponseDto>(`api/characters`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Failed to fetch available characters. [${JSON.stringify(serverApiException)}]`);
        return;
      }

      setAvailableCharacters(response.characters ?? []);
    } catch (error) {
      console.error("Fetch available characters error:", error);
    } finally {
      setIsLoadingCharacters(false);
    }
  };

  const fetchLorebooks = async () => {
    try {
      setIsLoadingLorebooks(true);
      const response: LorebooksResponseDto | null = await getFromServerApiAsync<LorebooksResponseDto>(`api/lorebooks`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Failed to fetch available lorebooks. [${JSON.stringify(serverApiException)}]`);
        return;
      }

      setAvailableLorebooks(response.lorebooks ?? []);
    } catch (error) {
      console.error("Fetch available lorebooks error:", error);
    } finally {
      setIsLoadingLorebooks(false);
    }
  };

  const fetchPersonas = async () => {
    try {
      setIsLoadingPersonas(true);
      const response: PersonasResponseDto | null = await getFromServerApiAsync<PersonasResponseDto>(`api/personas`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message) {
        console.error(`Failed to fetch available personas. [${JSON.stringify(serverApiException)}]`);
        return;
      }

      setAvailablePersonas(response.personas ?? []);
    } catch (error) {
      console.error("Fetch available personas error:", error);
    } finally {
      setIsLoadingPersonas(false);
    }
  };

  const toggleCharacterSelection = (characterId: string) => {
    setSelectedCharacterIds(prev =>
      prev.includes(characterId)
        ? prev.filter(id => id !== characterId)
        : [...prev, characterId]
    );
  };

  const toggleLorebookSelection = (lorebookId: string) => {
    setSelectedLorebookIds(prev =>
      prev.includes(lorebookId)
        ? prev.filter(id => id !== lorebookId)
        : [...prev, lorebookId]
    );
  };

  const selectPersona = (personaId: string) => {
    setSelectedPersonaId(prev => prev === personaId ? "" : personaId);
    setIsPersonaDropdownOpen(false);
  };

  const handleSave = async () => {
    if (!activeModule?.chatId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const response = await putToServerApiAsync(`api/chats/${activeModule.chatId}`, {
        name,
        chatId: activeModule.chatId,
        characterIds: selectedCharacterIds,
        lorebookIds: selectedLorebookIds,
        personaId: selectedPersonaId,
      });

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Save failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      }
    } catch (error) {
      console.error("Save chat details error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!activeModule?.chatId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const response = await deleteFromServerApiAsync(`api/chats/${activeModule.chatId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Deletion failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else {
        let module = {
          moduleName: "chats"
        } as SharedContextType;

        navigateTo(module);
      }
    } catch (error) {
      console.error("Deletion chat error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const selectedCharactersCount = selectedCharacterIds.length;
  const dropdownCharacterSelectionsLabel = selectedCharactersCount === 0
    ? "None selected"
    : selectedCharactersCount === 1
      ? (availableCharacters.find(c => c.characterId === selectedCharacterIds[0])?.name ?? "1 character")
      : `${selectedCharactersCount} characters selected`;

  const selectedLorebooksCount = selectedLorebookIds.length;
  const dropdownLorebookSelectionsLabel = selectedLorebooksCount === 0
    ? "None selected"
    : selectedLorebooksCount === 1
      ? (availableLorebooks.find(l => l.lorebookId === selectedLorebookIds[0])?.name ?? "1 lorebook")
      : `${selectedLorebooksCount} lorebooks selected`;

  const dropdownPersonaSelectionLabel = selectedPersonaId
    ? (availablePersonas.find(p => p.personaId === selectedPersonaId)?.name ?? "Unknown persona")
    : "None selected";

  return (
    <main className={styles.chatDetailsComponent}>
      {isNetworkDown ? (
        <div className={styles.networkDownContainer}>
          <AiOutlineDisconnect className={styles.networkDownIcon} />
          <label>CohesiveRP backend is unreachable</label>
        </div>
      ) : (
        isLoadingChatDetails ? (
          <ImSpinner2 className={styles.loadingChatDetailsSpinner} />
        ) : (
          <div className={styles.chatDetailsContainer}>
            <div className={styles.chatHeaderContainer}>
              <div className={styles.chatAvatarContainer}>
                <div className={styles.addNewAvatarImageContainer}>
                  <input type="file" style={{ display: "none" }} />
                  <img src={`${GetAvatarPathFromChatIdAndAvatarId(chatResponse?.chatId ?? "", "avatar")}`} alt="no image" />
                </div>
              </div>
            </div>
            <div className={styles.detailsContainer}>
              <div className={styles.chatNameContainer}>
                <label className={styles.chatNameLabel}>Chat Name</label>
                <textarea
                  className={styles.chatName}
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
                <label className={styles.chatId}>{chatResponse?.chatId ?? ""}</label>
              </div>

              {/* Persona single-select dropdown */}
              <div className={styles.lorebookSection}>
                <label className={styles.lorebookLabel}>Persona</label>
                <div className={styles.lorebookDropdownWrapper} ref={personaDropdownRef}>
                  <button
                    className={styles.lorebookDropdownTrigger}
                    onClick={() => {
                      setIsPersonaDropdownOpen(prev => !prev);
                      setIsLorebookDropdownOpen(false);
                      setIsCharacterDropdownOpen(false);
                    }}
                    disabled={isLoadingPersonas}
                    type="button"
                  >
                    {isLoadingPersonas ? (
                      <ImSpinner2 className={styles.lorebookSpinner} />
                    ) : (
                      <span className={styles.lorebookDropdownLabel}>{dropdownPersonaSelectionLabel}</span>
                    )}
                    {isPersonaDropdownOpen
                      ? <MdKeyboardArrowUp className={styles.lorebookDropdownChevron} />
                      : <MdKeyboardArrowDown className={styles.lorebookDropdownChevron} />
                    }
                  </button>

                  {isPersonaDropdownOpen && (
                    <div className={styles.lorebookDropdownMenu}>
                      {availablePersonas.length === 0 ? (
                        <div className={styles.lorebookDropdownEmpty}>No personas available</div>
                      ) : (
                        availablePersonas.map(persona => {
                          const isSelected = selectedPersonaId === persona.personaId;
                          return (
                            <div
                              key={persona.personaId}
                              className={`${styles.lorebookDropdownItem} ${isSelected ? styles.lorebookDropdownItemSelected : ""}`}
                              onClick={() => selectPersona(persona.personaId)}
                            >
                              <span className={styles.lorebookCheckMark}>
                                {isSelected && <MdCheck className={styles.lorebookCheckIcon} />}
                              </span>
                              <span className={styles.lorebookItemName}>{persona.name}</span>
                            </div>
                          );
                        })
                      )}
                    </div>
                  )}
                </div>
              </div>

              {/* Character multi-select dropdown */}
              <div className={styles.lorebookSection}>
                <label className={styles.lorebookLabel}>Characters</label>
                <div className={styles.lorebookDropdownWrapper} ref={characterDropdownRef}>
                  <button
                    className={styles.lorebookDropdownTrigger}
                    onClick={() => {
                      setIsCharacterDropdownOpen(prev => !prev);
                      setIsLorebookDropdownOpen(false);
                      setIsPersonaDropdownOpen(false);
                    }}
                    disabled={isLoadingCharacters}
                    type="button"
                  >
                    {isLoadingCharacters ? (
                      <ImSpinner2 className={styles.lorebookSpinner} />
                    ) : (
                      <span className={styles.lorebookDropdownLabel}>{dropdownCharacterSelectionsLabel}</span>
                    )}
                    {isCharacterDropdownOpen
                      ? <MdKeyboardArrowUp className={styles.lorebookDropdownChevron} />
                      : <MdKeyboardArrowDown className={styles.lorebookDropdownChevron} />
                    }
                  </button>

                  {isCharacterDropdownOpen && (
                    <div className={styles.lorebookDropdownMenu}>
                      {availableCharacters.length === 0 ? (
                        <div className={styles.lorebookDropdownEmpty}>No characters available</div>
                      ) : (
                        availableCharacters.map(character => {
                          const isSelected = selectedCharacterIds.includes(character.characterId);
                          return (
                            <div
                              key={character.characterId}
                              className={`${styles.lorebookDropdownItem} ${isSelected ? styles.lorebookDropdownItemSelected : ""}`}
                              onClick={() => toggleCharacterSelection(character.characterId)}
                            >
                              <span className={styles.lorebookCheckMark}>
                                {isSelected && <MdCheck className={styles.lorebookCheckIcon} />}
                              </span>
                              <span className={styles.lorebookItemName}>
                                {character.name ?? character.characterId}
                              </span>
                            </div>
                          );
                        })
                      )}
                    </div>
                  )}
                </div>
              </div>

              {/* Lorebook multi-select dropdown */}
              <div className={styles.lorebookSection}>
                <label className={styles.lorebookLabel}>Lorebooks</label>
                <div className={styles.lorebookDropdownWrapper} ref={lorebookDropdownRef}>
                  <button
                    className={styles.lorebookDropdownTrigger}
                    onClick={() => {
                      setIsLorebookDropdownOpen(prev => !prev);
                      setIsPersonaDropdownOpen(false);
                      setIsCharacterDropdownOpen(false);
                    }}
                    disabled={isLoadingLorebooks}
                    type="button"
                  >
                    {isLoadingLorebooks ? (
                      <ImSpinner2 className={styles.lorebookSpinner} />
                    ) : (
                      <span className={styles.lorebookDropdownLabel}>{dropdownLorebookSelectionsLabel}</span>
                    )}
                    {isLorebookDropdownOpen
                      ? <MdKeyboardArrowUp className={styles.lorebookDropdownChevron} />
                      : <MdKeyboardArrowDown className={styles.lorebookDropdownChevron} />
                    }
                  </button>

                  {isLorebookDropdownOpen && (
                    <div className={styles.lorebookDropdownMenu}>
                      {availableLorebooks.length === 0 ? (
                        <div className={styles.lorebookDropdownEmpty}>No lorebooks available</div>
                      ) : (
                        availableLorebooks.map(lorebook => {
                          const isSelected = selectedLorebookIds.includes(lorebook.lorebookId);
                          return (
                            <div
                              key={lorebook.lorebookId}
                              className={`${styles.lorebookDropdownItem} ${isSelected ? styles.lorebookDropdownItemSelected : ""}`}
                              onClick={() => toggleLorebookSelection(lorebook.lorebookId)}
                            >
                              <span className={styles.lorebookCheckMark}>
                                {isSelected && <MdCheck className={styles.lorebookCheckIcon} />}
                              </span>
                              <span className={styles.lorebookItemName}>
                                {lorebook.name ?? lorebook.lorebookId}
                              </span>
                            </div>
                          );
                        })
                      )}
                    </div>
                  )}
                </div>
              </div>

              <div className={styles.operationsButtonsContainer}>
                <div className={styles.operationsButtons}>
                  <button className={styles.deleteButton} onClick={handleDelete} disabled={isSaving}>
                    {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Delete"}
                  </button>
                  <button className={styles.saveButton} onClick={handleSave} disabled={isSaving}>
                    {isSaving ? <ImSpinner2 className={styles.saveSpinner} /> : "Save"}
                  </button>
                </div>
                {operationError && (
                  <label className={styles.saveErrorLabel}>Failed to save/delete. Please try again.</label>
                )}
              </div>
            </div>
          </div>
        )
      )}
    </main>
  );
}
