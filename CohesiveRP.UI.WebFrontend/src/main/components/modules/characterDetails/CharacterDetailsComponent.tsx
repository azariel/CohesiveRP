import styles from "./CharacterDetailsComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import { deleteFromServerApiAsync, getFromServerApiAsync, postToServerApiAsync, putToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharacterResponseDto } from "../../../../ResponsesDto/characters/CharacterResponseDto";
import type { SharedContextCharacterType } from "../../../../store/SharedContextCharacterType";
import { GetAvatarPathFromCharacterId } from "../../../../utils/avatarUtils";
import { ImSpinner2 } from "react-icons/im";
import type { SelectableChatsResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatsResponseDto";
import type { SelectableChatResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatResponseDto";
import type { AddChatRequestDto } from "../../../../RequestDto/chat/AddChatRequestDto";
import { MdAddBox } from "react-icons/md";
import { FormatDateTimeToMinutes } from "../../../../utils/DateUtils";
import type { SharedContextChatType } from "../../../../store/SharedContextChatType";
import type { SharedContextType } from "../../../../store/SharedContextType";

export default function CharacterDetailsComponent() {
  const { activeModule } = sharedContext<SharedContextCharacterType>();
  const { navigateTo } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const chatsContainerRef = useRef<HTMLDivElement>(null);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isLoadingChat, setIsLoadingChat] = useState(false);
  const [isLoadingCharacterDetails, setIsLoadingCharacterDetails] = useState(true);
  const [isLoadingChatsDetails, setIsLoadingChatsDetails] = useState(true);
  const [characterResponse, setCharacterResponse] = useState<CharacterResponseDto | null>(null);
  const [chatsDetailsResponse, setChatsDetailsResponse] = useState<SelectableChatsResponseDto | null>(null);

  // saving state
  const [characterName, setCharacterName] = useState("");
  const [creatorNotes, setCreatorNotes] = useState("");
  const [tags, setTags] = useState<string[]>([]);
  const [characterDescription, setCharacterDescription] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [operationError, setOperationError] = useState(false);

  useEffect(() => {
    const el = chatsContainerRef.current;
    if (!el)
      return;

    const onWheel = (e: WheelEvent) => {
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
        setCreatorNotes(response?.character?.creatorNotes ?? "");
        setTags(response?.character?.tags ?? []);
        setCharacterDescription(response?.character?.description ?? "");
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
          defaultChatAvatar: chat.avatarCharacterId,
          moduleName: "chat",
          currentUserInputValue: savedInput,
        } as SharedContextChatType;

        navigateTo(selectedChat);
        console.log(`Chat selected -> Module [${JSON.stringify(selectedChat)}] selected.`);
        console.log(`defaultAvatar:${chat.avatarCharacterId}`);
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
        characterId: activeModule.selectedCharacterId
      };
      
      const response:SelectableChatResponseDto | null = await postToServerApiAsync<SelectableChatResponseDto>("api/chats", payload);

      // add the newly created chat to state
      if(response) {
        setChatsDetailsResponse(previousCollection => {
          if (!previousCollection) {
            return { chats: [response]} as SelectableChatsResponseDto;
          }
          return {
            ...previousCollection,// copy other fields
            chats: [...(previousCollection.chats || []), response],// add newly created chat on top of others
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
        creatorNotes,
        tags });

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

  return (
    <main className={styles.characterDetailsComponent}>
      {isNetworkDown ? (
          <div className={styles.networkDownContainer}>
            <AiOutlineDisconnect className={styles.networkDownIcon} />
            <label>CohesiveRP backend is unreachable</label>
          </div>
        ) : (
        isLoadingCharacterDetails ? (
          <ImSpinner2 className={ styles.loadingCharacterDetailsSpinner } />
        ):(
          <div className={styles.characterDetailsContainer}>
            <div className={styles.characterHeaderContainer}>
              <div className={styles.characterAvatarContainer}>
                <img src={GetAvatarPathFromCharacterId(characterResponse?.character?.characterId ?? "")} alt="dev/Placeholder.png" />
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
                            <img src={GetAvatarPathFromCharacterId(chat.avatarCharacterId)} alt="Avatar" />
                          </div>
                          <label className={styles.chatFootLabel}>{FormatDateTimeToMinutes(chat.lastActivityDateTime) ?? ""}</label>
                        </div>
                      ))}
                  </div>
                  )}
                </div>
              </div>
            </div>
            <div className={styles.characterDetailsBody}>
              <div className={styles.characterCreatorNotesContainer}>
                <label className={styles.characterCreatorNotesLabel}>Creator Notes</label>
                <textarea
                  className={styles.characterCreatorNotes}
                  value={creatorNotes}
                  onChange={(e) => setCreatorNotes(e.target.value)}
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

              <div className={styles.characterDescriptionContainer}>
                <label className={styles.characterDescriptionLabel}>Description</label>
                <textarea
                  className={styles.characterDescription}
                  value={characterDescription}
                  onChange={(e) => setCharacterDescription(e.target.value)}
                />
              </div>
            </div>
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
        )
      )}
    </main>
  );
}