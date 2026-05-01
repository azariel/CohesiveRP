import styles from "./CharacterDetailsComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import { deleteFromServerApiAsync, getBlobFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterResponseDto } from "../../../../ResponsesDto/characters/CharacterResponseDto";
import type { SharedContextCharacterType } from "../../../../store/SharedContextCharacterType";
import { GetAvatarPathFromAvatarFilePath, GetAvatarPathFromCharacterName, GetAvatarPathFromChatId, GetFallbackEmpty } from "../../../../utils/avatarUtils";
import { ImSpinner2 } from "react-icons/im";
import type { SelectableChatsResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatsResponseDto";
import type { SelectableChatResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatResponseDto";
import type { AddChatRequestDto } from "../../../../RequestDto/chat/AddChatRequestDto";
import { MdAddBox, MdDelete } from "react-icons/md";
import { FormatDateTimeToMinutes } from "../../../../utils/DateUtils";
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import type { SharedContextType } from "../../../../store/SharedContextType";
import CharacterSheetComponent from "../characterSheets/CharacterSheetComponent";
import type { CharacterMainAvatarIllustrationQueryRequestDto } from "../../../../RequestDto/characters/CharacterMainAvatarIllustrationQueryRequestDto";
import { RiRobot2Fill } from "react-icons/ri";
import type { GeneratePromptInjectionForMainCharacterAvatarResponseDto } from "../../../../ResponsesDto/characters/GeneratePromptInjectionForMainCharacterAvatarResponseDto";
import { FaImage } from "react-icons/fa";


type DetailsTab = "info" | "sheet";
type OutfitKey = "Clothed" | "Underwear" | "Naked";
const OUTFIT_OPTIONS: OutfitKey[] = ["Clothed", "Underwear", "Naked"];
const OUTFIT_ENUM: Record<OutfitKey, number> = {
  Clothed: 3,
  Underwear: 2,
  Naked: 1,
};

export default function CharacterDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextCharacterType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const chatsContainerRef = useRef<HTMLDivElement>(null);
  const importCharacterCardRef = useRef<HTMLInputElement>(null);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingChat, setIsLoadingChat] = useState(false);
  const [isLoadingCharacterDetails, setIsLoadingCharacterDetails] = useState(true);
  const [isLoadingChatsDetails, setIsLoadingChatsDetails] = useState(true);
  const [characterResponse, setCharacterResponse] = useState<CharacterResponseDto | null>(null);
  const [chatsDetailsResponse, setChatsDetailsResponse] = useState<SelectableChatsResponseDto | null>(null);
  const [isGeneratingAvatar, setIsGeneratingAvatar] = useState(false);
  const [isGeneratingPromptInjection, setIsGeneratingPromptInjection] = useState(false);



  // saving state
  const [characterName, setCharacterName] = useState("");
  const [creator, setCreator] = useState("");
  const [creatorNotes, setCreatorNotes] = useState("");
  const [tags, setTags] = useState<string[]>([]);
  const [firstMessage, setFirstMessage] = useState("");
  const [alternateGreetings, setAlternateGreetings] = useState<string[]>([]);
  const [characterDescription, setCharacterDescription] = useState("");
  const [sheetKey, setSheetKey] = useState(0);
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);
  const [illustratorTag, setIllustratorTag] = useState("");
  const [selectedOutfit, setSelectedOutfit] = useState<OutfitKey>("Clothed");
  const [illustrationMapOutfits, setillustrationMapOutfits] = useState<{ outfit: OutfitKey; illustratorPromptInjection: string }[]>([]);
  const [lightboxAvatar, setLightboxAvatar] = useState<{ filePath: string; fileName: string; seed?: string | null } | null>(null);
  const [isDeletingAvatar, setIsDeletingAvatar] = useState(false);

  // tab state
  const [activeTab, setActiveTab] = useState<DetailsTab>("info");

  const getOutfitEntry = (outfit: OutfitKey) =>
  illustrationMapOutfits.find((e) => e.outfit === outfit) ?? { outfit, illustratorPromptInjection: "" };

  const setOutfitPrompt = (outfit: OutfitKey, value: string) =>
    setillustrationMapOutfits((prev) => {
      const exists = prev.find((e) => e.outfit === outfit);
      if (exists) return prev.map((e) => e.outfit === outfit ? { ...e, illustratorPromptInjection: value } : e);
      return [...prev, { outfit, illustratorPromptInjection: value }];
    });

  useEffect(() => {
    const el = chatsContainerRef.current;
    if (!el)
      return;

      const onWheel = (e: WheelEvent) => {
      const atStart = el.scrollLeft === 0;
      const atEnd = el.scrollLeft + el.clientWidth >= el.scrollWidth - 1;

      const scrollingLeft = e.deltaY < 0;
      const scrollingRight = e.deltaY > 0;

      if ((scrollingLeft && atStart) || (scrollingRight && atEnd)) {
        return;
      }

      e.preventDefault();
      el.scrollLeft += e.deltaY;
    };
    
    el.addEventListener("wheel", onWheel, { passive: false });
    return () => el.removeEventListener("wheel", onWheel);
  }, [isLoadingChatsDetails]);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;

    didComponentMountAlready.current = true;
    setIsLoadingCharacterDetails(true);

    const fetchCharacterDetails = async () => {
      try {
        const response: CharacterResponseDto | null = await getFromServerApiAsync<CharacterResponseDto>(`api/characters/${activeModule.selectedCharacterId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch characters details failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setCharacterResponse({
            code : -1,
            character: null
          });

          return;
        }

        console.log(`Characters details fetched successfully.`);
        setCharacterResponse(response);
        setCharacterName(response?.character?.name ?? "");
        setCreator(response?.character?.creator ?? "");
        setCreatorNotes(response?.character?.creatorNotes ?? "");
        setTags(response?.character?.tags ?? []);
        setFirstMessage(response?.character?.firstMessage ?? "");
        setAlternateGreetings(response?.character?.alternateGreetings ?? []);
        setCharacterDescription(response?.character?.description ?? "");
        setIllustratorTag(response?.character?.imageGenerationConfiguration?.illustratorTag ?? "");
        setillustrationMapOutfits(
          (response?.character?.imageGenerationConfiguration?.illustrationMapOutfits ?? []).map((e) => ({
            outfit: (e.outfit ?? "clothed") as OutfitKey,
            illustratorPromptInjection: e.illustratorPromptInjection ?? "",
          }))
        );
      } catch (error) {
        console.error("Fetch characters details error:", error);
      } finally{
        setIsLoadingCharacterDetails(false);
      }
    };

    const fetchChatsDetails = async () => {
      try {
        let characterModule = activeModule as SharedContextCharacterType;
        const response: SelectableChatsResponseDto | null = await getFromServerApiAsync<SelectableChatsResponseDto>(`api/chats?characterId=${characterModule.selectedCharacterId}`);
        
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch characters details failed. [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setChatsDetailsResponse({
            code : -1,
            chats: []
          });

          return;
        }
        
        console.log(`Characters details fetched successfully.`);
        setChatsDetailsResponse(response);
      } catch (error) {
        console.error("Fetch characters details error:", error);
      } finally{
        setIsLoadingChatsDetails(false);
      }
    };

    fetchCharacterDetails();
    fetchChatsDetails();
  }, []);

  const handleSpecificChatClick = async (chat: SelectableChatResponseDto) => {
    if(isLoadingChat)
      return;
    
      setIsLoadingChat(true);
      try {
        const savedInput = localStorage.getItem(`chatInput_${chat.chatId}`) ?? "";

        let selectedChat = {
          chatId: chat.chatId,
          moduleName: "chat",
          currentUserInputValue: savedInput,
        } as SharedContextChatType;

        navigateTo(selectedChat);
        console.log(`Chat selected -> Module [${JSON.stringify(selectedChat)}] selected.`);
      } finally{
        setIsLoadingChat(false);
      }
  };

  const handleCreateNewChatClick = async () => {
    if(isLoadingChat)
      return;

    setIsLoadingChat(true);
    try {
      const payload:AddChatRequestDto = {
        characterId: activeModule.selectedCharacterId,
        lorebookIds: [],
      };
      
      const response:SelectableChatResponseDto | null = await postToServerApiAsync<SelectableChatResponseDto>("api/chats", payload);

      if(response) {
        setChatsDetailsResponse(previousCollection => {
          if (!previousCollection) {
            return { chats: [response]} as SelectableChatsResponseDto;
          }
          return {
            ...previousCollection,
            chats: [...(previousCollection.chats || []), response],
          };
        });

        console.log(`New chat created. Response: [${JSON.stringify(response)}].`);
      } else {
        console.error(`Failed to create a new chat. Response:[${response}] json: [${JSON.stringify(response)}].`);
      }
    } finally{
      setIsLoadingChat(false);
    }
  };

  const handleSave = async () => {
    if (!activeModule?.selectedCharacterId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {

      const response = await putToServerApiAsync(`api/characters/${activeModule.selectedCharacterId}`, {
        characterId: activeModule.selectedCharacterId,
        characterDescription,
        characterName,
        creator,
        creatorNotes,
        firstMessage,
        alternateGreetings,
        tags,
        imageGenerationConfiguration: {
          illustratorTag: illustratorTag || null,
          illustrationMapOutfits: illustrationMapOutfits.map((e) => ({
            outfit: e.outfit,
            illustratorPromptInjection: e.illustratorPromptInjection || null,
          })),
        },
       });

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Save failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      }

    } catch (error) {
      console.error("Save character error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleExportCharacterCard = async () => {
    if (!activeModule?.selectedCharacterId || isSaving) return;

    try {
      const blob = await getBlobFromServerApiAsync(`api/characters/${activeModule.selectedCharacterId}/exportCharacterCard`);

      if (!blob || blob.size === 0) {
        console.error("CharacterCard export failed: empty or null blob.");
        setOperationError(true);
        return;
      }

      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = `${characterName || "character"}.png`;
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error("CharacterCard export error:", error);
      setOperationError(true);
    }
  };

  const handleImportCharacterCard = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {
      const formData = new FormData();
      formData.append("file", file);

      const response: CharacterResponseDto | null = await postToServerApiAsync<CharacterResponseDto>(
        `api/characters/${activeModule.selectedCharacterId}/importCharacterCard`,
        formData
      );

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code !== 200 || serverApiException?.message) {
        console.error(`CharacterCard import failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
        return;
      }

      // Refresh all character fields from the response
      const c = response.character;
      setCharacterResponse(response);
      setCharacterName(c?.name ?? "");
      setCreator(c?.creator ?? "");
      setCreatorNotes(c?.creatorNotes ?? "");
      setTags(c?.tags ?? []);
      setFirstMessage(c?.firstMessage ?? "");
      setAlternateGreetings(c?.alternateGreetings ?? []);
      setCharacterDescription(c?.description ?? "");
      setIllustratorTag(c?.imageGenerationConfiguration?.illustratorTag ?? "");
      setillustrationMapOutfits(
        (c?.imageGenerationConfiguration?.illustrationMapOutfits ?? []).map((e) => ({
          outfit: (e.outfit ?? "clothed") as OutfitKey,
          illustratorPromptInjection: e.illustratorPromptInjection ?? "",
        }))
      );

      // Force the CharacterSheetComponent to remount and re-fetch
      setSheetKey(k => k + 1);

    } catch (err) {
      console.error("CharacterCard import error:", err);
      setOperationError(true);
    } finally {
      setIsSaving(false);
      e.target.value = "";
    }
  };

  const handleDelete = async () => {
    if (!activeModule?.selectedCharacterId || isSaving)
      return;

    setIsSaving(true);
    setOperationError(false);

    try {

      const response = await deleteFromServerApiAsync(`api/characters/${activeModule.selectedCharacterId}`);

      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Deletion failed. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else{
        let module = {
          moduleName: "characters"
        } as SharedContextType;

        navigateTo(module);
      }

    } catch (error) {
      console.error("Deletion character error:", error);
      setOperationError(true);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteAvatar = async () => {
    if (!lightboxAvatar?.fileName || !activeModule?.selectedCharacterId || isDeletingAvatar)
      return;

    setIsDeletingAvatar(true);
    try {
      const response = await deleteFromServerApiAsync(
        `api/characters/${activeModule.selectedCharacterId}/avatars/${lightboxAvatar.fileName}`
      );
      const ex = response as ServerApiExceptionResponseDto | null;
      if (!response || ex?.message) {
        console.error(`Failed to delete avatar [${lightboxAvatar.fileName}].`);
        return;
      }

      // Remove from characterResponse in-place so the carousel updates immediately
      setCharacterResponse((prev) => {
        if (!prev?.character?.imageGenerationConfiguration?.illustrationMapOutfits) return prev;
        return {
          ...prev,
          character: {
            ...prev.character,
            imageGenerationConfiguration: {
              ...prev.character.imageGenerationConfiguration,
              illustrationMapOutfits: prev.character.imageGenerationConfiguration.illustrationMapOutfits.map((o) => ({
                ...o,
                sourceAvatars: (o.sourceAvatars ?? []).filter((a) => a.avatarFileName !== lightboxAvatar.fileName),
              })),
            },
          },
        };
      });

      setLightboxAvatar(null);
    } catch (err) {
      console.error("Delete avatar error:", err);
    } finally {
      setIsDeletingAvatar(false);
    }
  };

  const handleGenerateAvatar = async () => {
    if (isGeneratingAvatar) return;

    setIsGeneratingAvatar(true);
    try {
      const payload: CharacterMainAvatarIllustrationQueryRequestDto = {
        characterId: activeModule.selectedCharacterId ?? null,
        personaId: null,
        type: 0,
      };

      const response = await postToServerApiAsync("api/illustrator/queries", payload);
      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Failed to generate avatar. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else {
        console.log("Avatar generation query submitted successfully.");
      }
    } catch (error) {
      console.error("Generate avatar error:", error);
      setOperationError(true);
    } finally {
      setIsGeneratingAvatar(false);
    }
  };

  const handleGeneratePromptInjection = async () => {
    if (isGeneratingPromptInjection) return;

    setIsGeneratingPromptInjection(true);
    try {
      const payload = {
        characterId: activeModule.selectedCharacterId ?? null,
        outfit: OUTFIT_ENUM[selectedOutfit],
        type: 0, // IllustratorQueryType.CharacterMainAvatarGeneration
      };

      const response = await postToServerApiAsync("api/illustrator/promptInjection", payload);
      const serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || serverApiException?.message) {
        console.error(`Failed to generate prompt injection. [${JSON.stringify(serverApiException)}]`);
        setOperationError(true);
      } else {
        console.log("Prompt injection generated:", response);

        let typedResponse = response as GeneratePromptInjectionForMainCharacterAvatarResponseDto
        setOutfitPrompt(selectedOutfit, typedResponse.promptInjection ?? "");
      }
    } catch (error) {
      console.error("Generate prompt injection error:", error);
      setOperationError(true);
    } finally {
      setIsGeneratingPromptInjection(false);
    }
  };

  return (
    <div className={styles.characterDetailsComponent}>
      {isNetworkDown ? (
          <div className={styles.networkDownContainer}>
            <AiOutlineDisconnect className={styles.networkDownIcon} />
            <label>CohesiveRP backend is unreachable</label>
          </div>
        ) : (
        isLoadingCharacterDetails ? (
          <ImSpinner2 className={ styles.loadingCharacterDetailsSpinner } />
        ):(
          <div className={styles.characterDetailsWrapper}>
            <div className={styles.characterDetailsContainer}>
              <div className={styles.characterHeaderContainer}>
                <div className={styles.characterAvatarContainer}>
                  <img src={GetAvatarPathFromCharacterName(characterResponse?.character?.name ?? "")} alt="dev/Placeholder.png" onError={(e) => {
                    e.currentTarget.onerror = null;
                    e.currentTarget.src = GetFallbackEmpty();
                  }} />
                </div>
                <div className={styles.characterHeaderRightSideContainer}>
                  <textarea
                    className={styles.characterName}
                    value={characterName}
                    onChange={(e) => setCharacterName(e.target.value)}
                  />
                  <label className={styles.characterId}>{characterResponse?.character?.characterId ?? ""}</label>
                  <div className={styles.characterChatsDetailsContainer}>
                    <label className={styles.characterChatsDetailsChatsLabel}>Chats</label>
                    {isLoadingChatsDetails ? (
                      <ImSpinner2 className={ styles.loadingChatsDetailsSpinner } />
                    ):(
                      <div ref={chatsContainerRef} className={styles.chatsContainer}>
                        <div className={styles.chatAddNewChatContainer} onClick={async () => await handleCreateNewChatClick()}>
                          <MdAddBox className={styles.chatAddNewChatBtn} />
                        </div>
                        {chatsDetailsResponse?.chats?.map((chat, index) => (
                          <div key={index} className={styles.chatItem}>
                            <div className={styles.chatAvatarContainer} onClick={async () => await handleSpecificChatClick(chat)}>
                              <img src={GetAvatarPathFromChatId(chat.chatId)} alt="Avatar" onError={(e) => {
                                e.currentTarget.onerror = null;
                                e.currentTarget.src = GetFallbackEmpty();
                              }} />
                            </div>
                            <label className={styles.chatFootLabel}>{FormatDateTimeToMinutes(chat.lastActivityDateTime) ?? ""}</label>
                          </div>
                        ))}
                    </div>
                    )}
                  </div>
                </div>
              </div>

              {/* ── Tab bar ── */}
              <div className={styles.tabBar}>
                <button
                  className={`${styles.tabButton} ${activeTab === "info" ? styles.tabButtonActive : ""}`}
                  onClick={() => setActiveTab("info")}
                >
                  Info
                </button>
                <button
                  className={`${styles.tabButton} ${activeTab === "sheet" ? styles.tabButtonActive : ""}`}
                  onClick={() => setActiveTab("sheet")}
                >
                  Character Sheet
                </button>
              </div>

              {/* ── Tab content ── */}
              <div className={styles.characterDetailsBody}>

                {activeTab === "info" && (
                  <>
                    {/* ── JSON export / import ── */}
                    <div className={styles.jsonActionsContainer}>
                      <button
                        className={styles.jsonActionButton}
                        onClick={handleExportCharacterCard}
                        disabled={isSaving}
                        title="Export CharacterCard"
                      >
                        Export CharacterCard
                      </button>
                      <button
                        className={styles.jsonActionButton}
                        onClick={() => importCharacterCardRef.current?.click()}
                        disabled={isSaving}
                        title="Import CharacterCard (PNG)"
                      >
                        Import CharacterCard
                      </button>
                      <input
                        ref={importCharacterCardRef}
                        type="file"
                        accept="image/png,.png"
                        className={styles.hiddenFileInput}
                        onChange={handleImportCharacterCard}
                      />
                    </div>

                    <div className={styles.characterDescriptionContainer}>
                      <label className={styles.characterDescriptionLabel}>Description</label>
                      <textarea
                        className={styles.characterDescription}
                        value={characterDescription}
                        onChange={(e) => setCharacterDescription(e.target.value)}
                      />
                    </div>

                    <div className={styles.characterTagsContainer}>
                      <label className={styles.characterTagsLabel}>Tags</label>
                      <textarea
                        className={styles.characterTags}
                        value={tags?.join(",")}
                        onChange={(e) => setTags(e.target.value?.split(",")?.map(t => t.trim()))}
                      />
                    </div>

                    <div className={styles.characterFirstMessageContainer}>
                      <label className={styles.characterFirstMessageLabel}>First Message</label>
                      <textarea
                        className={styles.characterFirstMessage}
                        value={firstMessage}
                        onChange={(e) => setFirstMessage(e.target.value)}
                      />
                    </div>

                    <div className={styles.characterCreatorContainer}>
                      <label className={styles.characterCreatorLabel}>Creator</label>
                      <textarea
                        className={styles.characterCreator}
                        value={creator}
                        onChange={(e) => setCreator(e.target.value)}
                      />
                    </div>

                    <div className={styles.characterCreatorNotesContainer}>
                      <label className={styles.customLabel}>Creator Notes</label>
                      <textarea
                        className={styles.characterCreatorNotes}
                        value={creatorNotes}
                        onChange={(e) => setCreatorNotes(e.target.value)}
                      />
                    </div>

                    <hr className={styles.sectionDivider} />

                    <div className={styles.illustratorContainer}>
                      <label className={styles.customLabel}>Illustrator Tag</label>
                      <textarea
                        className={styles.illustratorPromptInjection}
                        style={{ minHeight: "1.3em", maxHeight: "3.2em" }}
                        value={illustratorTag}
                        onChange={(e) => setIllustratorTag(e.target.value)}
                      />
                    </div>

                    <div className={styles.illustratorContainer}>
                      <label className={styles.customLabel}>Outfit</label>
                      <select
                        className={styles.outfitSelect}
                        value={selectedOutfit}
                        onChange={(e) => setSelectedOutfit(e.target.value as OutfitKey)}
                      >
                        {OUTFIT_OPTIONS.map((o) => (
                          <option key={o} value={o}>{o.charAt(0).toUpperCase() + o.slice(1)}</option>
                        ))}
                      </select>
                    </div>

                    {/* ── Generate Prompt Injection ── */}
                    <div className={styles.illustratorContainer}>
                      <div className={styles.illustratorLabelRow}>
                        <label className={styles.customLabel}>Illustrator Prompt Injection</label>
                        <button
                          className={styles.promptInjectionAiButton}
                          onClick={handleGeneratePromptInjection}
                          disabled={isGeneratingPromptInjection || isSaving}
                          title="Auto-generate prompt injection"
                        >
                          {isGeneratingPromptInjection
                            ? <ImSpinner2 className={styles.saveSpinner} />
                            : <RiRobot2Fill />}
                        </button>
                      </div>
                      <textarea
                        className={styles.illustratorPromptInjection}
                        value={getOutfitEntry(selectedOutfit).illustratorPromptInjection}
                        onChange={(e) => setOutfitPrompt(selectedOutfit, e.target.value)}
                      />
                    </div>

                    {/* ── Generate Avatar ── */}
                    <div className={styles.illustratorContainer}>
                      <button
                        className={styles.generateAvatarButton}
                        onClick={handleGenerateAvatar}
                        disabled={isGeneratingAvatar || isSaving}
                        title="Generate avatar"
                      >
                        {isGeneratingAvatar
                          ? <ImSpinner2 className={styles.saveSpinner} />
                          : <FaImage />}
                      </button>
                    </div>

                    {(() => {
                      const avatars = characterResponse?.character?.imageGenerationConfiguration?.illustrationMapOutfits
                        ?.find((e) => (e.outfit ?? "").toLowerCase() === selectedOutfit.toLowerCase())
                        ?.sourceAvatars ?? [];
                      return avatars.length > 0 ? (
                        <div className={styles.avatarCarouselContainer}>
                          {avatars.map((ap, i) => (
                            <div key={i} className={styles.avatarCarouselItem} onClick={() => setLightboxAvatar({
                              filePath: ap.avatarFilePath ?? "",
                              fileName: ap.avatarFileName ?? "",
                              seed: ap.avatarSeed,
                            })}>
                              <img
                                src={GetAvatarPathFromAvatarFilePath(ap.avatarFilePath ?? "")}
                                alt={ap.avatarFileName ?? `Avatar ${i + 1}`}
                                onError={(e) => { e.currentTarget.style.opacity = "0.2"; }}
                              />
                              {ap.avatarSeed && (
                                <span className={styles.avatarCarouselSeed}>{ap.avatarSeed}</span>
                              )}
                            </div>
                          ))}
                        </div>
                      ) : null;
                    })()}
                  </>
                )}

                {activeTab === "sheet" && (
                  <CharacterSheetComponent 
                  key={sheetKey}
                  characterId={activeModule.selectedCharacterId}
                  personaId={null}
                  />
                )}
              </div>
            </div>

            {/* Save / Delete only shown on the info tab */}
            {activeTab === "info" && (
              <>
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
              </>
            )}
          </div>
        )
      )}

      {lightboxAvatar && (
        <div className={styles.lightboxOverlay} onClick={() => setLightboxAvatar(null)}>
          <div className={styles.lightboxCard} onClick={(e) => e.stopPropagation()}>
            <img
              className={styles.lightboxImage}
              src={GetAvatarPathFromAvatarFilePath(lightboxAvatar.filePath)}
              alt={lightboxAvatar.fileName}
              onError={(e) => { e.currentTarget.style.opacity = "0.2"; }}
            />
            <div className={styles.lightboxActions}>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxDeleteBtn}`}
                onClick={handleDeleteAvatar}
                disabled={isDeletingAvatar}
                title="Delete avatar"
              >
                {isDeletingAvatar ? <ImSpinner2 className={styles.lightboxSpinner} /> : <MdDelete />}
              </button>
              <button
                className={`${styles.lightboxBtn} ${styles.lightboxCloseBtn}`}
                onClick={() => setLightboxAvatar(null)}
                title="Close"
              >
                ✕
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
